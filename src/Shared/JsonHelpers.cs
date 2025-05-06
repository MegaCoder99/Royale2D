using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Shared;

public static class JsonHelpers
{
    private static void ExcludeEmptyStrings(JsonTypeInfo jsonTypeInfo)
    {
        if (jsonTypeInfo.Kind != JsonTypeInfoKind.Object) return;

        foreach (JsonPropertyInfo jsonPropertyInfo in jsonTypeInfo.Properties)
        {
            // Skip read-only properties when IgnoreReadOnlyProperties is true
            if (jsonTypeInfo.Options.IgnoreReadOnlyProperties &&
                jsonPropertyInfo.Get != null && jsonPropertyInfo.Set == null)
                continue;

            if (jsonPropertyInfo.PropertyType == typeof(string))
            {
                jsonPropertyInfo.ShouldSerialize = static (obj, value) => !string.IsNullOrEmpty((string)value!);
            }
        }
    }

    private static JsonSerializerOptions options = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IgnoreReadOnlyProperties = true,
        WriteIndented = true,
        IncludeFields = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { ExcludeEmptyStrings }
        },
        Converters =
        {
            new Int2DArrayConverter()
        }
    };

    // To be called in startup code of either editor or engine for custom converters for respective area
    public static void InitCustomConverters(params JsonConverter[] extraConverters)
    {
        foreach (JsonConverter converter in extraConverters ?? [])
        {
            options.Converters.Add(converter);
        }
    }

    public static T DeserializeJson<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, options)!;
    }

    public static string SerializeToJson<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, options);
    }

    public static T DeserializeJsonFile<T>(FilePath filePath)
    {
        string json = File.ReadAllText(filePath.fullPath);
        return DeserializeJson<T>(json);
    }

    public static void SerializeToJsonFile<T>(FilePath filePath, T obj)
    {
        filePath.WriteAllText(SerializeToJson(obj));
    }

    public static T DeepCloneJson<T>(T obj)
    {
        string json = SerializeToJson(obj);
        return DeserializeJson<T>(json);
    }
}

public class Int2DArrayConverter : JsonConverter<int[,]>
{
    public override int[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var list = JsonSerializer.Deserialize<List<List<int>>>(ref reader, options);
        if (list == null || list.Count == 0)
        {
            return new int[0, 0];
        }

        int rows = list.Count;
        int cols = list[0].Count;
        var array = new int[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                array[i, j] = list[i][j];
            }
        }

        return array;
    }

    public override void Write(Utf8JsonWriter writer, int[,] value, JsonSerializerOptions options)
    {
        int rows = value.GetLength(0);
        int cols = value.GetLength(1);
        var list = new List<List<int>>(rows);

        for (int i = 0; i < rows; i++)
        {
            var row = new List<int>(cols);
            for (int j = 0; j < cols; j++)
            {
                row.Add(value[i, j]);
            }
            list.Add(row);
        }

        JsonSerializer.Serialize(writer, list, options);
    }
}
