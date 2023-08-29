using Android.Widget;
using Java.Util;
using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Infrastructure;
using PipelineApproval.Infrastructure.Commands;
using PipelineApproval.Infrastructure.Extensions;
using PipelineApproval.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Uri = System.Uri;

namespace PipelineApproval.Presentation.ViewModels.Pages;

public class ProjectDetailsPageViewModel : BaseViewModel, INavigationAware
{
    private List<BuildOverview> originalList = new List<BuildOverview>();
    private ObservableRangeCollection<BuildOverview> pipelines;
    private TaskQueue taskQueue = new TaskQueue();

    private string continuationToken;
    private string company;
    private string credentials;
    private string project;
    private string searchText;
    private bool isLoading;

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

    //TODO: Verificar paginação
    public IAsyncCommand LoadMoreDataCommand =>
        new AsyncCommand(() => Task.Run(LoadPipelinesAsync), (a) => !IsLoading);

    public ProjectDetailsPageViewModel(
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

    public Task OnNavigatedFrom(IDictionary<string, string> parameters)
    {
        return Task.CompletedTask;
    }

    public async Task OnNavigatedTo(IDictionary<string, string> parameters)
    {
        var currentProject = parameters.GetValueOrDefault<Project>("Project");
        if (!parameters.TryGetValue("Organization", out company))
            return;

        var pat = await SecureStorage.GetAsync(Constants.Storage.PAT_TOKEN_KEY);
        credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", pat)));
        project = currentProject.name;
        await ExecuteBusyActionAsync(LoadPipelinesAsync).ConfigureAwait(false);
    }

    private async Task LoadPipelinesAsync()
    {
        try
        {
            IsLoading = true;

            if (!string.IsNullOrWhiteSpace(SearchText))
                return;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"https://dev.azure.com/{company}/{project}/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var continationQuery = !string.IsNullOrEmpty(continuationToken) ? $"&continuationToken={continuationToken}" : string.Empty;
                var response = await client.GetAsync("_apis/build/builds?api-version=7.0&$top=100&queryOrder=queueTimeDescending" + continationQuery);
                if (response.Headers.TryGetValues("x-ms-continuationtoken", out var values))
                {
                    continuationToken = values.FirstOrDefault();
                }

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = await response.Content.ReadFromJsonAsync<AzureApiResult<BuildOverview>>().ConfigureAwait(false);
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

                }
            }
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
}