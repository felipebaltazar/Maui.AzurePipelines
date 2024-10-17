using PipelineApproval.Abstractions;
using PipelineApproval.Infrastructure.Extensions;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace PipelineApproval;

public partial class App : Application
{
    private readonly INavigationService _navigationService;

    public App(INavigationService navigationService)
    {
        _navigationService = navigationService;
        UserAppTheme = AppTheme.Dark;

        InitializeComponent();
        MainPage = _navigationService.InitializeNavigation();
    }

    protected override void OnStart()
    {
        base.OnStart();
        MainPage.RaiseOnNavigatedTo(new Dictionary<string, string>(0));
    }

    protected override void OnResume()
    {
        base.OnResume();
        MainPage?.RaiseOnResumedAware();
    }

    protected override void OnSleep()
    {
        base.OnSleep();
        MainPage?.RaiseOnPausedAware();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_navigationService.InitializeNavigation());
    }
}
