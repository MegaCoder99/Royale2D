using Shared;
using SkiaSharp;
using System.Drawing;
using System.IO;
using System.Windows;

namespace Editor;

// Abstracts an image and draw operations on it.
// Always use base Drawer in parameters, fields, callbacks and whatnot for simplicity and maintainability
public abstract class Drawer
{
    protected virtual SKCanvas GetCanvas() { throw new NotImplementedException(); }
    protected virtual void DisposeCanvas(SKCanvas canvas) { canvas.Dispose(); }
    public virtual int width => throw new NotImplementedException();
    public virtual int height => throw new NotImplementedException();
    public virtual void Clear(Color color) { }
    public virtual void Dispose() { }

    public void DrawRect(MyRect rect, Color? fillColor, Color? strokeColor = null, float strokeWidth = 0, float fillAlpha = 1, float offX = 0, float offY = 0)
    {
        SKCanvas canvas = GetCanvas();
        if (fillColor != null)
        {
            SKColor skFillColor = new SKColor(fillColor.Value.R, fillColor.Value.G, fillColor.Value.B, (byte)(fillColor.Value.A * fillAlpha));
            using (SKPaint fillPaint = new SKPaint { Color = skFillColor, Style = SKPaintStyle.Fill })
            {
                canvas.DrawRect(rect.x1 + offX, rect.y1 + offY, rect.w, rect.h, fillPaint);
            }
        }

        if (strokeColor != null && strokeWidth > 0)
        {
            SKColor skStrokeColor = new SKColor(strokeColor.Value.R, strokeColor.Value.G, strokeColor.Value.B, strokeColor.Value.A);
            using (SKPaint strokePaint = new SKPaint { Color = skStrokeColor, Style = SKPaintStyle.Stroke, StrokeWidth = strokeWidth })
            {
                canvas.DrawRect(rect.x1 + offX, rect.y1 + offY, rect.w, rect.h, strokePaint);
            }
        }
        DisposeCanvas(canvas);
    }

    public void DrawRects(List<MyRect> rects, Color? fillColor, Color? strokeColor = null, float strokeWidth = 0, float fillAlpha = 1, float offX = 0, float offY = 0)
    {
        if (rects.Count == 0) return;
        SKCanvas canvas = GetCanvas();
        if (fillColor != null)
        {
            SKColor skFillColor = new SKColor(fillColor.Value.R, fillColor.Value.G, fillColor.Value.B, (byte)(fillColor.Value.A * fillAlpha));
            using (SKPaint fillPaint = new SKPaint { Color = skFillColor, Style = SKPaintStyle.Fill })
            {
                foreach (MyRect rect in rects)
                {
                    canvas.DrawRect(rect.x1 + offX, rect.y1 + offY, rect.w, rect.h, fillPaint);
                }
            }
        }

        if (strokeColor != null && strokeWidth > 0)
        {
            SKColor skStrokeColor = new SKColor(strokeColor.Value.R, strokeColor.Value.G, strokeColor.Value.B, strokeColor.Value.A);
            using (SKPaint strokePaint = new SKPaint { Color = skStrokeColor, Style = SKPaintStyle.Stroke, StrokeWidth = strokeWidth })
            {
                foreach (MyRect rect in rects)
                {
                    canvas.DrawRect(rect.x1 + offX, rect.y1 + offY, rect.w, rect.h, strokePaint);
                }
            }
        }
        DisposeCanvas(canvas);
    }

    // SKTextAlign already exists for horizontal, but the library doesn't provide a vertical one for some reason, so we make our own
    public enum SKTextAlignV
    {
        Top,
        Middle,
        Bottom,
    }

