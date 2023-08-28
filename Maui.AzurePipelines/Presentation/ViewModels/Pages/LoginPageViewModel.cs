using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Views;
using PipelineApproval.Infrastructure;
using PipelineApproval.Infrastructure.Commands;
using PipelineApproval.Infrastructure.Extensions;
using PipelineApproval.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace PipelineApproval.Presentation.ViewModels.Pages;

public class LoginPageViewModel : BaseViewModel, INavigationAware
{
    #region Fields

    private readonly IBrowserService _browserService;
    private string pat;
    private bool isReady;

    #endregion

    #region Properties

    public bool IsReady
    {
        get => isReady;
        set => SetProperty(ref isReady, value);
    }

    public string PAT
    {
        get => pat;
        set => SetProperty(ref pat, value);
    }

    public IAsyncCommand LoginCommand =>
        new AsyncCommand(() => ExecuteBusyActionOnNewTaskAsync(LoginCommandExecuteAsync));

    public IAsyncCommand DocumentationCommand =>
        new AsyncCommand(DocumentationCommandExecuteAsync);

    public IAsyncCommand GithubrepositoryCommand =>
        new AsyncCommand(GithubrepositoryCommandExecuteAsync);

    #endregion

    #region Constructors

    public LoginPageViewModel(
       IBrowserService browserService,
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
        _browserService = browserService;
    }

    #endregion

    #region INavigationAware

    public Task OnNavigatedTo(IDictionary<string, string> parameters)
    {
        return ExecuteBusyActionAsync(async () =>
        {
            try
            {
                var pat = await SecureStorage.GetAsync(Constants.Storage.PAT_TOKEN_KEY).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(pat))
                {
                    IsReady = true;
                    return;
                }

                var organizationsStr = Preferences.Get(Constants.Storage.USER_ORGANIZATIONS, string.Empty);
                if (string.IsNullOrWhiteSpace(organizationsStr))
                {
                    IsReady = true;
                    return;
                }

                var accountParameters = organizationsStr.ToNavigationParameters(Constants.Navigation.ACCOUNT_PARAMETER);
                await NavigationService.NavigateToAsync("MainPage", accountParameters).ConfigureAwait(false);
            }
            finally
            {
                IsReady = true;
            }
        });
    }

    public Task OnNavigatedFrom(IDictionary<string, string> parameters)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region Private Methods

    private async Task LoginCommandExecuteAsync()
    {
        await Task.Yield();

        try
        {
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", PAT)));
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"https://app.vssps.visualstudio.com/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                var response = await client.GetAsync("_apis/profile/profiles/me?api-version=5.1").ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var accountInfo = await response.Content.ReadFromJsonAsync<AccountInfo>().ConfigureAwait(false);
                    var id = accountInfo.id;

                    response = await client.GetAsync($"_apis/accounts?api-version=5.1&memberId={id}").ConfigureAwait(false);

                    var apiResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    await SecureStorage.SetAsync(Constants.Storage.PAT_TOKEN_KEY, PAT).ConfigureAwait(false);
                    Preferences.Set(Constants.Storage.USER_ORGANIZATIONS, apiResponse);

                    var navParameters = apiResponse.ToNavigationParameters(Constants.Navigation.ACCOUNT_PARAMETER);
                    await NavigationService.NavigateToAsync("MainPage", navParameters).ConfigureAwait(false);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Não foi possível efetuar login");
        }

        await NavigationService.PushPopupAsync<IAlertPopup>(p =>
        {
            p.MessageTitle = "Erro de autenticação";
            p.Message = "Não foi possivel autenticar, verifique se o 'Personal Access Token' é valido.";
            p.CancelButton = "Ok";
        }).ConfigureAwait(false);

    }

    private Task DocumentationCommandExecuteAsync()
    {
        var uri = new Uri(Constants.Url.PAT_DOCUMENTATION);
        return _browserService.OpenAsync(uri);
    }

    private Task GithubrepositoryCommandExecuteAsync()
    {
        var uri = new Uri(Constants.Url.GITHUB_REPOSIOTRY);
        return _browserService.OpenAsync(uri);
    }

    #endregion
}
