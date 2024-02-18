using Maui.ServerDrivenUI;
using Refit;

namespace PipelineApproval.Abstractions.Data;

public interface IServerDrivenUIApi
{
    [Get("/ServerDrivenUI?key={key}")]
    Task<ServerUIElement> GetUIElementAsync(string key);
}
