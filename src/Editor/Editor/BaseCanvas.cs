using Shared;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Editor;

public abstract class BaseCanvas
{
    public CanvasControl canvasControl;
    public ScrollBar verticalScrollBar => canvasControl.verticalScrollBar;
    public ScrollBar horizontalScrollBar => canvasControl.horizontalScrollBar;
    public SKElement image => canvasControl.image;
    public Canvas canvas => canvasControl.canvas;

    public Color backgroundColor = Color.FromArgb(250, 250, 250);
    public Color outOfBoundsColor = Color.LightGray;

    public bool mouseWheelScroll;

    public double mouseX;
    public double mouseY;

    public int mouseXInt => (int)mouseX;
    public int mouseYInt => (int)mouseY;

    public int tileSize;
    public int TS => tileSize;  // More concise alias for places that reference it a ton in a single line/method
    public int mouseI => mouseYInt / tileSize;
    public int mouseJ => mouseXInt / tileSize;
    public GridCoords GetMouseGridCoords() => new GridCoords(mouseI, mouseJ);

    public double lastClickedMouseX;
    public double lastClickedMouseY;

    public bool isLeftMouseDown;

    // Use mouseX / mouseY for most purposes, these are just for delta calculations
    private double rawMouseX;
    private double rawMouseY;

    public Action<int>? onZoomChange;
    private int _zoom;
    public int zoom { get => _zoom; set { _zoom = value; onZoomChange?.Invoke(value); } }

    public Action<int>? onScrollXChange;
    private int _scrollX;
    public int scrollX { get => _scrollX; set { _scrollX = value; onScrollXChange?.Invoke(value); } }

    public Action<int>? onScrollYChange;
    private int _scrollY;
    public int scrollY { get => _scrollY; set { _scrollY = value; onScrollYChange?.Invoke(value); } }

    public int totalWidth;
    public int totalHeight;

    public int canvasWidth;
    public int canvasHeight;

    public bool isFixed;

    public int lastMouseMoveI = -1;
    public int lastMouseMoveJ = -1;

    private CanvasTool _tool;
    public virtual CanvasTool tool 
    {
        get => _tool;
        set
        {
            _tool?.OnDestroy(this);
            _tool = value;
            OnToolChange(value);
        }
    }
    protected void OnToolChange(CanvasTool newTool)
    {
        if (newTool.cursor != null)
        {
            SetCursor(newTool.cursor);
        }
        else
        {
            SetDefaultCursor();
        }
        InvalidateImage();
    }

    public BaseCanvas(CanvasControl canvasControl, int canvasWidth, int canvasHeight, bool isFixed, int zoom, bool mouseWheelScroll, int tileSize)
    {
        this.canvasControl = canvasControl;
        _tool = GetDefaultTool();

        canvasControl.PaintSurfaceAction = Repaint;

        this.isFixed = isFixed;
        this.zoom = zoom;
        this.mouseWheelScroll = mouseWheelScroll;
        this.tileSize = tileSize;

        Resize(canvasWidth, canvasHeight, canvasWidth, canvasHeight);

        verticalScrollBar.ValueChanged += VerticalScrollBar_ValueChanged;
        horizontalScrollBar.ValueChanged += HorizontalScrollBar_ValueChanged;

        canvas.MouseMove += OnMouseMoveRawHandler;
        canvas.MouseDown += OnMouseDownRawHandler;
        canvas.MouseUp += OnMouseUpRawHandler;
        canvas.MouseEnter += OnMouseEnterRawHandler;
        canvas.MouseLeave += OnMouseLeaveRawHandler;
        canvas.KeyDown += OnKeyDownRawHandler;
        canvas.KeyUp += OnKeyUpRawHandler;
        canvas.MouseWheel += OnMouseWheelRawHandler;

        canvas.GotFocus += (sender, e) =>
        {
            //canvasControl.canvasBorder.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 86, 157, 229));
        };

        canvas.LostFocus += (sender, e) =>
        {
            //canvasControl.canvasBorder.BorderBrush = System.Windows.Media.Brushes.Transparent;
        };
    }

