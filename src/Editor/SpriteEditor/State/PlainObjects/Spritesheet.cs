using Editor;
using Shared;
using SkiaSharp;
using System.Drawing;

namespace SpriteEditor;

public class Spritesheet
{
    public string name { get; set; }    // If in nested path, will be "foo/bar.png", etc. relative to the spritesheets folder
    public FilePath filePath { get; set; }  // The full path on disk

    private Lazy<BitmapDrawer> _drawer;
    public BitmapDrawer drawer => _drawer.Value;

    private Lazy<PixelData[,]> _imgPixelGrid;
    public PixelData[,] imgPixelGrid => _imgPixelGrid.Value;

    public Spritesheet(string name, FilePath filePath)
    {
        this.name = name;
        this.filePath = filePath;
        Reload();
    }

    public void Reload()
    {
        // Lazy load the drawer and pixel grid because otherwise they will make app startup real slow
        _drawer = new Lazy<BitmapDrawer>(() => new BitmapDrawer(SKBitmap.Decode(filePath.fullPath)));
        _imgPixelGrid = new Lazy<PixelData[,]>(() => Get2DArrayFromImage(drawer.skBitmap));
    }

    public PixelData[,] Get2DArrayFromImage(SKBitmap bitmap)
    {
        var arr = new PixelData[bitmap.Height, bitmap.Width];

        IntPtr pixels = bitmap.GetPixels();
        int width = bitmap.Width;
        int height = bitmap.Height;
        int bytesPerPixel = 4;
        unsafe
        {
            byte* ptr = (byte*)pixels.ToPointer();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int offset = (i * width + j) * bytesPerPixel;
                    byte blue = ptr[offset];
                    byte green = ptr[offset + 1];
                    byte red = ptr[offset + 2];
                    byte alpha = ptr[offset + 3];
                    arr[i, j] = new PixelData(j, i, Color.FromArgb(alpha, red, green, blue), []);
                }
            }
        }

        for (int i = 0; i < arr.GetLength(0); i++)
        {
            for (int j = 0; j < arr.GetLength(1); j++)
            {
                List<PixelData> neighbors = arr[i, j].neighbors;
                AddIfNotNull(neighbors, Get2DArrayEl(arr, i - 1, j - 1));
                AddIfNotNull(neighbors, Get2DArrayEl(arr, i - 1, j));
                AddIfNotNull(neighbors, Get2DArrayEl(arr, i - 1, j + 1));
                AddIfNotNull(neighbors, Get2DArrayEl(arr, i, j - 1));
                AddIfNotNull(neighbors, Get2DArrayEl(arr, i, j));
                AddIfNotNull(neighbors, Get2DArrayEl(arr, i, j + 1));
                AddIfNotNull(neighbors, Get2DArrayEl(arr, i + 1, j - 1));
                AddIfNotNull(neighbors, Get2DArrayEl(arr, i + 1, j));
                AddIfNotNull(neighbors, Get2DArrayEl(arr, i + 1, j + 1));
            }
        }

        return arr;
    }

    public void AddIfNotNull(List<PixelData> list, PixelData? pixel)
    {
        if (pixel != null)
        {
            list.Add(pixel);
        }
    }

    public PixelData? Get2DArrayEl(PixelData[,] arr, int i, int j)
    {
        if (i < 0 || i >= arr.GetLength(0)) return null;
        if (j < 0 || j >= arr.GetLength(1)) return null;
        if (arr[i, j].rgb.A == 0) return null;
        return arr[i, j];
    }

    public MyRect? GetPixelClumpRect(float x, float y)
    {
        int ix = MyMath.Round(x);
        int iy = MyMath.Round(y);
        PixelData? selectedNode = imgPixelGrid.InRange(iy, ix) ? imgPixelGrid[iy, ix] : null;
        if (selectedNode == null)
        {
            return null;
        }
        if (selectedNode.rgb.A == 0)
        {
            return null;
        }

        var queue = new List<PixelData>();
        queue.Add(selectedNode);

        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = -1f;
        var maxY = -1f;

        var num = 0;
        var visitedNodes = new HashSet<PixelData>();
        while (queue.Count > 0)
        {
            var node = queue[0];
            queue.RemoveAt(0);
            num++;
            if (node.x < minX) minX = node.x;
            if (node.y < minY) minY = node.y;
            if (node.x > maxX) maxX = node.x;
            if (node.y > maxY) maxY = node.y;

            foreach (PixelData? neighbor in node.neighbors)
            {
                if (neighbor == null) continue;
                if (visitedNodes.Contains(neighbor)) continue;
                if (!queue.Contains(neighbor))
                {
                    queue.Add(neighbor);
                }
            }
            visitedNodes.Add(node);
        }
        return new MyRect(MyMath.Round(minX), MyMath.Round(minY), MyMath.Round(maxX + 1), MyMath.Round(maxY + 1));
    }

    public MyRect? GetSelectedPixelRect(float x, float y, float endX, float endY)
    {
        x = MyMath.Round(x);
        y = MyMath.Round(y);

        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = -1f;
        var maxY = -1f;

        for (int i = (int)y; i <= endY; i++)
        {
            for (int j = (int)x; j <= endX; j++)
            {
                if (imgPixelGrid.InRange(i, j) && imgPixelGrid[i, j].rgb.A != 0)
                {
                    if (i < minY) minY = i;
                    if (i > maxY) maxY = i;
                    if (j < minX) minX = j;
                    if (j > maxX) maxX = j;
                }
            }
        }

        if (minX == float.MaxValue || minY == float.MaxValue || maxX == -1 || maxY == -1) return null;

        return new MyRect(MyMath.Round(minX), MyMath.Round(minY), MyMath.Round(maxX + 1), MyMath.Round(maxY + 1));
    }
}
