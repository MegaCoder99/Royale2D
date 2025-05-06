using Shared;

namespace Editor;

public class ExportedPixelRect
{
    public Drawer drawer;
    public string sortKey;

    // These are populated by the export packing process
    public int newSpritesheetNum;
    public MyRect? newRect;

    public ExportedPixelRect(Drawer drawer, string sortKey = "")
    {
        this.drawer = drawer;
        this.sortKey = sortKey;
    }
}

public class ImagePacker
{
    FolderPath exportFolderPath;
    string exportImageFileName;
    int maxImageSize;

    public ImagePacker(FolderPath exportFolderPath, string exportImageFileName, int maxImageSize)
    {
        this.exportFolderPath = exportFolderPath;
        this.exportImageFileName = exportImageFileName;
        this.maxImageSize = maxImageSize;
    }

    public void PackExportImages(List<ExportedPixelRect> exportedPixelRects, bool disposeDrawers, bool newRowPerSortKey)
    {
        int maxRowHeight = 0;
        int padding = 1;
        int padding2x = padding * 2;
        int currentX = 0;
        int currentY = 0;
        int newSpritesheetNum = 0;

        // Clone for safety, then sort by sortKey and if tie, then height
        exportedPixelRects = new(exportedPixelRects);
        exportedPixelRects.Sort(
            (a, b) =>
            {
                if (a.sortKey == b.sortKey)
                {
                    return a.drawer.height.CompareTo(b.drawer.height);
                }
                else
                {
                    return a.sortKey.CompareToNatural(b.sortKey);
                }
            }
        );

        List<BitmapDrawer> newSpritesheetDrawers = [new BitmapDrawer(maxImageSize, maxImageSize)];
        List<MyPoint> lastDrawnBotRightPoint = [new MyPoint(0, 0)];
        string prevSortKey = "";

        foreach (ExportedPixelRect exportedPixelRect in exportedPixelRects)
        {
            int w = exportedPixelRect.drawer.width;
            int h = exportedPixelRect.drawer.height;

            bool newSpriteRow = newRowPerSortKey && (prevSortKey != exportedPixelRect.sortKey);

            if (currentX + padding2x + w > maxImageSize || newSpriteRow)
            {
                currentX = 0;
                currentY += maxRowHeight + padding2x;
                maxRowHeight = 0;
            }

            prevSortKey = exportedPixelRect.sortKey;

            if (h > maxRowHeight)
            {
                maxRowHeight = h;
            }

            if (currentY + padding + h >= maxImageSize)
            {
                currentY = 0;
                newSpritesheetNum++;
                newSpritesheetDrawers.Add(new BitmapDrawer(maxImageSize, maxImageSize));
                lastDrawnBotRightPoint.Add(new MyPoint(0, 0));
            }

            exportedPixelRect.newRect = new MyRect(
                currentX + padding,
                currentY + padding,
                currentX + padding + w,
                currentY + padding + h);

            newSpritesheetDrawers[newSpritesheetNum].DrawImage(
                exportedPixelRect.drawer,
                currentX + padding,
                currentY + padding);

            lastDrawnBotRightPoint[newSpritesheetNum] = new MyPoint(
                Math.Max(lastDrawnBotRightPoint[newSpritesheetNum].x, exportedPixelRect.newRect.Value.x2),
                Math.Max(lastDrawnBotRightPoint[newSpritesheetNum].y, exportedPixelRect.newRect.Value.y2));

            currentX += padding2x + w;
            exportedPixelRect.newSpritesheetNum = newSpritesheetNum;
        }

        for (int i = 0; i < newSpritesheetDrawers.Count; i++)
        {
            // NOTE: -1, -1 was once removed in Resize call below to fix a bug. Hopefully this doesn't cause other bugs elsewhere, but leaving this comment for reference.
            newSpritesheetDrawers[i].Resize(lastDrawnBotRightPoint[i].x, lastDrawnBotRightPoint[i].y);
            string fileName = GetExportImageFileName(i);
            newSpritesheetDrawers[i].SaveBitmapToDisk(exportFolderPath.AppendFile(fileName).fullPath);
            newSpritesheetDrawers[i].Dispose();
        }

        if (disposeDrawers)
        {
            foreach (ExportedPixelRect exportedPixelRect in exportedPixelRects)
            {
                exportedPixelRect.drawer.Dispose();
            }
        }
    }

    public string GetExportImageFileName(int newSpritesheetNum)
    {
        return exportImageFileName + (newSpritesheetNum == 0 ? "" : (newSpritesheetNum + 1).ToString()) + ".png";
    }
}
