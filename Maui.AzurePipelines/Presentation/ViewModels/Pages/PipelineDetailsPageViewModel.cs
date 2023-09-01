using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Infrastructure.Extensions;
using PipelineApproval.Models;

namespace PipelineApproval.Presentation.ViewModels.Pages;

public class PipelineDetailsPageViewModel : BaseViewModel, INavigationAware
{
    private readonly IAzureService _azureService;

    private ObservableRangeCollection<Record> records =
        new ObservableRangeCollection<Record>();

    private ObservableRangeCollection<Approval> approvals =
        new ObservableRangeCollection<Approval>();

    private BuildOverview buildOverview;

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

    private async Task LoadDataAsync()
    {
        var result = await _azureService.GetBuildTimeLineAsync(organization, project, buildOverview.id).ConfigureAwait(false);
        var records = result.Records;
        
        var orderedRecord = records.OrderBy(r => r.order);

        var stages = orderedRecord.Where(r => string.IsNullOrWhiteSpace(r.parentId))
                                  .Select(r => GetChild(r, orderedRecord))
                                  .SelectMany(r => r);

        Records.AddRange(stages.Where(s=> s.type != "Checkpoint" && s.type != "Phase"));
        Approvals.AddRange(result.Approvals);
    }

    IEnumerable<Record> GetChild(Record record, IEnumerable<Record> table)
    {
        return new[] { record }.Union(table
                    .Where(x => x.parentId == record.id)
                    .Union(table.Where(x => x.parentId == record.id)
                        .SelectMany(y => GetChild(y, table))));
    }
    
}
