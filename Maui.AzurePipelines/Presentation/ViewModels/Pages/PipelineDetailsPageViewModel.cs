using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Infrastructure.Extensions;
using PipelineApproval.Models;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PipelineApproval.Presentation.ViewModels.Pages;

public class PipelineDetailsPageViewModel : BaseViewModel, INavigationAware
{
    private const int PAGINATION = 20;

    private readonly IAzureService _azureService;

    private ObservableRangeCollection<Record> records =
        new ObservableRangeCollection<Record>();

    private ObservableRangeCollection<Approval> approvals =
        new ObservableRangeCollection<Approval>();

    private BuildOverview buildOverview;
    private Record selectedRecord;
    private List<Record> stages;

    private string organization;
    private string project;

    public BuildOverview BuildOverview
    {
        get => buildOverview;
        set => SetProperty(ref buildOverview, value);
    }

    public ObservableRangeCollection<Approval> Approvals
    {
        get => approvals;
        set => SetProperty(ref approvals, value);
    }

    public ObservableRangeCollection<Record> Records
    {
        get => records;
        set => SetProperty(ref records, value);
    }

    public Record SelectedRecord
    {
        get => selectedRecord;
        set => SetProperty(ref selectedRecord, value, OnSelectedRecordChanged);
    }

    public PipelineDetailsPageViewModel(
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

    public Task OnNavigatedFrom(IDictionary<string, string> parameters)
    {
        if (parameters.ContainsKey("NavigationMode"))
        {
            stages = null;
            Approvals.Clear();
            Records.Clear();
        }

        return Task.CompletedTask;
    }

    public async Task OnNavigatedTo(IDictionary<string, string> parameters)
    {
        try
        {
            project = parameters.GetValueOrDefault("Project");
            organization = parameters.GetValueOrDefault("Organization");
            BuildOverview = parameters.GetValueOrDefault<BuildOverview>("BuildOverview");

            await ExecuteBusyActionOnNewTaskAsync(LoadDataAsync).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var teste = ex;
        }
    }

    private void OnSelectedRecordChanged()
    {
        try
        {
            if (SelectedRecord == null || SelectedRecord?.log?.id == null)
                return;

            var parameters = new Dictionary<string, string>
            {
                { "LogId", selectedRecord.log.id.ToString() },
                { "Organization", organization },
                { "Project", project },
                { "RunId", buildOverview.id.ToString()}
            };

            NavigationService.NavigateToAsync("/TaskLogPage", parameters).SafeFireAndForget();
        }
        finally
        {

            SelectedRecord = null;
        }
    }

    private async Task LoadDataAsync()
    {
        var result = await _azureService.GetBuildTimeLineAsync(organization, project, buildOverview.id).ConfigureAwait(false);
        Approvals.AddRange(result.Approvals);

        var records = result.Records;
        var orderedRecord = records.OrderBy(r => r.order);
        var childs = orderedRecord.Where(r => !string.IsNullOrWhiteSpace(r.parentId))
                                  .GroupBy(r => r.parentId)
                                  .ToDictionary(r => r.Key, r => r.ToList());

        stages = orderedRecord.Where(r => string.IsNullOrWhiteSpace(r.parentId))
                                  .Select(r => GetChild(r, childs).Reverse())
                                  .SelectMany(r => r)
                                  .Where(s => s.type != "Checkpoint" && s.type != "Phase")
                                  .ToList();

        Records.AddRange(stages);
    }

    private IEnumerable<Record> GetChild(Record record, IDictionary<string, List<Record>> table)
    {
        if (record.IsExapanded && table.Remove(record.id, out var items))
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                foreach (var child in GetChild(item, table))
                {
                    yield return child;
                }
            }
        }

        yield return record;
    }

}