    public void DrawText(string text, float x, float y, Color? fillColor, Color? strokeColor, float size,
        float strokeWidth = 1.5f, SKTextAlign hAlign = SKTextAlign.Left, SKTextAlignV vAlign = SKTextAlignV.Top)
    {
        SKCanvas canvas = GetCanvas();

        using (SKPaint paint = new SKPaint
        {
            TextSize = size,
            IsAntialias = true
        })
        {
            // Set Horizontal Alignment
            paint.TextAlign = hAlign;

            // Get Font Metrics for Vertical Alignment
            SKFontMetrics metrics;
            paint.GetFontMetrics(out metrics);

            float adjustedY = y;

            switch (vAlign)
            {
                case SKTextAlignV.Top:
                    adjustedY += -metrics.Ascent;
                    break;
                case SKTextAlignV.Middle:
                    adjustedY += (-metrics.Ascent + metrics.Descent) / 2;
                    break;
                case SKTextAlignV.Bottom:
                    adjustedY += -metrics.Descent;
                    break;
            }

            // Draw Stroke
            if (strokeColor != null)
            {
                SKColor skStrokeColor = new SKColor(strokeColor.Value.R, strokeColor.Value.G, strokeColor.Value.B, strokeColor.Value.A);
                using (SKPaint strokePaint = paint.Clone())
                {
                    strokePaint.Color = skStrokeColor;
                    strokePaint.Style = SKPaintStyle.Stroke;
                    strokePaint.StrokeWidth = strokeWidth;
                    canvas.DrawText(text, x, adjustedY, strokePaint);
                }
            }

            // Draw Fill
            if (fillColor != null)
            {
                SKColor skFillColor = new SKColor(fillColor.Value.R, fillColor.Value.G, fillColor.Value.B, fillColor.Value.A);
                using (SKPaint textPaint = paint.Clone())
                {
                    textPaint.Color = skFillColor;
                    textPaint.Style = SKPaintStyle.Fill;
                    canvas.DrawText(text, x, adjustedY, textPaint);
                }
            }
        }

        DisposeCanvas(canvas);
    }

    public void DrawCircle(float x, float y, float r, Color? fillColor, Color? lineColor = null, float lineThickness = 1)
    {
        SKCanvas canvas = GetCanvas();
        if (fillColor != null)
        {
            SKColor skFillColor = new SKColor(fillColor.Value.R, fillColor.Value.G, fillColor.Value.B, fillColor.Value.A);
            using (SKPaint fillPaint = new SKPaint { Color = skFillColor, Style = SKPaintStyle.Fill })
            {
                canvas.DrawCircle(x, y, r, fillPaint);
            }
        }

        if (lineColor != null && lineThickness > 0)
        {
            SKColor skLineColor = new SKColor(lineColor.Value.R, lineColor.Value.G, lineColor.Value.B, lineColor.Value.A);
            using (SKPaint linePaint = new SKPaint { Color = skLineColor, Style = SKPaintStyle.Stroke, StrokeWidth = lineThickness })
            {
                canvas.DrawCircle(x, y, r, linePaint);
            }
        }
        DisposeCanvas(canvas);
    }

    public void DrawPolygon(MyShape shape, bool fill, Color? fillColor, Color? lineColor, int lineThickness = 1, float alpha = 1, float offX = 0, float offY = 0)
    {
        SKCanvas canvas = GetCanvas();

        var pts = shape.points;
        SKPoint[] points = pts.ConvertAll(point => new SKPoint(point.x + offX, point.y + offY)).ToArray();

        using (SKPath path = new SKPath())
        {
            // Start from the first point
            path.MoveTo(points[0]);

            // Draw lines connecting each point in the polygon
            for (int i = 1; i < points.Length; i++)
            {
                path.LineTo(points[i]);
            }

            // Close the path to complete the polygon
            path.Close();

            // Fill the polygon if requested
            if (fill && fillColor != null)
            {
                SKColor skFillColor = new SKColor(fillColor.Value.R, fillColor.Value.G, fillColor.Value.B, (byte)(fillColor.Value.A * alpha));
                using (SKPaint fillPaint = new SKPaint { Color = skFillColor, Style = SKPaintStyle.Fill })
                {
                    canvas.DrawPath(path, fillPaint);
                }
            }

            /*
            // Draw the polygon outline if requested
            if (lineColor != null)
            {
                SKColor skLineColor = new SKColor(lineColor.Value.R, lineColor.Value.G, lineColor.Value.B, (byte)(lineColor.Value.A * alpha));
                using (SKPaint linePaint = new SKPaint { Color = skLineColor, Style = SKPaintStyle.Stroke, StrokeWidth = lineThickness })
                {
                    canvas.DrawPath(path, linePaint);
                }
            }
            */
        }

        DisposeCanvas(canvas);
    }

