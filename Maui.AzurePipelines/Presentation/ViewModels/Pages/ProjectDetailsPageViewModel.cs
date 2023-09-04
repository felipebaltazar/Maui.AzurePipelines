using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Infrastructure.Commands;
using PipelineApproval.Infrastructure.Extensions;
using PipelineApproval.Models;

namespace PipelineApproval.Presentation.ViewModels.Pages;

public class ProjectDetailsPageViewModel : BaseViewModel, INavigationAware
{
    #region Fields

    private readonly IAzureService _azureService;

    private List<BuildOverview> originalList = new List<BuildOverview>();
    private ObservableRangeCollection<BuildOverview> pipelines;
    private TaskQueue taskQueue = new TaskQueue();

    private string company;
    private string project;
    private string searchText;
    private bool isLoading;

    #endregion

    #region Properties

    public ObservableRangeCollection<BuildOverview> Pipelines
    {
        get => pipelines;
        set => SetProperty(ref pipelines, value);
    }

    public string SearchText
    {
        get => searchText;
        set => SetProperty(ref searchText, value, OnSearchChanged);
    }

    public bool IsLoading
    {
        get => isLoading;
        set => SetProperty(ref isLoading, value);
    }

    public IAsyncCommand LoadMoreDataCommand =>
        new AsyncCommand(() => ExecuteBusyActionOnNewTaskAsync(()=> LoadPipelinesAsync()), (a) => !IsLoading);

    public IAsyncCommand<BuildOverview> PipelineSelectedCommand =>
        new AsyncCommand<BuildOverview>((b) => ExecuteBusyActionOnNewTaskAsync(() => NavigateToPipelineDetailsAsync(b)), (a) => !IsLoading);

    #endregion

    #region Constructors

    public ProjectDetailsPageViewModel(
        IAzureService azureService,
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
        _azureService = azureService;
    }

    #endregion

    #region INavigationAware

    public Task OnNavigatedFrom(IDictionary<string, string> parameters)
    {
        if (parameters.ContainsKey("NavigationMode"))
        {
            _azureService.ClearCache();

            pipelines.Clear();
            originalList.Clear();
            SearchText = string.Empty;
        }

        return Task.CompletedTask;
    }

    public async Task OnNavigatedTo(IDictionary<string, string> parameters)
    {
        var currentProject = parameters.GetValueOrDefault<Project>("Project");
        if (!parameters.TryGetValue("Organization", out company))
            return;

        project = currentProject.name;
        await ExecuteBusyActionAsync(LoadPipelinesAsync).ConfigureAwait(false);
    }

    #endregion

    #region Private Methods

    private async Task LoadPipelinesAsync()
    {
        try
        {
            IsLoading = true;

            if (!string.IsNullOrWhiteSpace(SearchText))
                return;

            var result = await _azureService.GetBuildsAsync(company, project).ConfigureAwait(false);
            var lastProject = result.value.LastOrDefault();

            if (Pipelines == null)
            {
                originalList = new List<BuildOverview>(result.value);
                Pipelines = new ObservableRangeCollection<BuildOverview>(result.value);
            }
            else
            {
                originalList.AddRange(Pipelines);
                Pipelines.AddRange(result.value);
            }
        }
        catch (Exception ex)
        {
            var teste = ex;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnSearchChanged()
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            Pipelines.ReplaceRange(originalList);
            return;
        }

        _ = taskQueue.Enqueue((ct) =>
        {
            return Task.Run(async () =>
            {
                await Task.Delay(500);

                if (ct.IsCancellationRequested)
                    return;

                var filtered = originalList.Where(p => p?.definition?.name?.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                if (ct.IsCancellationRequested)
                    return;

                MainThreadService.BeginInvokeOnMainThread(() => Pipelines.ReplaceRange(filtered));
            });
        }, true);

    }

    private async Task NavigateToPipelineDetailsAsync(BuildOverview selected)
    {
        var objParameter = selected.ToNavigationParameters("BuildOverview");
        objParameter.Add("Organization", company);
        objParameter.Add("Project", project);

        await NavigationService.NavigateToAsync("/PipelineDetailsPage", objParameter);
    }

    #endregion
}