using Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpriteEditor;

// If you need to migrate your data model, for simple cases you can leverage the code in this template/sample file.
// Call Init() early in app launch flow before setting up the windows, and then specify the converter with custom Write() to migrate your contract.
// You can then migrate your assets by running ForceSaveAll script (fs), but be sure to validate your asset diffs in Git before commiting.
// IMPORTANT - READ EVERY TIME: run the migration first on your assets, THEN change/refactor your data models in code,
// so you can work off the new format without having to revert your code changes. Otherwise you'll get in a local dev pickle.
// Don't forget to change references in XAML to your old model properties, those won't be caught by compiler
public class Migrator
{
    public static void Init()
    {
        //JsonHelpers.InitCustomConverters(new MyRectJsonConverter());
    }
}

/*
public class MyRectJsonConverter : JsonConverter<MyRect>
{
    public override MyRect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Use default deserialization without recursion
        var defaultOptions = new JsonSerializerOptions(options);
        defaultOptions.Converters.Remove(this); // Prevent recursive use of this converter
        return JsonSerializer.Deserialize<MyRect>(ref reader, defaultOptions);
    }

    public override void Write(Utf8JsonWriter writer, MyRect value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x1", value.topLeftPoint.x);
        writer.WriteNumber("y1", value.topLeftPoint.y);
        writer.WriteNumber("x2", value.botRightPoint.x);
        writer.WriteNumber("y2", value.botRightPoint.y);
        writer.WriteEndObject();
    }
}
*/