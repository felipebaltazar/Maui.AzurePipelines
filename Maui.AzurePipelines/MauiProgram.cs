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
using Microsoft.Maui.Handlers;
using Maui.ServerDrivenUI;
using PipelineApproval.Abstractions.Data;
using System.Diagnostics;
using TaskExtensions = PipelineApproval.Infrastructure.Extensions.TaskExtensions;
using System.Xml;

#if DEBUG
using DotNet.Meteor.HotReload.Plugin;
#endif

namespace PipelineApproval;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        TaskExtensions.SetDefaultExceptionHandling(ex =>
        {
            Debug.Write(ex.ToString());

            if (Debugger.IsAttached)
                Debugger.Break();
        });

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseVirtualListView()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("DevOpsIcons.ttf", "DevOpsIcons");
                fonts.AddFont("FontAwesomeSolid.otf", "FontAwesomeSolid");
            })
            .UseSentry(options =>
            {
                options.Dsn = "https://9443fbd4b733ff0976c2aeeb237a4913@o4508135677755392.ingest.us.sentry.io/4508135679328256";

#if DEBUG
                options.Debug = true;
#endif
                options.TracesSampleRate = 1.0;
            })
            .ConfigureServerDrivenUI(s =>
            {
                s.UIElementCacheExpiration = TimeSpan.FromMinutes(1);

                s.RegisterElementGetter(async (key, provider) =>
                {
                    try
                    {
                        var response = await provider.GetService<IServerDrivenUIApi>().GetUIElementAsync(key);
                        var xaml = response.ToXaml();

                        try
                        {
                            using (var textreader = new StringReader(xaml))
                            using (var reader = XmlReader.Create(textreader))
                            {
                                var value = reader.Read();
                            }

                            SentrySdk.AddBreadcrumb("XML Parser", "Success", "ServerUIElement",
                                data: new Dictionary<string, string>
                                {
                                    { "Xaml", xaml }
                                });
                        }
                        catch (Exception ex)
                        {
                            SentrySdk.CaptureException(ex, s => s.AddBreadcrumb("XAML", "ServerDrivenUI", "ServerUIElement",
                            data: new Dictionary<string, string>
                                {
                                    { "Xaml", xaml }
                                }));
                        }

                        return response;
                    }
                    catch (Exception e)
                    {
                        SentrySdk.CaptureException(e);
                        return null;
                    }
                });

                s.AddServerElement("595597a8-25df-4d60-99f4-4b5bad595403");
            })
            .ConfigureMopups();

#if DEBUG
        builder.EnableHotReload()
        .Logging.AddDebug();
#endif
        var serviceCollection = builder.Services;

        ConfigureHandlers();
        RegisterServices(serviceCollection);
        RegisterPages(serviceCollection);
        RegisterPopups(serviceCollection);

        return builder.Build();
    }

    private static void ConfigureHandlers()
    {
#if ANDROID
        EntryHandler.Mapper.AppendToMapping(nameof(Entry),
            (handler, view) => handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent));
#endif
    }

    private static void RegisterPopups(IServiceCollection sCollection)
    {
        sCollection.AddScoped<IAlertPopup, AlertPopup>();
        sCollection.AddScoped<ISelectTeamPopup, SelectTeamPopup>();
        sCollection.AddScoped<IOrganizationLoginPopup, OrganizationLoginPopup>();
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
        sCollection.AddSingleton<IMainThreadService, MainThreadService>();
        sCollection.AddSingleton<IPreferencesService, PreferencesService>();
        sCollection.AddSingleton<ISecureStorageService, SecureStorageService>();

        sCollection.AddSingleton<ILogger, SentryLoggerService>();

        sCollection.AddSingleton<INavigationService, NavigationService>();
        sCollection.AddSingleton<ILazyDependency<INavigationService>, LazyDependency<INavigationService>>();

        sCollection.AddSingleton<ILoaderService, LoaderService>();
        sCollection.AddSingleton<ILazyDependency<ILoaderService>, LazyDependency<ILoaderService>>();

        sCollection.AddAzureApiService();
        sCollection.AddServerDrivenUIApi();
    }
}
