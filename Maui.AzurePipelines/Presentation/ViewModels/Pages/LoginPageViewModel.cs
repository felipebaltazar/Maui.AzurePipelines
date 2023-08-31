using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Views;
using PipelineApproval.Infrastructure;
using PipelineApproval.Infrastructure.Commands;
using PipelineApproval.Infrastructure.Extensions;

namespace PipelineApproval.Presentation.ViewModels.Pages;

public class LoginPageViewModel : BaseViewModel, INavigationAware
{
    #region Fields

    private readonly ISecureStorageService _secureStorageService;
    private readonly IVisualStudioService _visualStudioService;
    private readonly IPreferencesService _preferencesService;
    private readonly IBrowserService _browserService;

    private bool isReady;
    private string pat;

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
       IPreferencesService preferencesService,
       IVisualStudioService visualStudioService,
       ISecureStorageService secureStorageService,
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
        _secureStorageService = secureStorageService;
        _visualStudioService = visualStudioService;
        _preferencesService = preferencesService;
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
                var pat = await _secureStorageService.GetAsync(Constants.Storage.PAT_TOKEN_KEY).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(pat))
                {
                    IsReady = true;
                    return;
                }

                var organizationsStr = _preferencesService.Get(Constants.Storage.USER_ORGANIZATIONS, string.Empty);
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
                MainThreadService.BeginInvokeOnMainThread(() =>
                {
                    IsReady = true;
                });
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
            var isValid = await _visualStudioService.ValidateAccessTokenAsync(PAT).ConfigureAwait(false);
            if (isValid)
            {
                var apiResponse = await _visualStudioService.GetOrganizationsAsync().ConfigureAwait(false);
                var navParameters = apiResponse.ToNavigationParameters(Constants.Navigation.ACCOUNT_PARAMETER);

                _preferencesService.Set(Constants.Storage.USER_ORGANIZATIONS, navParameters[Constants.Navigation.ACCOUNT_PARAMETER]);
                await NavigationService.NavigateToAsync("MainPage", navParameters).ConfigureAwait(false);

                return;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Não foi possível efetuar login");
        }

        await NavigationService.PushPopupAsync<IAlertPopup>(p =>
        {
            p.MessageTitle = "Erro de autenticação";
            p.Message = "Não foi possível autenticar, verifique se o 'Personal Access Token' é valido.";
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
