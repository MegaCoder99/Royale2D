using System.Windows;

namespace Editor;

public static class DpiHelper
{
    private static (float DpiX, float DpiY) _cachedDpiScaling = (1.0f, 1.0f);
    private static bool _isInitialized = false;

    /// <summary>
    /// Initializes the DPI helper to start tracking DPI changes.
    /// Should be called once during application startup.
    /// </summary>
    public static void Init(Window mainWindow)
    {
        if (_isInitialized) return;

        if (mainWindow == null)
            throw new ArgumentNullException(nameof(mainWindow));

        UpdateDpiScaling(mainWindow);

        // Listen for DPI changes
        mainWindow.DpiChanged += (sender, args) =>
        {
            UpdateDpiScaling(mainWindow);
        };

        _isInitialized = true;
    }

    /// <summary>
    /// Gets the current DPI scaling factors.
    /// </summary>
    /// <returns>A tuple containing the X and Y scaling factors.</returns>
    public static (float DpiX, float DpiY) GetDpiScaling()
    {
        if (!_isInitialized)
        {
            return (1.0f, 1.0f);
        }

        return _cachedDpiScaling;
    }

    /// <summary>
    /// Updates the cached DPI scaling factors for the specified window.
    /// </summary>
    /// <param name="window">The window to retrieve the DPI scaling from.</param>
    private static void UpdateDpiScaling(Window window)
    {
        PresentationSource source = PresentationSource.FromVisual(window);
        if (source?.CompositionTarget != null)
        {
            _cachedDpiScaling = (
                (float)source.CompositionTarget.TransformToDevice.M11,
                (float)source.CompositionTarget.TransformToDevice.M22
            );
        }
        else
        {
            _cachedDpiScaling = (1.0f, 1.0f);
        }
    }
}