using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using Shared;
using System.Drawing.Imaging;

namespace Editor;

public class BitmapHelpers
{
    // Use this instead of new Bitmap everywhere. Has important converison code
    public static Bitmap CreateBitmapFromFile(FilePath filePath)
    {
        var bitmap = new Bitmap(filePath.fullPath);
        if (IsIndexed(bitmap.PixelFormat))
        {
            return ConvertToRgb(bitmap);
        }
        else
        {
            return bitmap;
        }
    }

    // Check if the image is in an indexed format
    static bool IsIndexed(PixelFormat format)
    {
        return format == PixelFormat.Format1bppIndexed ||
               format == PixelFormat.Format4bppIndexed ||
               format == PixelFormat.Format8bppIndexed;
    }

    // Convert an indexed bitmap to RGB format
    static Bitmap ConvertToRgb(Bitmap indexedBitmap)
    {
        // Create a new RGB Bitmap with the same dimensions
        Bitmap rgbBitmap = new Bitmap(indexedBitmap.Width, indexedBitmap.Height, PixelFormat.Format32bppArgb);

        // Copy the original DPI to avoid unintended scaling
        rgbBitmap.SetResolution(indexedBitmap.HorizontalResolution, indexedBitmap.VerticalResolution);

        using (Graphics g = Graphics.FromImage(rgbBitmap))
        {
            g.DrawImage(indexedBitmap, 0, 0);
        }

        return rgbBitmap;
    }

    public static Bitmap? GetBitmapFromClipboard(out bool isSourceMapEditor)
    {
        isSourceMapEditor = false;

        if (Clipboard.ContainsImage())
        {
            BitmapSource bitmapSource = GetImageFromClipboardInternal(out isSourceMapEditor);
            if (bitmapSource != null)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Save the BitmapSource as a PNG to a memory stream
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    encoder.Save(memoryStream);

                    // Create a Bitmap from the memory stream
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return new Bitmap(memoryStream);
                }
            }
        }

        MessageBox.Show("Clipboard does not contain an image.");
        return null;
    }

    public static void SetBitmapToClipboard(Bitmap bitmap)
    {
        // Lock the bitmap's bits
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

        try
        {
            // Check if the bitmap uses 32 bits per pixel (supports alpha transparency)
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new InvalidOperationException("Only 32bppArgb format is supported for transparency.");
            }

            // Create a BitmapSource from the raw bitmap data
            BitmapSource bitmapSource = BitmapSource.Create(
                bitmap.Width,
                bitmap.Height,
                bitmap.HorizontalResolution,
                bitmap.VerticalResolution,
                System.Windows.Media.PixelFormats.Bgra32, // BGRA format to match ARGB
                null,
                bitmapData.Scan0,
                bitmapData.Stride * bitmap.Height,
                bitmapData.Stride);

            SetBitmapToClipoardInternal(bitmapSource);
        }
        finally
        {
            // Unlock the bitmap's bits
            bitmap.UnlockBits(bitmapData);
        }
    }

    public static void SetBitmapToClipoardInternal(BitmapSource image)
    {
        DataObject dataObject = new DataObject();
        dataObject.SetImage(image); // Standard image data
        dataObject.SetData("Source", "MapEditor"); // Custom metadata (e.g., app name)

        Clipboard.SetDataObject(dataObject, true); // Store both image & metadata
    }

    public static BitmapSource GetImageFromClipboardInternal(out bool isSourceMapEditor)
    {
        // Get image from clipboard
        BitmapSource image = Clipboard.GetImage(); // Works as usual

        // Get metadata separately
        IDataObject clipboardData = Clipboard.GetDataObject();
        string? sourceApp = clipboardData?.GetData("Source") as string;

        isSourceMapEditor = (sourceApp == "MapEditor");

        return image;
    }
}