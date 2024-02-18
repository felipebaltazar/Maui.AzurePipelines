using Maui.ServerDrivenUI;
using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Views;
using PipelineApproval.Infrastructure.Commands;
using PipelineApproval.Infrastructure.Extensions;
using PipelineApproval.Models;
using System.Text.Json;
using System.Web;
using Constants = PipelineApproval.Infrastructure.Constants;

namespace PipelineApproval.Presentation.ViewModels.Pages;

public class MainPageViewModel : BaseViewModel, IInitializeAware
{
    private readonly ISecureStorageService _secureStorageService;
    private readonly IPreferencesService _preferencesService;
    private readonly IAzureService _azureService;
    private readonly IServerDrivenUIService _serverDrivenUIService;
    private string _url;

    private string company;
    private string project;
    private string buildId;

    private Team selectedTeam;
    private AccountInfo selectedOrganization;
    private AccountResponseApi accountApiResponse;
    private IList<Team> teams;

    private List<Approval> approvals = new List<Approval>();
    private List<Project> projects = new List<Project>();

    private ObservableRangeCollection<Board> boards =
        new ObservableRangeCollection<Board>();

    private ObservableRangeCollection<Pipeline> pinnedPipelines
        = new ObservableRangeCollection<Pipeline>();

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

    public ObservableRangeCollection<Board> Boards
    {
        get => boards;
        set => SetProperty(ref boards, value);
    }

    public ObservableRangeCollection<Pipeline> PinnedPipelines
    {
        get => pinnedPipelines;
        set => SetProperty(ref pinnedPipelines, value);
    }

    public AccountInfo SelectedOrganization
    {
        get => selectedOrganization;
        set => SetProperty(ref selectedOrganization, value);
    }

    public Team SelectedTeam
    {
        get => selectedTeam;
        set => SetProperty(ref selectedTeam, value);
    }

    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    public IAsyncCommand ChangeOrganizationCommand =>
        new AsyncCommand(ChangeOrganizationCommandExecuteAsync);

    public IAsyncCommand ChangeTeamCommand =>
        new AsyncCommand(ChangeTeamCommandExecuteAsync);

    public IAsyncCommand LogoutCommand =>
        new AsyncCommand(LogoutCommandExecuteAsync);

    public MainPageViewModel(
        ILazyDependency<INavigationService> navigationService,
        ILazyDependency<ILoaderService> loaderService,
        ISecureStorageService secureStorageService,
        IPreferencesService preferencesService,
        IMainThreadService mainThreadService,
        IServerDrivenUIService serverDrivenUIService,
        IAzureService azureService,
        ILogger logger)
        : base(
            loaderService,
            navigationService,
            mainThreadService,
            logger)
    {
        _secureStorageService = secureStorageService;
        _preferencesService = preferencesService;
        _azureService = azureService;
        _serverDrivenUIService = serverDrivenUIService;
    }

    public async Task InitializeAsync(IDictionary<string, string> parameters)
    {
        //await _serverDrivenUIService.ClearCacheAsync();
        accountApiResponse = parameters.GetValueOrDefault<AccountResponseApi>(Constants.Navigation.ACCOUNT_PARAMETER);

        await ExecuteBusyActionAsync(async () =>
        {
            var selectedIndex = _preferencesService.Get("SelectedOrganization", 0);
            var selected = accountApiResponse.value.ElementAt(selectedIndex);

            SelectedOrganization = selected;

            await LoadAllAsync().ConfigureAwait(false);
        });
    }

    public async Task GoToPipelineCommandExecuteAsync()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            await DisplayAlertAsync("Erro", "Você precisa preencher o campo 'URL'");
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
            var parameters = overview.ToNavigationParameters("BuildOverview");

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
            Url = string.Empty;
            IsBusy = false;
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
                _preferencesService.Set("SelectedOrganization", index);
                _ = Task.Run(LoadAllAsync);
            };
        }).ConfigureAwait(false);
    }

    private async Task ChangeTeamCommandExecuteAsync()
    {
        await NavigationService.PushPopupAsync<ISelectTeamPopup>(p =>
        {
            p.Teams = teams;
            p.OnSelected = (s) =>
            {
                SelectedTeam = s;
                var index = teams.IndexOf(s);
                _preferencesService.Set(SelectedOrganization.accountName + "_SelectedTeam", index);
                _ = Task.Run(LoadBoardsAsync);
            };
        }).ConfigureAwait(false);
    }

    private async Task LogoutCommandExecuteAsync()
    {
        _preferencesService.Set(SelectedOrganization.accountName + "_SelectedTeam", 0)
                           .Set("SelectedOrganization", 0);

        await _secureStorageService.SetAsync(Constants.Storage.PAT_TOKEN_KEY, string.Empty).ConfigureAwait(false);
        await NavigationService.NavigateToAsync("LoginPage").ConfigureAwait(false);
    }

    private Task NavigateToProjectCommandExecuteAsync(Project project)
    {
        var objParameter = JsonSerializer.Serialize(project).ToNavigationParameters("Project");
        objParameter.Add("Organization", SelectedOrganization.accountName);
        return NavigationService.NavigateToAsync("/ProjectDetailsPage", objParameter);
    }

    private async Task LoadAllAsync()
    {
        await Task.WhenAll(LoadProjectsAsync(),
                           LoadBoardsAsync()).ConfigureAwait(false);
    }

    private async Task LoadProjectsAsync()
    {
        var apiResponse = await _azureService.GetProjectsAsync(selectedOrganization.accountName).ConfigureAwait(false);
        var projects = apiResponse.value
            .Select(p =>
            {
                p.NavigateToProjectCommand =
                        new AsyncCommand(() => ExecuteBusyActionOnNewTaskAsync(() => NavigateToProjectCommandExecuteAsync(p)));
                return p;
            });



        Projects = projects.ToList();
    }

    private async Task LoadBoardsAsync()
    {
        var selectedIndex = _preferencesService.Get(SelectedOrganization.accountName + "_SelectedTeam", 0);
        var result = await _azureService.GetTeamsAsync(SelectedOrganization.accountName).ConfigureAwait(false);

        teams = result.ToList();
        SelectedTeam = teams.ElementAt(selectedIndex);

        var boards = await _azureService.GetBoardsAsync(SelectedOrganization.accountName, SelectedTeam.ProjectName, SelectedTeam.Id).ConfigureAwait(false);
        Boards.ReplaceRange(boards);
    }

    private async Task LoadPinnedPipelinesAsync()
    {
        var pinnedPipelinesStr = _preferencesService.Get(SelectedOrganization.accountName + "_PinnedPipelines", "");
        if (string.IsNullOrWhiteSpace(pinnedPipelinesStr))
            return;

        //TODO: Listar pipelines pinadas
    }
}
