using Mopups.Pages;
using Mopups.Services;
using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Views;
using PipelineApproval.Infrastructure.Extensions;

namespace PipelineApproval.Presentation.Views.Controls;

public partial class OrganizationLoginPopup : PopupPage, IOrganizationLoginPopup
{
    public IAsyncCommand<string> OnResultCommand
    {
        get;
        set;
    }

    public OrganizationLoginPopup()
    {
        InitializeComponent();
    }

    private void confirmBtb_Clicked(object sender, EventArgs e)
    {
        MopupService.Instance.PopAsync().SafeFireAndForget();
        OnResultCommand?.Execute(organizationField.Text);
    }
}