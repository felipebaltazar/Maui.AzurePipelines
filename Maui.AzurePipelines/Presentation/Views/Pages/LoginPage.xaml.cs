using PipelineApproval.Abstractions.Views;

namespace PipelineApproval.Presentation.Views.Pages;

public partial class LoginPage : BaseContentPage, IStartPage
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