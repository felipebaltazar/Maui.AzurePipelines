using PipelineApproval.Infrastructure.Extensions;

namespace PipelineApproval.Presentation.Views.Pages;

public partial class ProjectDetailsPage : ContentPage
{
	public ProjectDetailsPage()
	{
		InitializeComponent();
	}

    protected override bool OnBackButtonPressed()
    {
        var parameters = "NavigatingBack".ToNavigationParameters("NavigationMode");

        Application.Current
            .MainPage
            .CurrentPage()
            .RaiseOnNavigatedFrom(parameters);

        return base.OnBackButtonPressed();
    }
}