using Foundation;
using UIKit;

namespace PipelineApproval;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override void OnActivated(UIApplication application)
    {
        (application.ConnectedScenes.AnyObject as UIWindowScene)?.Windows.FirstOrDefault()?.MakeKeyWindow();
        base.OnActivated(application);
    }

    public override void OnResignActivation(UIApplication application)
    {
        (application.ConnectedScenes.AnyObject as UIWindowScene)?.Windows.FirstOrDefault()?.MakeKeyWindow();
        base.OnResignActivation(application);
    }
}
