using PipelineApproval.Models;

namespace PipelineApproval.Abstractions.Views;

public interface ISelectOrganizationPopup
{
    IList<AccountInfo> Organizations { get; set; }

    Action<AccountInfo> OnSelected { get; set; }
}
