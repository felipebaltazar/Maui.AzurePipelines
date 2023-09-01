using System.Text.Json;

namespace PipelineApproval.Infrastructure.Extensions;

public static class DictionaryExtensions
{
    public static T GetValueOrDefault<T>(this IDictionary<string, string> dictionary, string key, T defaultValue = null)
        where T : class
    {
        if (dictionary.TryGetValue(key, out var resultStr))
        {
            return JsonSerializer.Deserialize<T>(resultStr);
        }

        return defaultValue;
    }

    public static string GetValueOrDefault(this IDictionary<string, string> dictionary, string key, string defaultValue = null)
    {
        if (dictionary.TryGetValue(key, out var resultStr))
        {
            return resultStr;
        }

        return defaultValue;
    }
}
