
using PipelineApproval.Infrastructure.Extensions;

namespace PipelineApproval.Presentation.Views.Pages;

public partial class PipelineDetailsPage : ContentPage
{
	public PipelineDetailsPage()
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