    public void DrawLine(float x, float y, float x2, float y2, Color? color, float thickness, bool antialias = true)
    {
        SKCanvas canvas = GetCanvas();
        if (color != null)
        {
            SKColor skColor = new SKColor(color.Value.R, color.Value.G, color.Value.B, color.Value.A);
            using (SKPaint linePaint = new SKPaint { Color = skColor, StrokeWidth = thickness, Style = SKPaintStyle.Stroke, IsAntialias = antialias })
            {
                canvas.DrawLine(x, y, x2, y2, linePaint);
            }
        }
        DisposeCanvas(canvas);
    }

    public void DrawImage(Drawer drawer, float dx, float dy, float sx = 0, float sy = 0, float sw = -1, float sh = -1,
    float alpha = 1, bool overwritePixels = false, bool flipX = false, bool flipY = false, bool flipAroundCenter = false)
    {
        if (drawer is BitmapDrawer bitmapDrawer)
        {
            SKBitmap skBitmapToDraw = bitmapDrawer.skBitmap;
            SKCanvas canvas = GetCanvas();
            using (SKPaint paint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, (byte)(255 * alpha)),
                IsAntialias = true,
                BlendMode = overwritePixels ? SKBlendMode.Src : SKBlendMode.SrcOver
            })
            {
                sw = (sw == -1 ? skBitmapToDraw.Width : sw);
                sh = (sh == -1 ? skBitmapToDraw.Height : sh);

                SKRect srcRect = new SKRect(sx, sy, sx + sw, sy + sh);
                SKRect destRect = new SKRect(dx, dy, dx + sw, dy + sh);

                if (flipX || flipY)
                {
                    canvas.Save();
                    SKMatrix matrix = SKMatrix.Identity;

                    // Calculate the pivot point based on flipAroundCenter parameter
                    float pivotX = flipAroundCenter ? dx + sw / 2 : dx;
                    float pivotY = flipAroundCenter ? dy + sh / 2 : dy;

                    if (flipX)
                    {
                        matrix = matrix.PostConcat(SKMatrix.CreateScale(-1, 1, pivotX, pivotY));
                    }
                    if (flipY)
                    {
                        matrix = matrix.PostConcat(SKMatrix.CreateScale(1, -1, pivotX, pivotY));
                    }
                    canvas.SetMatrix(matrix);
                }

                canvas.DrawBitmap(skBitmapToDraw, srcRect, destRect, paint);

                if (flipX || flipY)
                {
                    canvas.Restore();
                }
            }
            DisposeCanvas(canvas);
        }
    }
}

public class CanvasDrawer : Drawer
{
    public SKCanvas preProvidedCanvas;
    public BaseCanvas baseCanvas;
    public int preProvidedCanvasW;
    public int preProvidedCanvasH;
    protected override SKCanvas GetCanvas() => preProvidedCanvas;
    public override int width => preProvidedCanvasW;
    public override int height => preProvidedCanvasH;

    public CanvasDrawer(SKCanvas preProvidedCanvas, int preProvidedCanvasW, int preProvidedCanvasH, BaseCanvas baseCanvas)
    {
        this.preProvidedCanvas = preProvidedCanvas;
        this.preProvidedCanvasW = preProvidedCanvasW;
        this.preProvidedCanvasH = preProvidedCanvasH;
        this.baseCanvas = baseCanvas;
    }

    public override void Clear(Color color)
    {
        SKColor skColor = new SKColor(color.R, color.G, color.B, color.A);
        preProvidedCanvas.Clear(skColor);
    }

    // Do not dispose canvas, it's pre-provided and provider manages it
    protected override void DisposeCanvas(SKCanvas canvas) { }
}

public class BitmapDrawer : Drawer, IDisposable
{
    public SKBitmap skBitmap;
    protected override SKCanvas GetCanvas() => new SKCanvas(skBitmap);
    public override int width => skBitmap.Width;
    public override int height => skBitmap.Height;