    #region misc

    public void ChangeToDefaultTool()
    {
        tool = GetDefaultTool();
    }

    public virtual CanvasTool GetDefaultTool()
    {
        return new SelectTool();
    }

    public virtual MyPoint GetAnchorPoint()
    {
        return new MyPoint(mouseXInt, mouseYInt);
    }

    public bool ResizeTotal(int totalWidth, int totalHeight)
    {
        int totalWidthToUse = Math.Max(totalWidth, canvasWidth);
        int totalHeightToUse = Math.Max(totalHeight, canvasHeight);
        if (this.totalWidth == totalWidthToUse && this.totalHeight == totalHeightToUse) return false;

        Resize(canvasWidth, canvasHeight, totalWidthToUse, totalHeightToUse);
        return true;
    }

    public void Resize(int canvasWidth, int canvasHeight, int totalWidth, int totalHeight)
    {
        this.canvasWidth = canvasWidth;
        this.canvasHeight = canvasHeight;

        this.totalWidth = totalWidth;
        this.totalHeight = totalHeight;

        canvas.Width = this.canvasWidth;
        canvas.Height = this.canvasHeight;

        image.Width = this.canvasWidth;
        image.Height = this.canvasHeight;

        horizontalScrollBar.Width = this.canvasWidth;

        UpdateScrollBars();
    }

    public bool RectInBounds(MyRect rect)
    {
        return rect.x2 > scrollX && rect.y2 > scrollY && rect.x1 < scrollX + canvasWidth && rect.y1 < scrollY + canvasHeight;
    }
    #endregion

    #region drawing

    public void StartAnimation()
    {
        canvasControl.StartAnimation();
    }

    public void StopAnimation()
    {
        canvasControl.StopAnimation();
    }

    // Forces refresh/redraw of the image control
    public void InvalidateImage()
    {
        image.InvalidateVisual();
    }

    private void Repaint(SKPaintSurfaceEventArgs e)
    {
        (float dpiX, float dpiY) = DpiHelper.GetDpiScaling();

        SKCanvas canvas = e.Surface.Canvas;

        canvas.ResetMatrix();
        canvas.Scale(zoom * dpiX, zoom * dpiY);
        canvas.Translate(-scrollX, -scrollY);

        CanvasDrawer canvasDrawer = new CanvasDrawer(canvas, canvasWidth, canvasHeight, this);

        canvasDrawer.Clear(outOfBoundsColor);

        Drawer? editorImageDrawer = GetEditorImageDrawer();
        if (editorImageDrawer != null)
        {
            canvasDrawer.DrawImage(editorImageDrawer, 0, 0);
        }

        DrawToCanvas(canvasDrawer);

        // Tool drawing action
        if (!tool.redrawUsesZoomScroll)
        {
            canvas.ResetMatrix();
            canvas.Scale(dpiX, dpiY);
        }
        tool.OnRedraw(this, canvasDrawer);
    }

    // Gets the primary image to draw on the canvas. Consumers of BaseCanvas override this. Generally, this should not result in rendering something every invalidate cycle.
    // The reason is because this method/system is designed for optimized, pre-cached, large images that represent the entire width/height of the canvas UI.
    // Anything in this image should be affected by zoom/scroll. Typically, this is the "meat" of what you're showing to the user in your canvas UI.
    // For example, for the map editor, it would be the map layers/tiles, basically the world image, minus some special "tooling" layers
    // For the sprite editor, it would be the spritesheet contents, the sprite animation, minus some special tooling gizmos.
    public virtual Drawer? GetEditorImageDrawer()
    {
        return null;
    }

