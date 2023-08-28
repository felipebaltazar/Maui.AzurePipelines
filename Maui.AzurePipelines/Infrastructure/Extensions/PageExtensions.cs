using PipelineApproval.Abstractions;

namespace PipelineApproval.Infrastructure.Extensions;

public static class PageExtensions
{
    public static Page CurrentPage(this Page page)
    {
        if(page is NavigationPage nav)
        {
            return CurrentPage(nav.CurrentPage);
        }
        else if (page is TabbedPage tabbedPage)
        {
            return CurrentPage(tabbedPage.CurrentPage);
        }
        else
        {
            return page;
        }
    }

    public static void RaiseOnResumedAware(this Page page)
    {
        if (page is NavigationPage nav)
        {
            RaiseOnResumedAware(nav.CurrentPage);
            return;
        }

        if (page?.BindingContext is IApplicationLifecycleListener vmAware)
        {
            _ = Task.Run(vmAware.OnResumedAsync);
        }

        if(page is IApplicationLifecycleListener pageAware)
        {
            _ = Task.Run(pageAware.OnResumedAsync);
        }
    }

    public static void RaiseOnPausedAware(this Page page)
    {
        if (page is NavigationPage nav)
        {
            RaiseOnPausedAware(nav.CurrentPage);
            return;
        }

        if (page?.BindingContext is IApplicationLifecycleListener vmAware)
        {
            _ = Task.Run(vmAware.OnPausedAsync);
        }

        if (page is IApplicationLifecycleListener pageAware)
        {
            _ = Task.Run(pageAware.OnPausedAsync);
        }
    }

    public static void RaiseOnNavigatedTo(this Page page, IDictionary<string, string> parameters)
    {
        if (page is NavigationPage nav)
        {
            RaiseOnNavigatedTo(nav.CurrentPage, parameters);
            return;
        }

        if (page is INavigationAware pageAware)
        {
            _ = Task.Run(() => pageAware.OnNavigatedTo(parameters));
        }

        if (page?.BindingContext is INavigationAware vmAware)
        {
            _ = Task.Run(() => vmAware.OnNavigatedTo(parameters));
        }
    }

    public static void RaiseOnNavigatedFrom(this Page page, IDictionary<string, string> parameters)
    {
        if (page is NavigationPage nav)
        {
            RaiseOnNavigatedFrom(nav.CurrentPage, parameters);
            return;
        }

        if (page is INavigationAware pageAware)
        {
            _ = Task.Run(() => pageAware.OnNavigatedFrom(parameters));
        }

        if (page?.BindingContext is INavigationAware vmAware)
        {
            _ = Task.Run(() => vmAware.OnNavigatedFrom(parameters));
        }
    }
}
