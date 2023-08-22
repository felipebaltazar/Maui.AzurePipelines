namespace PipelineApproval;

public partial class App : Application
{
	public App()
	{
		UserAppTheme = AppTheme.Dark;
		InitializeComponent();

		MainPage = new AppShell();
	}
}
