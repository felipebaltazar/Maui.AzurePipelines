using PipelineApproval.Abstractions.Views;

namespace PipelineApproval.Presentation.Views.Pages;

public partial class LoginPage : ContentPage, IStartPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private void ImageButton_Clicked(object sender, EventArgs e)
    {
		patField.IsPassword = !patField.IsPassword;
    }
}