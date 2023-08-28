namespace PipelineApproval.Abstractions;

public interface INavigationAware
{
    Task OnNavigatedTo(IDictionary<string, string> parameters);

    Task OnNavigatedFrom(IDictionary<string, string> parameters);
}
