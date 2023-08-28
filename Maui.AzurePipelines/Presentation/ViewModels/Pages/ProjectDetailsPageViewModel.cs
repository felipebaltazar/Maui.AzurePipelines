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
    private ObservableRangeCollection<PipelineOverview> pipelines;
    private string continuationToken;
    private string company;
    private string credentials;
    private string project;

    public ObservableRangeCollection<PipelineOverview> Pipelines
    {
        get => pipelines;
        set => SetProperty(ref pipelines, value);
    }

    public IAsyncCommand LoadMoreDataCommand =>
        new AsyncCommand(LoadPipelinesAsync);

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
        await LoadPipelinesAsync().ConfigureAwait(false);
    }

    private async Task LoadPipelinesAsync()
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri($"https://dev.azure.com/{company}/{project}/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var continationQuery = !string.IsNullOrEmpty(continuationToken) ? $"&continuationToken={continuationToken}" : string.Empty;
            var response = await client.GetAsync("_apis/pipelines?api-version=7.0&$top=200" + continationQuery);
            if (response.Headers.TryGetValues("x-ms-continuationtoken", out var values))
            {
                continuationToken = values.FirstOrDefault();
            }

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var teste = await response.Content.ReadAsStringAsync();
                    var result = await response.Content.ReadFromJsonAsync<AzureApiResult<PipelineOverview>>().ConfigureAwait(false);
                    if (Pipelines == null)
                    {
                        Pipelines = new ObservableRangeCollection<PipelineOverview>(result.value);
                    }
                    else
                    {
                        Pipelines.AddRange(result.value);
                    }
                }
                catch (Exception ex)
                {

                    var teste = ex ;
                }
                
            }
        }
    }
}
