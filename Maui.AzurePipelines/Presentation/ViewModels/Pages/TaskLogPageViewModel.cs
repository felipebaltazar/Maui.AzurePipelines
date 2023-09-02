using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Models;

namespace PipelineApproval.Presentation.ViewModels.Pages;

public class TaskLogPageViewModel : BaseViewModel, INavigationAware
{
    private readonly IAzureService _azureService;

    private ObservableRangeCollection<string> logs = new ObservableRangeCollection<string>();

    public string Organization { get; set; }

    public string Project { get; set; }

    public string PipelineId { get; set; }

    public string RunId { get; set; }

    public string LogId { get; set; }

    public ObservableRangeCollection<string> Logs
    {
        get => logs;
        set => SetProperty(ref logs, value);
    }

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

    public Task OnNavigatedFrom(IDictionary<string, string> parameters)
    {
        Logs.Clear();
        return Task.CompletedTask;
    }

    public Task OnNavigatedTo(IDictionary<string, string> parameters)
    {
        return ExecuteBusyActionOnNewTaskAsync(async () =>
        {
            var result = await _azureService.GetLogAsync(Organization, Project, RunId, LogId).ConfigureAwait(false);
            Logs.AddRange(result.value.Select(RemoveDateTime));
        });
    }

    private string RemoveDateTime(string line)
    {
        var spaceIndex = line.IndexOf(' ');
        return line[(spaceIndex + 1)..];
    }
}
