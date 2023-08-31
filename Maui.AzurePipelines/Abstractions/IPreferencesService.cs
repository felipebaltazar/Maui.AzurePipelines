namespace PipelineApproval.Abstractions;

public interface IPreferencesService
{
    string Get(string key, string defaultValue);

    IPreferencesService Set(string key, string value);
}
