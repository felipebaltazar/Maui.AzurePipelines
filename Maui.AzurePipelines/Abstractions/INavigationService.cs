namespace PipelineApproval.Abstractions;

public interface INavigationService
{
    string GetNavigationUriPath();
    Page InitializeNavigation();
    Task NavigateToAsync(string url, IDictionary<string, string> parameters = null);
    Task PushPopupAsync<T>(Action<T> setup = null);
}
