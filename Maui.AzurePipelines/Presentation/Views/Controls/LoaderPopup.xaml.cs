using Mopups.Pages;

namespace PipelineApproval.Presentation.Views.Controls;

public partial class LoaderPopup : PopupPage
{
	public LoaderPopup()
	{
		InitializeComponent();
	}

    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}