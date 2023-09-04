using PipelineApproval.Models;

namespace PipelineApproval.Abstractions.Views;

public interface ISelectTeamPopup
{
    IList<Team> Teams { get; set; }

    Action<Team> OnSelected { get; set; }
}
