namespace Intalio.Tools.Common.Extensions
{
    public static class DictionaryExtensions
    {
        public static int? TryGetValue(this IDictionary<string, object> parameters, string key)
        {
            if (parameters.ContainsKey(key))
            {
                return Convert.ToInt32(parameters[key]);
            }
            return null;
        }
    }
}
