namespace PipelineApproval.Abstractions;

public interface IPreferencesService
{
    string Get(string key, string defaultValue);

    int Get(string key, int defaultValue);

    IPreferencesService Set(string key, string value);

    IPreferencesService Set(string key, int value);
}