    // Another overridable draw method, but things drawn here are designed to be rendered every invalidate cycle, so they should be fast to draw.
    // If they get out of control in terms of perf issues, they should be moved to the "pre-cached" GetEditorImageDrawer() method/system.
    // As of right now, stuff drawn here is also affected by zoom/scroll, with no way to opt-out.
    public virtual void DrawToCanvas(Drawer canvasDrawer)
    {
    }

    #endregion

    #region input
    public void Focus()
    {
        canvas.Focus();
        Keyboard.Focus(canvas);
    }

    private void OnMouseMoveRawHandler(object sender, MouseEventArgs e)
    {
        var control = sender as UIElement;
        var position = e.GetPosition(control);

        mouseX = (position.X / zoom) + scrollX;
        mouseY = (position.Y / zoom) + scrollY;

        double lastRawMouseX = rawMouseX;
        double lastRawMouseY = rawMouseY;

        rawMouseX = (int)position.X;
        rawMouseY = (int)position.Y;

        if (Helpers.SpaceHeld() && !isFixed)
        {
            ChangeScrollPos(scrollX + (int)(lastRawMouseX - rawMouseX), scrollY + (int)(lastRawMouseY - rawMouseY));
            InvalidateImage();
        }

        if (mouseI != lastMouseMoveI || mouseJ != lastMouseMoveJ)
        {
            lastMouseMoveI = mouseI;
            lastMouseMoveJ = mouseJ;
            tool.OnMouseGridCoordsChange(this);
        }

        OnMouseMove();
        tool.OnMouseMove(this);
    }

    public virtual void OnMouseMove()
    {
    }

    private void OnMouseDownRawHandler(object sender, MouseButtonEventArgs e)
    {
        Focus();
        if (e.ChangedButton == MouseButton.Left)
        {
            lastClickedMouseX = mouseX;
            lastClickedMouseY = mouseY;
            isLeftMouseDown = true;
            OnLeftMouseDown();
            tool.OnLeftMouseDown(this);
        }
        else if (e.ChangedButton == MouseButton.Right)
        {
            lastClickedMouseX = mouseX;
            lastClickedMouseY = mouseY;
            OnRightMouseDown();
        }
        else if (e.ChangedButton == MouseButton.Middle)
        {
            OnMiddleMouseDown();
        }
    }

    public virtual void OnLeftMouseDown()
    {
    }

    public virtual void OnRightMouseDown()
    {
    }

    public virtual void OnMiddleMouseDown()
    {
    }

