using Mopups.Interfaces;
using Mopups.Pages;
using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Views;
using PipelineApproval.Infrastructure.Extensions;
using System.Web;

namespace PipelineApproval.Infrastructure.Services;

public sealed class NavigationService : INavigationService
{
    #region Fields

    private const string RELATIVE_URL = "/";
    private const string POP_URL = "../";
    private const string QUERY = "?";

    private readonly IServiceProvider _serviceProvider;
    private readonly IPopupNavigation _popupNavigation;

    private INavigation _navigation;
    private NavigationPage _rootNavigation;

    #endregion

    #region Constructors

    public NavigationService(
        IServiceProvider provider,
        IPopupNavigation popupNavigation)
    {
        _serviceProvider = provider;

        var startPage = _serviceProvider.GetService<IStartPage>() as Page;
        ViewModelLocator.LocateViewModel(startPage, _serviceProvider);
        var navigationPage = new NavigationPage(startPage);

        _rootNavigation = navigationPage;
        _navigation = _rootNavigation.Navigation;
        _popupNavigation = popupNavigation;
    }

    #endregion

    #region INavigationService

    public Page InitializeNavigation()
    {
        return _rootNavigation;
    }

    public string GetNavigationUriPath()
    {
        try
        {
            var pages = _navigation
                .NavigationStack
                .Select(x => x.Title ?? x.GetType().Name);

            return string.Join("/", pages);
        }
        catch (Exception)
        {
        }

        return string.Empty;

    }

    public Task PushPopupAsync<T>(Action<T> setup = null)
    {
        var popup = _serviceProvider.GetService<T>();

        if (popup is PopupPage popupPage)
        {
            return TryInvokeOnMainThread(() =>
            {
                setup?.Invoke(popup);

                async Task showPopupBeforeLoader()
                {
                    if (_popupNavigation.PopupStack.Any())
                    {
                        await _popupNavigation.PopAsync();
                    }

                    await _popupNavigation.PushAsync(popupPage);
                }

                return showPopupBeforeLoader();
            });
        }

        return Task.CompletedTask;
    }

    public Task NavigateToAsync(string url, IDictionary<string, string> parameters = null) =>
        NavigateToInternal(url, parameters ?? new Dictionary<string, string>(0), true);

    #endregion

    #region Private Methods

    private async Task NavigateToInternal(string url, IDictionary<string, string> parameters, bool animated = true)
    {
        if (url == POP_URL)
        {
            _rootNavigation.RaiseOnNavigatedFrom(parameters);
            await TryInvokeOnMainThread(() => _navigation.PopAsync(animated)).ConfigureAwait(false);
            return;
        }

        if (!url.StartsWith(RELATIVE_URL) &&
           !url.StartsWith(POP_URL))
        {
            ReplaceRoot(url, parameters);
            return;
        }

        var uri = new Uri(url);
        var segments = uri.Segments.Where(s => !RELATIVE_URL.Equals(s));

        foreach (var segment in segments)
        {
            var decodedSegment = HttpUtility.UrlDecode(segment);
            if (decodedSegment.Contains(".."))
            {
                _rootNavigation.RaiseOnNavigatedFrom(parameters);
                await TryInvokeOnMainThread(() => _navigation.PopAsync(animated)).ConfigureAwait(false);
                continue;
            }

            var indiceQuery = decodedSegment.IndexOf(QUERY) + 1;
            var query = indiceQuery > 0
                ? decodedSegment.Substring(indiceQuery)
                : null;

            var pagina = decodedSegment.Replace($"{QUERY}{query}", string.Empty);
            var urlParameters = query?
                .Split('&')
                .Select(q => q.Split('='))
                .ToDictionary(p => p[0], p => p[1]);

            if (urlParameters != null)
            {
                parameters = parameters
                    .Concat(urlParameters)
                    .ToDictionary(p => p.Key, p => p.Value);
            }

            await PushPageAsync(pagina, parameters, animated).ConfigureAwait(false);
        }
    }

    private void ReplaceRoot(string url, IDictionary<string, string> parameters)
    {
        var uri = new Uri($"/{url}");
        var pageName = uri.Segments.First(s => !RELATIVE_URL.Equals(s));

        _ = TryInvokeOnMainThread(async () =>
        {
            var page = ResolvePage(pageName, parameters);
            _rootNavigation.RaiseOnNavigatedFrom(parameters);
            page.Opacity = 0;

            await Application.Current.MainPage
                            .CurrentPage()
                            .FadeTo(0);

            Application.Current.MainPage = _rootNavigation = new NavigationPage(page);
            _navigation = _rootNavigation.Navigation;
            _ = page.FadeTo(1);
            page.RaiseOnNavigatedTo(parameters);
        });
    }

    private Task PushPageAsync(string nomePagina, IDictionary<string, string> parameters, bool animated)
    {
        return TryInvokeOnMainThread(async () =>
        {
            _rootNavigation.RaiseOnNavigatedFrom(parameters);
            var page = ResolvePage(nomePagina, parameters);
            await _navigation.PushAsync(page, animated).ConfigureAwait(false);
            page.RaiseOnNavigatedTo(parameters);
        });
    }

    private Page ResolvePage(string url, IDictionary<string, string> parameters)
    {
        var pageType = Type.GetType($"PipelineApproval.Presentation.Views.Pages.{url}");
        var page = (Page)_serviceProvider.GetService(pageType);

        ViewModelLocator.LocateViewModel(page, _serviceProvider);

        if (page.BindingContext is IInitializeAware initialize)
        {
            _ = Task.Run(() => initialize.InitializeAsync(parameters));
        }

        ParseParameters(page, parameters);

        return page;
    }

    private static void ParseParameters(Page page, IDictionary<string, string> parameters)
    {
        if (parameters is null)
            return;

        var vmType = page.BindingContext.GetType();
        foreach (var parameter in parameters)
        {
            var property = vmType.GetProperty(parameter.Key);
            if (property?.PropertyType == parameter.Value.GetType())
                property?.SetValue(page.BindingContext, parameter.Value);
        }
    }

    private Task TryInvokeOnMainThread(Func<Task> value)
    {
        if (MainThread.IsMainThread)
        {
            return value.Invoke();
        }
        else
        {
            return MainThread.InvokeOnMainThreadAsync(value);
        }
    }

    private void TryInvokeOnMainThread(Action value)
    {
        if (MainThread.IsMainThread)
        {
            value.Invoke();
        }
        else
        {
            MainThread.BeginInvokeOnMainThread(value);
        }
    }

    #endregion
}
