using AsyncAwaitBestPractices;
using Mopups.Pages;
using Mopups.Services;
using PipelineApproval.Abstractions.Views;

namespace PipelineApproval.Presentation.Views.Controls;

public partial class AlertPopup : PopupPage, IAlertPopup
{
    public string Message 
    {
        get => description.Text;
        set => MainThread.BeginInvokeOnMainThread(()=> description.Text = value);
    }

    public string MessageTitle
    {
        get => titleLabel.Text;
        set => MainThread.BeginInvokeOnMainThread(() => titleLabel.Text = value);
    }

    public string CancelButton
    {
        get => cancelButton.Text;
        set => MainThread.BeginInvokeOnMainThread(() => cancelButton.Text = value);
    }

    public AlertPopup()
	{
		InitializeComponent();
	}

    private void Button_Clicked(object sender, EventArgs e)
    {
        MopupService.Instance.PopAsync().SafeFireAndForget();
    }
}