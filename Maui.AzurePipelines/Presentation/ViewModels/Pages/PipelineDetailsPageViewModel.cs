using Microsoft.Extensions.Logging;
using Microsoft.Maui.Adapters;
using PipelineApproval.Abstractions;
using PipelineApproval.Infrastructure.Commands;
using PipelineApproval.Infrastructure.Extensions;
using PipelineApproval.Models;

namespace PipelineApproval.Presentation.ViewModels.Pages;

public class PipelineDetailsPageViewModel : BaseViewModel, INavigationAware
{
    #region Fields

    private readonly IAzureService _azureService;
    private readonly IAsyncCommand<Approval> _approvalCommand;

    private ObservableRangeCollection<Record> records =
        new ObservableRangeCollection<Record>();

    private ObservableRangeCollection<Approval> approvals =
        new ObservableRangeCollection<Approval>();

    private ItemPosition selectedRecordPosition;
    private BuildOverview buildOverview;
    private Record selectedRecord;
    private List<Record> stages;

    private IEnumerable<Approval> approvalsCache;
    private IEnumerable<Record> recordsCache;

    private string organization;
    private string project;

    #endregion

    #region Properties

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

    public ObservableCollectionAdapter<Record> Records
    {
        get => new ObservableCollectionAdapter<Record>(records);
    }

    public Record SelectedRecord
    {
        get => selectedRecord;
        set => SetProperty(ref selectedRecord, value, OnSelectedRecordChanged);
    }

    public ItemPosition SelectedRecordPosition
    {
        get => selectedRecordPosition;
        set => SetProperty(ref selectedRecordPosition, value);
    }

    #endregion

    #region Constructors

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
        _approvalCommand =
            new AsyncCommand<Approval>(ApproveCommandExecuteAsync);
    }

    #endregion

    #region INavigationAware

    public Task OnNavigatedFrom(IDictionary<string, string> parameters)
    {
        if (parameters.ContainsKey("NavigationMode"))
        {
            recordsCache = null;
            approvalsCache = null;
            stages = null;
            Approvals.Clear();
            records.Clear();
        }

        return Task.CompletedTask;
    }

    public async Task OnNavigatedTo(IDictionary<string, string> parameters)
    {
        project = parameters.GetValueOrDefault("Project");
        organization = parameters.GetValueOrDefault("Organization");
        BuildOverview = parameters.GetValueOrDefault<BuildOverview>("BuildOverview");

        await ExecuteBusyActionAsync(LoadDataAsync).ConfigureAwait(false);
    }

    #endregion

    #region Private Methods

    public Record GetRecordAt(int index) =>
        records.ElementAtOrDefault(index);

    private async Task ApproveCommandExecuteAsync(Approval elementToApprove)
    {
        if (string.IsNullOrEmpty(elementToApprove.Comment))
        {
            await DisplayAlertAsync("Erro", "Você precisa preencher o campo comentario!");
            return;
        }

        var newUpdate = new ApprovalUpdate()
        {
            approvalId = elementToApprove.id,
            status = "approved",
            comments = elementToApprove.Comment
        };

        (var isSuccess, var responseText) = await _azureService.ApproveAsync(organization, project, newUpdate);

        if (isSuccess)
        {
            await DisplayAlertAsync("Sucesso", "Aprovado com sucesso!");
        }
        else
        {
            await DisplayAlertAsync("Erro na request",
                                    $"Não foi possível aprovar pipeline, tente novamente.\nResponse:{responseText}");
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            if (recordsCache == null || approvalsCache == null)
            {
                var result = await _azureService.GetBuildTimeLineAsync(organization, project, buildOverview.id).ConfigureAwait(false);
                recordsCache = result.Records;
                approvalsCache = result.Approvals;
            }

            var approvals = approvalsCache.Where(a => a.status != "approved").Select(SetApprovalCommand);
            MainThreadService.BeginInvokeOnMainThread(() => Approvals.AddRange(approvals));

            var orderedRecord = recordsCache.OrderBy(r => r.order);
            var childs = orderedRecord.Where(r => !string.IsNullOrWhiteSpace(r.parentId))
                                      .GroupBy(r => r.parentId)
                                      .ToDictionary(r => r.Key, r => r.ToList());

            stages = orderedRecord.Where(r => string.IsNullOrWhiteSpace(r.parentId))
                                      .Select(r => GetChild(r, childs).Reverse())
                                      .SelectMany(r => r)
                                      .Where(s => s.type != "Checkpoint" && s.type != "Phase")
                                      .ToList();

            MainThreadService.BeginInvokeOnMainThread(() =>
            {
                records.ReplaceRange(stages);
                Records.InvalidateData();
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, nameof(PipelineDetailsPageViewModel) + "." + nameof(LoadDataAsync));
            await DisplayAlertAsync("Erro", "Não foi possível carregar dados sobre o build.");
        }
    }

    private void OnSelectedRecordChanged()
    {
        try
        {
            if (SelectedRecord == null)
                return;

            if (!SelectedRecord.IsExpanded)
            {
                SelectedRecord.IsExpanded = true;
                _ = Task.Run(async () => await ExecuteBusyActionAsync(LoadDataAsync));
            }
            else if (SelectedRecord?.log?.id != null)
            {
                NavigateToLogDetails();
            }
        }
        finally
        {
            SelectedRecord = null;
        }
    }

    private void NavigateToLogDetails()
    {
        var parameters = new Dictionary<string, string>
            {
                { "LogId", selectedRecord.log.id.ToString() },
                { "Organization", organization },
                { "Project", project },
                { "RunId", buildOverview.id.ToString()},
                { "TaskName", selectedRecord.name}
            };

        NavigationService.NavigateToAsync("/TaskLogPage", parameters).SafeFireAndForget();
    }

    private IEnumerable<Record> GetChild(Record record, IDictionary<string, List<Record>> table)
    {
        if (record.IsExpanded && table.Remove(record.id, out var items))
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

    private Approval SetApprovalCommand(Approval approval)
    {
        approval.ApproveCommand = _approvalCommand;
        return approval;
    }

    #endregion
}
