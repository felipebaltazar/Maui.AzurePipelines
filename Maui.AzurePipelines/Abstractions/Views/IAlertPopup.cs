namespace PipelineApproval.Abstractions.Views;

public interface IAlertPopup
{
    string Message { get; set; }

    string MessageTitle { get; set; }

    string CancelButton { get; set; }
}
