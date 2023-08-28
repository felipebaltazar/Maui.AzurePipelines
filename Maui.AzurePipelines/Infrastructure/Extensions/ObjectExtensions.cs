using System.Text.Json;

namespace PipelineApproval.Infrastructure.Extensions
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, string> ToNavigationParameters(this object obj, string key = null)
        {
            var result = new Dictionary<string, string>(1);

            if (obj != null)
            {
                var objStr = JsonSerializer.Serialize(obj);
                result.Add(key ?? obj.GetType().Name, objStr);
            }

            return result;
        }

        public static IDictionary<string, string> ToNavigationParameters(this string strObject, string key)
        {
            var result = new Dictionary<string, string>(1);

            if (!string.IsNullOrWhiteSpace(strObject))
            {
                result.Add(key, strObject);
            }

            return result;
        }
    }
}
