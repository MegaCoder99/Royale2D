using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Editor;

public static class ScreenHelper
{
    /// <summary>
    /// Gets the screen resolution for the monitor containing the specified window
    /// </summary>
    /// <param name="window">The window to check</param>
    /// <returns>Size containing the screen width and height in pixels</returns>
    public static Size GetCurrentScreenResolution(Window window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        var monitor = GetMonitorFromWindow(window);
        return new Size(monitor.Info.rcMonitor.Width, monitor.Info.rcMonitor.Height);
    }

    /// <summary>
    /// Gets detailed screen information for the monitor containing the specified window
    /// </summary>
    /// <param name="window">The window to check</param>
    /// <returns>ScreenInfo containing resolution and DPI information</returns>
    public static ScreenInfo GetCurrentScreenInfo(Window window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        var monitor = GetMonitorFromWindow(window);

        // Get DPI scaling
        var presentationSource = PresentationSource.FromVisual(window);
        var dpiScale = 1.0;
        if (presentationSource?.CompositionTarget != null)
        {
            dpiScale = presentationSource.CompositionTarget.TransformToDevice.M11;
        }

        return new ScreenInfo
        {
            PhysicalResolution = new Size(
                monitor.Info.rcMonitor.Width,
                monitor.Info.rcMonitor.Height
            ),
            WorkingArea = new Size(
                monitor.Info.rcWork.Width,
                monitor.Info.rcWork.Height
            ),
            IsPrimary = monitor.Info.dwFlags.HasFlag(MonitorInfoFlags.PRIMARY),
            DeviceName = new string(monitor.Info.szDevice).TrimEnd('\0'),  // Convert char array to string and trim null chars
            DpiScale = dpiScale,
            ScaledResolution = new Size(
                monitor.Info.rcMonitor.Width / dpiScale,
                monitor.Info.rcMonitor.Height / dpiScale
            )
        };
    }

    private static MonitorDetails GetMonitorFromWindow(Window window)
    {
        var windowInteropHelper = new WindowInteropHelper(window);
        var monitorHandle = NativeMethods.MonitorFromWindow(
            windowInteropHelper.Handle,
            NativeMethods.MONITOR_DEFAULTTONEAREST);

        var monitorInfo = new NativeMethods.MONITORINFOEX();
        monitorInfo.cbSize = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));
        NativeMethods.GetMonitorInfo(monitorHandle, ref monitorInfo);

        return new MonitorDetails { Handle = monitorHandle, Info = monitorInfo };
    }
}

public class ScreenInfo
{
    public Size PhysicalResolution { get; set; }
    public Size WorkingArea { get; set; }
    public Size ScaledResolution { get; set; }
    public bool IsPrimary { get; set; }
    public string DeviceName { get; set; } = "";
    public double DpiScale { get; set; }
}

internal static class NativeMethods
{
    public const int MONITOR_DEFAULTTONEAREST = 2;

    [DllImport("user32.dll")]
    public static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MONITORINFOEX
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public MonitorInfoFlags dwFlags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szDevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }
}

[Flags]
internal enum MonitorInfoFlags : uint
{
    PRIMARY = 1
}

internal class MonitorDetails  // Renamed from MonitorInfo to avoid conflict
{
    public IntPtr Handle { get; set; }
    public NativeMethods.MONITORINFOEX Info { get; set; }  // Renamed from MonitorInfo to Info
}