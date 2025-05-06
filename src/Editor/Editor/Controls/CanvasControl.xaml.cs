using SkiaSharp.Views.Desktop;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Editor;

/// <summary>
/// Interaction logic for CanvasControl.xaml
/// </summary>
public partial class CanvasControl : UserControl
{
    public Action<SKPaintSurfaceEventArgs>? PaintSurfaceAction;
    public Action<double>? AnimateAction;
    private readonly Stopwatch _stopwatch = new Stopwatch();
    private double _lastRenderTime;
    private bool _isAnimating;

    public CanvasControl()
    {
        InitializeComponent();
        CompositionTarget.Rendering += CompositionTarget_Rendering;
        this.Unloaded += CanvasControl_Unloaded;
    }

    public void ImagePaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        PaintSurfaceAction?.Invoke(e);
    }

    public void StartAnimation()
    {
        _stopwatch.Start();
        _isAnimating = true;
    }

    public void StopAnimation()
    {
        _stopwatch.Stop();
        _stopwatch.Reset();
        _lastRenderTime = 0;
        _isAnimating = false;
    }

    private void CompositionTarget_Rendering(object? sender, EventArgs e)
    {
        if (!_isAnimating) return;

        double currentTime = _stopwatch.Elapsed.TotalMilliseconds;
        double deltaTime = currentTime - _lastRenderTime;
        _lastRenderTime = currentTime;

        AnimateAction?.Invoke(deltaTime);
    }

    // Clean up
    private void CanvasControl_Unloaded(object sender, RoutedEventArgs e)
    {
        CompositionTarget.Rendering -= CompositionTarget_Rendering;
        this.Unloaded -= CanvasControl_Unloaded;
        _stopwatch.Stop();
    }
}