    public BitmapDrawer(SKBitmap skBitmap)
    {
        this.skBitmap = skBitmap;
        // For some reason, SKBitmap doesn't throw exception when it fails to create bitmap, so we need to check it manually everywhere
        if (skBitmap == null) throw new Exception("Failed to create bitmap.");
    }

    public BitmapDrawer(FilePath imageFilePath)
    {
        skBitmap = SKBitmap.Decode(imageFilePath.fullPath);
        if (skBitmap == null) throw new Exception("Failed to create bitmap.");
    }

    public BitmapDrawer(Uri imageResourceUri)
    {
        System.Windows.Resources.StreamResourceInfo resourceInfo = Application.GetResourceStream(imageResourceUri);

        // Load the stream into an SKBitmap
        using (var stream = resourceInfo.Stream)
        {
            skBitmap = SKBitmap.Decode(stream);
        }
        if (skBitmap == null) throw new Exception("Failed to create bitmap.");
    }

    public static BitmapDrawer FromBase64Encoding(string encoding)
    {
        byte[] data = Convert.FromBase64String(encoding);
        using (MemoryStream stream = new MemoryStream(data))
        {
            return new BitmapDrawer(SKBitmap.Decode(stream));
        }
    }

    public BitmapDrawer(int width, int height)
    {
        skBitmap = new SKBitmap(width, height);
    }

    public override void Clear(Color color)
    {
        SKColor skColor = new SKColor(color.R, color.G, color.B, color.A);
        skBitmap.Erase(skColor);
    }

    public void Resize(int newWidth, int newHeight)
    {
        SKBitmap resizedBitmap = new SKBitmap(newWidth, newHeight);
        using (SKCanvas canvas = new SKCanvas(resizedBitmap))
        {
            canvas.Clear(SKColors.Transparent);
            SKRect sourceRect = new(0, 0, newWidth, newHeight);
            SKRect destRect = new(0, 0, newWidth, newHeight);
            canvas.DrawBitmap(skBitmap, sourceRect, destRect);
        }
        skBitmap.Dispose();
        skBitmap = resizedBitmap;
    }

    public BitmapDrawer[,] Split(int maxWidth, int maxHeight)
    {
        int rows = (int)Math.Ceiling((float)height / maxHeight);
        int cols = (int)Math.Ceiling((float)width / maxWidth);
        BitmapDrawer[,] pieces = new BitmapDrawer[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                int x = j * maxWidth;
                int y = i * maxHeight;
                int w = Math.Min(maxWidth, width - x);
                int h = Math.Min(maxHeight, height - y);
                SKBitmap subBitmap = new SKBitmap(w, h);
                using (SKCanvas canvas = new SKCanvas(subBitmap))
                {
                    canvas.Clear(SKColors.Transparent);
                    SKRect sourceRect = new(x, y, x + w, y + h);
                    SKRect destRect = new(0, 0, w, h);
                    canvas.DrawBitmap(skBitmap, sourceRect, destRect);
                }
                pieces[i, j] = new BitmapDrawer(subBitmap);
            }
        }

