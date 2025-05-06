using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared;

public record TileClumpInstance(int i, int j, string tileClumpName);

public record MapSectionLayerModel
{
    [JsonConverter(typeof(TileGridConverter))]
    public int[,] tileGrid;

    [JsonConstructor]
    public MapSectionLayerModel(int[,] tileGrid)
    {
        this.tileGrid = tileGrid;
    }

    public MapSectionLayerModel(int width, int height)
    {
        tileGrid = SharedHelpers.Create2DArray(width, height, Tile.TransparentTileId);
    }
}

public class TileGridConverter : JsonConverter<int[,]>
{
    public override int[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? tileGridEncoded = reader.GetString();
        int[,] tileGrid = !string.IsNullOrEmpty(tileGridEncoded) ? DeserializeTileGridFromBase64(tileGridEncoded) : new int[0, 0];
        return tileGrid;
    }

    public override void Write(Utf8JsonWriter writer, int[,] value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(SerializeTileGridToBase64(value));
    }

    private string SerializeTileGridToBase64(int[,] data)
    {
        // Assuming all rows are the same length
        int numRows = data.GetLength(0);
        int numCols = data.GetLength(1);
        byte[] bytes = new byte[4 + numRows * numCols * 2]; // Additional 4 bytes for storing dimensions

        // Store width and height at the beginning
        bytes[0] = (byte)(numCols & 0xFF);
        bytes[1] = (byte)((numCols >> 8) & 0xFF);
        bytes[2] = (byte)(numRows & 0xFF);
        bytes[3] = (byte)((numRows >> 8) & 0xFF);

        int byteIndex = 4; // Start writing data after the first four bytes
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                int value = data[i, j];
                if (value < 0 || value > 65535)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Integer out of range for ushort.");
                }

                bytes[byteIndex++] = (byte)(value & 0xFF);         // Lower byte
                bytes[byteIndex++] = (byte)((value >> 8) & 0xFF);  // Upper byte
            }
        }

        return Convert.ToBase64String(bytes);
    }

    private int[,] DeserializeTileGridFromBase64(string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);

        // Read width and height from the first four bytes
        int numCols = (bytes[0] & 0xFF) | ((bytes[1] & 0xFF) << 8);
        int numRows = (bytes[2] & 0xFF) | ((bytes[3] & 0xFF) << 8);

        int[,] data = new int[numRows, numCols];
        int byteIndex = 4; // Start reading data after the first four bytes

        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                int value = (bytes[byteIndex++] & 0xFF) | ((bytes[byteIndex++] & 0xFF) << 8);
                data[i, j] = value;
            }
        }

        return data;
    }
}