    private void OnMouseUpRawHandler(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            isLeftMouseDown = false;
            OnLeftMouseUp();
            tool.OnLeftMouseUp(this);
        }
        else if (e.ChangedButton == MouseButton.Right)
        {
            OnRightMouseUp();
            tool.OnRightMouseUp(this);
        }
        else if (e.ChangedButton == MouseButton.Middle)
        {
            tool.OnMiddleMouseUp(this);
        }
    }

    public virtual void OnLeftMouseUp()
    {
    }

    public virtual void OnRightMouseUp()
    {
    }

    public bool isMouseOver;
    private void OnMouseEnterRawHandler(object sender, MouseEventArgs e)
    {
        isMouseOver = true;
        OnMouseEnter();
    }

    public virtual void OnMouseEnter()
    {
        Focus();
    }

    private void OnMouseLeaveRawHandler(object sender, MouseEventArgs e)
    {
        isMouseOver = false;
        Mouse.OverrideCursor = null;
        OnMouseLeave();
        tool.OnMouseLeave(this);
        isLeftMouseDown = false;
    }

    public virtual void OnMouseLeave()
    {
    }

    private void OnMouseWheelRawHandler(object sender, MouseWheelEventArgs e)
    {
        if (mouseWheelScroll && !Helpers.ControlHeld())
        {
            int scrollAmount = -Math.Sign(e.Delta) * 50;
            ChangeScrollPos(scrollX, scrollY + scrollAmount);
            InvalidateImage();
            return;
        }
        int delta = Math.Sign(e.Delta);
        ChangeZoom(zoom + delta);
        InvalidateImage();
    }

    private void OnKeyDownRawHandler(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        var key = e.Key;

        if (key == Key.Space && !isFixed)
        {
            SetCursor(Cursors.ScrollAll);
        }

        OnKeyDown(key);
    }

    public virtual void OnKeyDown(Key key)
    {
    }

    private void OnKeyUpRawHandler(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        var keyCode = e.Key;

        if (keyCode == Key.Space)
        {
            SetDefaultCursor();
        }
    }

    public virtual void OnKeyUp(Key key)
    {
    }

    public void SetCursor(Cursor cursor)
    {
        canvas.Cursor = cursor;
    }

    public void SetDefaultCursor()
    {
        SetCursor(Cursors.Arrow);
    }
    #endregion

    #region zoom/scroll
    bool inProgramaticScrollUpdate;
    public void UpdateScrollBars()
    {
        if (isFixed)
        {
            verticalScrollBar.Visibility = Visibility.Collapsed;
            horizontalScrollBar.Visibility = Visibility.Collapsed;
            return;
        }

        inProgramaticScrollUpdate = true;

        verticalScrollBar.Maximum = ((totalHeight * zoom) - canvasHeight);
        horizontalScrollBar.Maximum = ((totalWidth * zoom) - canvasWidth);

        verticalScrollBar.ViewportSize = canvasHeight;
        horizontalScrollBar.ViewportSize = canvasWidth;

        verticalScrollBar.SmallChange = (canvasHeight / 20);
        horizontalScrollBar.SmallChange = (canvasWidth / 20);
        verticalScrollBar.LargeChange = (canvasHeight / 10);
        horizontalScrollBar.LargeChange = (canvasWidth / 10);

        verticalScrollBar.Value = scrollY * zoom;
        horizontalScrollBar.Value = scrollX * zoom;

        inProgramaticScrollUpdate = false;
    }

    private void VerticalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (inProgramaticScrollUpdate) return;
        scrollY = (int)(e.NewValue / zoom);
        ClampScroll();
        InvalidateImage();
    }

    private void HorizontalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (inProgramaticScrollUpdate) return;
        scrollX = (int)(e.NewValue / zoom);
        ClampScroll();
        InvalidateImage();
    }

    public void ChangeZoom(int newZoom)
    {
        if (newZoom > 5 || newZoom < 1 || zoom == newZoom) return;
        this.zoom = newZoom;
        AnchorScroll();
        ClampScroll();
        UpdateScrollBars();
    }

    public void ChangeScrollPos(int x, int y)
    {
        if (scrollX == x && scrollY == y) return;
        scrollX = x;
        scrollY = y;
        ClampScroll();
        UpdateScrollBars();
    }

    public void AnchorScroll()
    {
        MyPoint anchorPoint = GetAnchorPoint();
        scrollX = anchorPoint.x - ((canvasWidth / zoom) / 2);
        scrollY = anchorPoint.y - ((canvasHeight / zoom) / 2);
    }

    public void ClampScroll()
    {
        scrollX = MyMath.Clamp(scrollX, 0, Math.Max(totalWidth - (canvasWidth / zoom), 0));
        scrollY = MyMath.Clamp(scrollY, 0, Math.Max(totalHeight - (canvasHeight / zoom), 0));
    }

    public void CenterScrollToPos(int x, int y)
    {
        ChangeScrollPos(x - (canvasWidth / 2) / zoom, y - (canvasHeight / 2) / zoom);
        InvalidateImage();
    }

    public (int, int) ConvertAbsoluteToViewport(int absoluteX, int absoluteY)
    {
        int adjustedX = absoluteX - scrollX;
        int adjustedY = absoluteY - scrollY;

        int viewportX = (int)(adjustedX * (double)zoom);
        int viewportY = (int)(adjustedY * (double)zoom);

        return (viewportX, viewportY);
    }
    #endregion
}
