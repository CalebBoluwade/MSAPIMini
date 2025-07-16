using System.Text.Json;

namespace MS.API.Mini.Extensions;

// Extension methods for JSON handling
public static class JsonExtensions
{
    public static T? FromJson<T>(this string json) where T : class
    {
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<T>(json);
    }

    public static string ToJson<T>(this T? obj) where T : class
    {
        return obj == null ? string.Empty : JsonSerializer.Serialize(obj);
    }
}