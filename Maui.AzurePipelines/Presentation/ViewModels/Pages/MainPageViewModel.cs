using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Views;
using PipelineApproval.Infrastructure.Commands;
using PipelineApproval.Infrastructure.Extensions;
using PipelineApproval.Models;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Web;
using Constants = PipelineApproval.Infrastructure.Constants;

namespace PipelineApproval.Presentation.ViewModels.Pages;

public class MainPageViewModel : BaseViewModel, IInitializeAware
{
    private string _pat;
    private string _url;

    private string company;
    private string project;
    private string buildId;

    private AccountInfo selectedOrganization;
    private AccountResponseApi accountApiResponse;

    private List<Approval> approvals = new List<Approval>();
    private List<Project> projects = new List<Project>();

    public List<Approval> Approvals
    {
        get => approvals;
        set => SetProperty(ref approvals, value);
    }

    public List<Project> Projects
    {
        get => projects;
        set => SetProperty(ref projects, value);
    }

    public AccountInfo SelectedOrganization
    {
        get => selectedOrganization;
        set => SetProperty(ref selectedOrganization, value);
    }

    public string PAT
    {
        get => _pat;
        set => SetProperty(ref _pat, value);
    }

    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    public IAsyncCommand ChangeOrganizationCommand =>
        new AsyncCommand(ChangeOrganizationCommandExecuteAsync);

    public MainPageViewModel(
        ILazyDependency<ILoaderService> loaderService,
        ILazyDependency<INavigationService> navigationService,
        IMainThreadService mainThreadService,
        ILogger logger)
        : base(
            loaderService,
            navigationService,
            mainThreadService,
            logger)
    {
    }

    public async Task InitializeAsync(IDictionary<string, string> parameters)
    {
        accountApiResponse = parameters.GetValueOrDefault<AccountResponseApi>(Constants.Navigation.ACCOUNT_PARAMETER);

        await ExecuteBusyActionAsync(async () =>
        {
            var pat = await SecureStorage.GetAsync(Constants.Storage.PAT_TOKEN_KEY).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(pat))
            {
                PAT = pat;
            }

            var selectedIndex = Preferences.Get("SelectedOrganization", 0);
            var selected = accountApiResponse.value.ElementAt(selectedIndex);

            SelectedOrganization = selected;

            await LoadProjects().ConfigureAwait(false);
        });
    }

    public async Task LoadDataAsync()
    {
        if (string.IsNullOrWhiteSpace(Url) || string.IsNullOrWhiteSpace(PAT))
        {
            await DisplayAlertAsync("Erro", "Você precisa preencher os campos 'URL da pipe' e 'Personal AccessToken'!");
            return;
        }

        try
        {
            IsBusy = true;

            var uri = new Uri(Url);
            if (uri.Authority.IndexOf("dev.azure.com") >= 0)
            {
                company = uri.Segments[1]
                    .Replace("/", "")
                    .Trim();

                project = uri.Segments[2]
                    .Replace("/", "")
                    .Trim();
            }
            else
            {
                company = uri.Authority
                    .Replace("/", "")
                    .Replace(".visualstudio.com", "")
                    .Trim();

                project = uri.Segments[1]
                    .Replace("/", "")
                    .Trim();
            }

            buildId = HttpUtility.ParseQueryString(uri.Query).Get("buildId");
            var credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", PAT)));

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"https://dev.azure.com/{company}/{project}/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var response = await client.GetAsync($"_apis/build/builds/{buildId}/timeline?api-version=6.0");
                response.EnsureSuccessStatusCode();

                var apiresponse = await response.Content.ReadFromJsonAsync<BuildIdReponse>();
                var recordTasks = new List<Task<(HttpResponseMessage, Record)>>();

                foreach (var record in apiresponse.records)
                {
                    if (record.type == "Checkpoint.Approval")
                    {
                        var checkpointRecord = apiresponse.records.FirstOrDefault(r => r.id == record.parentId);
                        var stageRecord = apiresponse.records.FirstOrDefault(r => r.id == checkpointRecord.parentId);
                        async Task<(HttpResponseMessage, Record)> getApprovalAsync()
                        {
                            var result = await client.GetAsync($"_apis/pipelines/approvals/{record.id}?$expand=steps&$expand=permissions&api-version=7.0-preview.1").ConfigureAwait(false);
                            return (result, stageRecord);
                        }

                        recordTasks.Add(getApprovalAsync());
                    }
                }

                var recordTasksResult = await Task.WhenAll(recordTasks).ConfigureAwait(false);

                var readTasks = recordTasksResult
                    .Where(t => t.Item1.IsSuccessStatusCode)
                    .Select(t =>
                    {
                        async Task<Approval> extractApproval()
                        {
                            var approval = await t.Item1.Content.ReadFromJsonAsync<Approval>();
                            approval.ApproveCommand = new AsyncCommand<Approval>(ApproveCommandExecuteAsync);
                            approval.stageRecord = t.Item2;
                            return approval;
                        }

                        return extractApproval();
                    });

                var approvals = await Task.WhenAll(readTasks).ConfigureAwait(false);
                Approvals = approvals.ToList();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro na request",
                $"Não foi possível recuperar dados da pipeline, tente novamente.\nException:{ex.Message}\nStackTrace: {ex.StackTrace}");

            Approvals = new List<Approval>(0);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadProjects()
    {
        var credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", PAT)));

        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri($"https://dev.azure.com/{selectedOrganization.accountName}/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await client.GetAsync($"_apis/projects?api-version=7.0");
            response.EnsureSuccessStatusCode();

            var apiresponse = await response.Content.ReadFromJsonAsync<ProjectsResponseApi>();

            Projects = apiresponse.value.ToList();
        }
    }

    private async Task ChangeOrganizationCommandExecuteAsync()
    {
        await NavigationService.PushPopupAsync<ISelectOrganizationPopup>(p =>
        {
            p.Organizations = accountApiResponse.value;
            p.OnSelected = (s) =>
            {
                SelectedOrganization = s;
                var index = Array.IndexOf(accountApiResponse.value, s);
                Preferences.Set("SelectedOrganization", index);
                _ = Task.Run(LoadProjects);
            };
        }).ConfigureAwait(false);
    }

    private async Task ApproveCommandExecuteAsync(Approval elementToApprove)
    {
        if (string.IsNullOrEmpty(elementToApprove.Comment))
        {
            await DisplayAlertAsync("Erro", "Você precisa preencher o campo comentario!");
            return;
        }

        var credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", PAT)));
        var approvals = new List<Approval>();

        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri($"https://dev.azure.com/{company}/{project}/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var newUpdate = new ApprovalUpdate()
            {
                approvalId = elementToApprove.id,
                status = "approved",
                comments = elementToApprove.Comment
            };

            var body = new[]
            {
                    newUpdate
                };

            var response = await client.PatchAsJsonAsync($"_apis/pipelines/approvals?api-version=7.1-preview.1", body);

            if (response.IsSuccessStatusCode)
            {
                await DisplayAlertAsync("Sucesso", "Aprovado com sucesso!");
                await LoadDataAsync();
            }
            else
            {
                var responseText = await response.Content.ReadAsStringAsync();
                await DisplayAlertAsync("Erro na request",
                                        $"Não foi possível aprovar pipeline, tente novamente.\nResponse:{responseText}");
            }
        }
    }

    private Task DisplayAlertAsync(string title, string message, string cancelButton = "Entendi!")
    {
        return NavigationService.PushPopupAsync<IAlertPopup>(p =>
        {
            p.MessageTitle = title;
            p.Message = message;
            p.CancelButton = cancelButton;
        });
    }

}