        return pieces;
    }

    public bool Trim(out int left, out int right, out int top, out int bottom)
    {
        left = skBitmap.Width;
        right = 0;
        top = skBitmap.Height;
        bottom = 0;

        // Iterate through all pixels to find bounds of non-white area
        for (int y = 0; y < skBitmap.Height; y++)
        {
            for (int x = 0; x < skBitmap.Width; x++)
            {
                SKColor pixel = skBitmap.GetPixel(x, y);

                if (pixel.Alpha > 0)
                {
                    if (x < left) left = x;
                    if (x > right) right = x;
                    if (y < top) top = y;
                    if (y > bottom) bottom = y;
                }
            }
        }

        // No non-transparent pixels found
        if (left > right || top > bottom)
        {
            return false;
        }

        // Calculate width and height of the trimmed bitmap
        int trimmedWidth = right - left + 1;
        int trimmedHeight = bottom - top + 1;

        // Create a new bitmap with the trimmed size
        SKBitmap trimmedBitmap = new SKBitmap(trimmedWidth, trimmedHeight);
        using (SKCanvas canvas = new SKCanvas(trimmedBitmap))
        {
            // Draw the trimmed portion of the original bitmap onto the new one
            SKRect sourceRect = new(left, top, right + 1, bottom + 1);
            SKRect destRect = new(0, 0, trimmedWidth, trimmedHeight);
            canvas.DrawBitmap(skBitmap, sourceRect, destRect);
        }

        // Replace the original bitmap with the trimmed bitmap
        skBitmap.Dispose();
        skBitmap = trimmedBitmap;

        return true;
    }

    public bool Trim(int left, int right, int top, int bottom)
    {
        // Iterate through all pixels to find bounds of non-white area
        for (int y = 0; y < skBitmap.Height; y++)
        {
            for (int x = 0; x < skBitmap.Width; x++)
            {
                SKColor pixel = skBitmap.GetPixel(x, y);

                if (pixel.Alpha > 0)
                {
                    if (x < left) left = x;
                    if (x > right) right = x;
                    if (y < top) top = y;
                    if (y > bottom) bottom = y;
                }
            }
        }

        // No non-transparent pixels found
        if (left > right || top > bottom)
        {
            return false;
        }

        // Calculate width and height of the trimmed bitmap
        int trimmedWidth = right - left + 1;
        int trimmedHeight = bottom - top + 1;

        // Create a new bitmap with the trimmed size
        SKBitmap trimmedBitmap = new SKBitmap(trimmedWidth, trimmedHeight);
        using (SKCanvas canvas = new SKCanvas(trimmedBitmap))
        {
            // Draw the trimmed portion of the original bitmap onto the new one
            SKRect sourceRect = new(left, top, right + 1, bottom + 1);
            SKRect destRect = new(0, 0, trimmedWidth, trimmedHeight);
            canvas.DrawBitmap(skBitmap, sourceRect, destRect);
        }

        // Replace the original bitmap with the trimmed bitmap
        skBitmap.Dispose();
        skBitmap = trimmedBitmap;

        return true;
    }

    public string GetHash()
    {
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            // Use a MemoryStream to collect pixel data
            using (var memoryStream = new System.IO.MemoryStream())
            {
                for (int y = 0; y < skBitmap.Height; y++)
                {
                    for (int x = 0; x < skBitmap.Width; x++)
                    {
                        // Get the pixel color
                        SKColor pixel = skBitmap.GetPixel(x, y);

                        // Write the color data (RGBA) as bytes
                        memoryStream.WriteByte(pixel.Red);
                        memoryStream.WriteByte(pixel.Green);
                        memoryStream.WriteByte(pixel.Blue);
                        memoryStream.WriteByte(pixel.Alpha);
                    }
                }

                // Compute the hash from the pixel data
                byte[] hash = sha256.ComputeHash(memoryStream.ToArray());

                // Convert the hash to a hexadecimal string
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    public void SaveBitmapToDisk(string filePath, SKEncodedImageFormat format = SKEncodedImageFormat.Png, int quality = 100)
    {
        // Encode the bitmap to the desired image format
        using (var image = SKImage.FromBitmap(skBitmap))
        using (var data = image.Encode(format, quality))
        {
            // Save the encoded data to a file
            using (var stream = File.OpenWrite(filePath))
            {
                data.SaveTo(stream);
            }
        }
    }
    public Color GetPixel(int x, int y)
    {
        SKColor skColor = skBitmap.GetPixel(x, y);
        return Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue);
    }

    public void SetPixel(int x, int y, Color color)
    {
        // Draw a pixel to the bitmap
        skBitmap.SetPixel(x, y, new SKColor(color.R, color.G, color.B, color.A));
    }

    private bool _disposed = false;

    // If possible, call Dispose() manually when done with the drawer to free up memory/resources.
    // Otherwise, destructor will be called when it goes out of scope but can't control when that happens, leading to potential perf hiccups
    // It is not always feasible to call Dispose() manually, since Drawers can be deeply embedded in undo stack action closures, so just rely on GC/destructor for those
    public override void Dispose()
    {
        DisposeInternal();
        GC.SuppressFinalize(this);  // For optimizing GC to not run destructor
    }

    ~BitmapDrawer()
    {
        DisposeInternal();
    }

    private void DisposeInternal()
    {
        if (_disposed) return;
        _disposed = true;
        skBitmap.Dispose();
    }
}
