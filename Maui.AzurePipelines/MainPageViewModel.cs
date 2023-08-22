using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Web;
using System.Windows.Input;

namespace PipelineApproval;

public class MainPageViewModel : BindableObject
{
    private List<Approval> _approvals = new List<Approval>();
    private Func<string, string, string, Task> _displayAlertAction;

    private string _pat;
    private string _url;
    private bool _isBusy;

    private string company;
    private string project;
    private string buildId;

    public List<Approval> Approvals
    {
        get => _approvals;
        set
        {
            if (value != _approvals)
            {
                _approvals = value;
                TryInvokeOnMainThread(() => OnPropertyChanged(nameof(Approvals)));
            }

        }
    }

    public string PAT
    {
        get => _pat;
        set
        {
            if (value != _pat)
            {
                _pat = value;
                TryInvokeOnMainThread(() => OnPropertyChanged(nameof(PAT)));
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (value != _isBusy)
            {
                _isBusy = value;
                TryInvokeOnMainThread(() => OnPropertyChanged(nameof(IsBusy)));
            }

        }
    }

    public string Url
    {
        get => _url;
        set
        {
            if (value != _url)
            {
                _url = value;

                TryInvokeOnMainThread(() => OnPropertyChanged(nameof(Url)));
            }

        }
    }

    public async Task InitializeAsync()
    {
        try
        {
            IsBusy = true;

            var pat = await SecureStorage.GetAsync("personal_access_token");
            if (!string.IsNullOrWhiteSpace(pat))
            {
                PAT = pat;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public MainPageViewModel(Func<string, string, string, Task> displayAlertAction)
    {
        _displayAlertAction = displayAlertAction;
    }


    public async Task OnApperingAsync()
    {
        if (string.IsNullOrWhiteSpace(Url) || string.IsNullOrWhiteSpace(PAT))
        {
            await _displayAlertAction.Invoke("Erro", "Você precisa preencher os campos 'URL da pipe' e 'Personal AccessToken'!", "Entendi!");
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
                            approval.ApproveCommand = new Command(ApproveCommandExecute);
                            approval.stageRecord = t.Item2;
                            return approval;
                        }

                        return extractApproval();
                    });

                var approvals = await Task.WhenAll(readTasks).ConfigureAwait(false);

                Approvals = approvals.ToList();
            }

            await SecureStorage.SetAsync("personal_access_token", PAT).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await _displayAlertAction.Invoke("Erro na request", $"Não foi possível recuperar dados da pipeline, tente novamente.\nException:{ex.Message}\nStackTrace: {ex.StackTrace}", "Entendi!");
            Approvals = new List<Approval>(0);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void ApproveCommandExecute(object result)
    {
        await ApproveCommandExecuteAsync(result);
    }

    private async Task ApproveCommandExecuteAsync(object result)
    {
        if (result is Approval elementToApprove)
        {

            if (string.IsNullOrEmpty(elementToApprove.Comment))
            {
                await _displayAlertAction.Invoke("Erro", "Você precisa preencher o campo comentario!", "Entendi!");
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
                    await _displayAlertAction.Invoke("Sucesso",
                        $"Aprovado com sucesso!",
                        "Entendi!");

                    await OnApperingAsync();
                }
                else
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    await _displayAlertAction.Invoke("Erro na request",
                                                $"Não foi possível aprovar pipeline, tente novamente.\nResponse:{responseText}",
                                                "Entendi!");
                }
            }
        }
    }

    private void TryInvokeOnMainThread(Action action)
    {
        if (MainThread.IsMainThread)
            action.Invoke();

        else
            MainThread.BeginInvokeOnMainThread(action);
    }
}

public class ApprovalUpdate
{
    public string approvalId { get; set; }
    public string status { get; set; }
    public string comments { get; set; }
}


public class BuildIdReponse
{
    public Record[] records { get; set; }
    public string lastChangedBy { get; set; }
    public DateTime lastChangedOn { get; set; }
    public string id { get; set; }
    public int changeId { get; set; }
    public string url { get; set; }
}

public class Record
{
    public object[] previousAttempts { get; set; }
    public string id { get; set; }
    public string parentId { get; set; }
    public string type { get; set; }
    public string name { get; set; }
    public DateTime? startTime { get; set; }
    public DateTime? finishTime { get; set; }
    public object currentOperation { get; set; }
    public int? percentComplete { get; set; }
    public string state { get; set; }
    public string result { get; set; }
    public string resultCode { get; set; }
    public int changeId { get; set; }
    public DateTime lastModified { get; set; }
    public string workerName { get; set; }
    public int order { get; set; }
    public Details details { get; set; }
    public int errorCount { get; set; }
    public int warningCount { get; set; }
    public object url { get; set; }
    public Log log { get; set; }
    public PipelineTask task { get; set; }
    public int attempt { get; set; }
    public string identifier { get; set; }
    public int queueId { get; set; }
    public Issue[] issues { get; set; }
}

public class Details
{
    public string id { get; set; }
    public int changeId { get; set; }
    public string url { get; set; }
}

public class Log
{
    public int id { get; set; }
    public string type { get; set; }
    public string url { get; set; }
}

public class PipelineTask
{
    public string id { get; set; }
    public string name { get; set; }
    public string version { get; set; }
}

public class Issue
{
    public string type { get; set; }
    public string category { get; set; }
    public string message { get; set; }
    public Data data { get; set; }
}

public class Data
{
    public string logFileLineNumber { get; set; }
    public string type { get; set; }
}


public class Approval
{
    public Record stageRecord { get; set; }

    public string id { get; set; }
    public Step[] steps { get; set; }
    public string status { get; set; }
    public DateTime createdOn { get; set; }
    public DateTime lastModifiedOn { get; set; }
    public string executionOrder { get; set; }
    public int minRequiredApprovers { get; set; }
    public object[] blockedApprovers { get; set; }
    public string permissions { get; set; }
    public _Links _links { get; set; }
    public Pipeline pipeline { get; set; }
    public string Comment { get; set; }
    public ICommand ApproveCommand { get; set; }
}

public class Self
{
    public string href { get; set; }
}

public class Pipeline
{
    public Owner owner { get; set; }
    public string id { get; set; }
    public string name { get; set; }
}

public class Owner
{
    public _Links _links { get; set; }
    public int id { get; set; }
    public string name { get; set; }
}

public class _Links
{
    public Web web { get; set; }
    public Self self { get; set; }
}

public class Web
{
    public string href { get; set; }
}


public class Step
{
    public Assignedapprover assignedApprover { get; set; }
    public string status { get; set; }
    public DateTime lastModifiedOn { get; set; }
    public int order { get; set; }
    public Lastmodifiedby lastModifiedBy { get; set; }
    public DateTime initiatedOn { get; set; }
    public object[] history { get; set; }
}

public class Assignedapprover
{
    public string displayName { get; set; }
    public string id { get; set; }
    public string uniqueName { get; set; }
    public string descriptor { get; set; }
}

public class Lastmodifiedby
{
    public string displayName { get; set; }
    public string id { get; set; }
    public string uniqueName { get; set; }
    public string descriptor { get; set; }
}
