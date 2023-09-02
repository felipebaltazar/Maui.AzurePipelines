using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Views;
using PipelineApproval.Infrastructure.Commands;
using PipelineApproval.Infrastructure.Extensions;
using PipelineApproval.Models;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;
using Constants = PipelineApproval.Infrastructure.Constants;

namespace PipelineApproval.Presentation.ViewModels.Pages;

public class MainPageViewModel : BaseViewModel, IInitializeAware
{
    private readonly IAzureService _azureService;

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
        IAzureService azureService,
        ILogger logger)
        : base(
            loaderService,
            navigationService,
            mainThreadService,
            logger)
    {
        _azureService = azureService;
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

    public async Task GoToPipelineAsync()
    {
        if (string.IsNullOrWhiteSpace(Url) || string.IsNullOrWhiteSpace(PAT))
        {
            await DisplayAlertAsync("Erro", "Você precisa preencher o campo 'URL da pipe'");
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
            
            var overview = await _azureService.GetBuildAsync(company, project, buildId).ConfigureAwait(false);
            var parameters =  overview.ToNavigationParameters("BuildOverview");

            parameters.Add("Project", project);
            parameters.Add("Organization", company);

            await NavigationService.NavigateToAsync("/PipelineDetailsPage", parameters);
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
            var baseUrl = $"https://dev.azure.com/{selectedOrganization.accountName}/";
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await client.GetAsync($"_apis/projects?api-version=7.0&getDefaultTeamImageUrl=true");
            response.EnsureSuccessStatusCode();

            var apiresponse = await response.Content.ReadFromJsonAsync<ProjectsResponseApi>();
            var imagesTasks = apiresponse.value
                .Select(p =>
                {
                    p.NavigateToProjectCommand =
                            new AsyncCommand(() => ExecuteBusyActionOnNewTaskAsync(() => NavigateToProjectCommandExecuteAsync(p)));

                    async Task<(HttpResponseMessage, Project)> downloadImage()
                    {
                        try
                        {
                            var teste = p.defaultTeamImageUrl.Replace(baseUrl, string.Empty);
                            var result = await client.GetAsync(p.defaultTeamImageUrl.Replace(baseUrl, string.Empty)).ConfigureAwait(false);
                            return (result, p);
                        }
                        catch (Exception)
                        {
                        }

                        return (null, p);
                    }

                    return downloadImage();
                });

            var httpResponses = await Task.WhenAll(imagesTasks).ConfigureAwait(false);
            var filePaths = httpResponses.Select(SaveImageAsync);
            var projects = await Task.WhenAll(filePaths).ConfigureAwait(false);

            Projects = projects.ToList();
        }
    }

    private async Task<Project> SaveImageAsync((HttpResponseMessage message, Project project) result)
    {
        if (result.message == null)
            return result.project;

        try
        {
            var hash = GenerateHash(result.project.defaultTeamImageUrl);
            var cacheDir = Path.Combine(FileSystem.Current.CacheDirectory, "images");
            var finalPath = Path.Combine(cacheDir, hash + ".png");

            if (!Directory.Exists(cacheDir))
                Directory.CreateDirectory(cacheDir);

            if (!File.Exists(finalPath))
            {
                var apiResult = await result.message.Content.ReadFromJsonAsync<ImageApiResult>();
                File.WriteAllBytes(finalPath, Convert.FromBase64String(apiResult.imageData));
            }

            result.project.TeamImageFile = finalPath;

            return result.project;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Não foi possivel baixar imagem");
        }

        return result.project;
    }

    public string GenerateHash(string text)
    {
        using (var sha = SHA256.Create())
        {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));

            var hexChars = bytes
                .Select(b => b.ToString("X2"));

            return string.Concat(hexChars);
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
                await GoToPipelineAsync();
            }
            else
            {
                var responseText = await response.Content.ReadAsStringAsync();
                await DisplayAlertAsync("Erro na request",
                                        $"Não foi possível aprovar pipeline, tente novamente.\nResponse:{responseText}");
            }
        }
    }

    private Task NavigateToProjectCommandExecuteAsync(Project project)
    {
        var objParameter = JsonSerializer.Serialize(project).ToNavigationParameters("Project");
        objParameter.Add("Organization", SelectedOrganization.accountName);
        return NavigationService.NavigateToAsync("/ProjectDetailsPage", objParameter);
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
