namespace PipelineApproval.Abstractions.Views;

public interface IOrganizationLoginPopup
{
    IAsyncCommand<string> OnResultCommand { get; set; }
}
