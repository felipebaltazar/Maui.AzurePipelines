using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Models;

namespace PipelineApproval.Presentation.ViewModels.Pages;

public class TaskLogPageViewModel : BaseViewModel, INavigationAware
{
    #region Fields

    private readonly IAzureService _azureService;

    private ObservableRangeCollection<TaskLog> logs =
        new ObservableRangeCollection<TaskLog>();

    #endregion

    #region Properties

    public string Organization { get; set; }

    public string Project { get; set; }

    public string PipelineId { get; set; }

    public string RunId { get; set; }

    public string LogId { get; set; }

    public ObservableRangeCollection<TaskLog> Logs
    {
        get => logs;
        set => SetProperty(ref logs, value);
    }

    #endregion

    #region Constructors

    public TaskLogPageViewModel(
        IAzureService azureService,
        ILazyDependency<ILoaderService> loaderService,
        ILazyDependency<INavigationService> navigationService,
        IMainThreadService mainThreadService, ILogger logger)
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
            Logs.Clear();
        }

        return Task.CompletedTask;
    }

    public Task OnNavigatedTo(IDictionary<string, string> parameters)
    {
        return ExecuteBusyActionOnNewTaskAsync(async () =>
        {
            var result = await _azureService.GetLogAsync(Organization, Project, RunId, LogId).ConfigureAwait(false);
            Logs.AddRange(result.value.Select(s => new TaskLog(s)));
        });
    }

    #endregion
}
