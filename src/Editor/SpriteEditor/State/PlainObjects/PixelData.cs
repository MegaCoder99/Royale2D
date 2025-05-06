using System.Drawing;

namespace SpriteEditor;

public class PixelData
{
    public float x;
    public float y;
    public Color rgb;
    public List<PixelData> neighbors;

    public PixelData(float x, float y, Color rgb, List<PixelData> neighbors)
    {
        this.x = x;
        this.y = y;
        this.rgb = rgb;
        this.neighbors = neighbors;
    }
}
