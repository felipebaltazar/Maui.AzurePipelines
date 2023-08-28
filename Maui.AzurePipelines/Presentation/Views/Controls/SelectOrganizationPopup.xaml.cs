using AsyncAwaitBestPractices;
using Mopups.Pages;
using Mopups.Services;
using PipelineApproval.Abstractions.Views;
using PipelineApproval.Models;

namespace PipelineApproval.Presentation.Views.Controls;

public partial class SelectOrganizationPopup : PopupPage, ISelectOrganizationPopup
{
    IList<AccountInfo> organizations =
        new List<AccountInfo>();

    public IList<AccountInfo> Organizations
    {
        get => organizations;
        set
        {
            organizations = value;
            BindableLayout.SetItemsSource(this.containerItems, value);
        }
    }

    public Action<AccountInfo> OnSelected
    {
        get;
        set;
    }

    public SelectOrganizationPopup()
    {
        InitializeComponent();
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is not Frame frame)
            return;

        if (e.Parameter is not AccountInfo accountInfo)
            return;

        OnSelected?.Invoke(accountInfo);

        MopupService.Instance.PopAsync()
                             .SafeFireAndForget();
    }
}