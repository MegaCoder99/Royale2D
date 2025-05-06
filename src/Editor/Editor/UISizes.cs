#pragma warning disable CS8618
using System.Windows;

namespace Editor;

public class UISizes
{
    public static UISizes main;

    public int spriteCanvasWidth;
    public int spriteCanvasHeight;
    public int spritesheetCanvasWidth;
    public int spritesheetCanvasHeight;
    public int mapCanvasWidth;
    public int scratchCanvasWidth;
    public int mapCanvasHeight;

    public static void Init(Window window)
    {
        Size res = ScreenHelper.GetCurrentScreenResolution(window);
        if (res.Width > 1920 && DpiHelper.GetDpiScaling().DpiX <= 1.5)
        {
            // 4K sizes
            main = new UISizes
            {
                spriteCanvasWidth = 1000,
                spriteCanvasHeight = 800,
                spritesheetCanvasWidth = 1320,
                spritesheetCanvasHeight = 782,
                mapCanvasWidth = 1200,
                scratchCanvasWidth = 750,
                mapCanvasHeight = 980,
            };
        }
        else
        {
            // 1080p sizes
            main = new UISizes
            {
                spriteCanvasWidth = 750,
                spriteCanvasHeight = 600,
                spritesheetCanvasWidth = 900,
                spritesheetCanvasHeight = 600,
                mapCanvasWidth = 880,
                scratchCanvasWidth = 430,
                mapCanvasHeight = 620,
            };
        }
    }
}
