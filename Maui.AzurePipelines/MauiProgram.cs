using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using Mopups.Services;
using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Views;
using PipelineApproval.Infrastructure.Extensions;
using PipelineApproval.Infrastructure.Services;
using PipelineApproval.Models;
using PipelineApproval.Presentation.ViewModels.Pages;
using PipelineApproval.Presentation.Views.Controls;
using PipelineApproval.Presentation.Views.Pages;

namespace PipelineApproval;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("DevOpsIcons.ttf", "DevOpsIcons");
            })
            .ConfigureMopups();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        RegisterServices(builder);
        RegisterPages(builder);
        RegisterPopups(builder);

        return builder.Build();
    }

    private static void RegisterPopups(MauiAppBuilder builder)
    {
        builder.Services.AddScoped<IAlertPopup, AlertPopup>();
        builder.Services.AddScoped<ISelectOrganizationPopup, SelectOrganizationPopup>();
    }

    private static void RegisterPages(MauiAppBuilder builder)
    {
        builder.Services.AddPageStartAndViewModel<LoginPage, LoginPageViewModel>();
        builder.Services.AddPageAndViewModel<MainPage, MainPageViewModel>();
    }

    private static void RegisterServices(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton(MopupService.Instance);
        builder.Services.AddSingleton<IBrowserService, BrowserService>();
        builder.Services.AddSingleton<ILogger, AppCenterLoggerService>();
        builder.Services.AddSingleton<IMainThreadService, MainThreadService>();

        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ILazyDependency<INavigationService>, LazyDependency<INavigationService>>();

        builder.Services.AddSingleton<ILoaderService, LoaderService>();
        builder.Services.AddSingleton<ILazyDependency<ILoaderService>, LazyDependency<ILoaderService>>();
    }
}
