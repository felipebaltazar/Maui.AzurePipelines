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
                fonts.AddFont("FontAwesomeSolid.otf", "FontAwesomeSolid");
            })
            .ConfigureMopups();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        var serviceCollection = builder.Services;

        RegisterServices(serviceCollection);
        RegisterPages(serviceCollection);
        RegisterPopups(serviceCollection);

        return builder.Build();
    }

    private static void RegisterPopups(IServiceCollection sCollection)
    {
        sCollection.AddScoped<IAlertPopup, AlertPopup>();
        sCollection.AddScoped<ISelectOrganizationPopup, SelectOrganizationPopup>();
    }

    private static void RegisterPages(IServiceCollection sCollection)
    {
        sCollection.AddPageStartAndViewModel<LoginPage, LoginPageViewModel>();

        sCollection.AddPageAndViewModel<PipelineDetailsPage, PipelineDetailsPageViewModel>();
        sCollection.AddPageAndViewModel<ProjectDetailsPage, ProjectDetailsPageViewModel>();
        sCollection.AddPageAndViewModel<TaskLogPage, TaskLogPageViewModel>();
        sCollection.AddPageAndViewModel<MainPage, MainPageViewModel>();
    }

    private static void RegisterServices(IServiceCollection sCollection)
    {
        sCollection.AddSingleton(MopupService.Instance);
        sCollection.AddSingleton<IBrowserService, BrowserService>();
        sCollection.AddSingleton<ILogger, AppCenterLoggerService>();
        sCollection.AddSingleton<IMainThreadService, MainThreadService>();
        sCollection.AddSingleton<IPreferencesService, PreferencesService>();
        sCollection.AddSingleton<ISecureStorageService, SecureStorageService>();

        sCollection.AddSingleton<INavigationService, NavigationService>();
        sCollection.AddSingleton<ILazyDependency<INavigationService>, LazyDependency<INavigationService>>();

        sCollection.AddSingleton<ILoaderService, LoaderService>();
        sCollection.AddSingleton<ILazyDependency<ILoaderService>, LazyDependency<ILoaderService>>();

        sCollection.AddAzureApiService();
    }
}
