using Shared;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor;

public class Helpers : SharedHelpers
{
    public static int GetColorDifference(Color color1, Color color2)
    {
        return Math.Abs(color1.R - color2.R) +
               Math.Abs(color1.G - color2.G) +
               Math.Abs(color1.B - color2.B) +
               Math.Abs(color1.A - color2.A);
    }

    // Normalize a list of numbers to be as close to zero as possible
    IEnumerable<int> NormalizeList(IEnumerable<int> numbers)
    {
        if (numbers.All(n => n >= 0))
        {
            return numbers.Select(n => n - numbers.Min());
        }
        else if (numbers.All(n => n <= 0))
        {
            return numbers.Select(n => n - numbers.Max());
        }
        else
        {
            return numbers;
        }
    }

    public static bool FocusedElementAllowsGlobalHotkeys(Window window, KeyEventArgs e)
    {
        IInputElement? focusedElement = FocusManager.GetFocusedElement(window);
        Key key = e.Key;

        //Console.WriteLine(focusedElement?.GetType()?.ToString() ?? "null");

        if (key == Key.Tab || key == Key.Space) return false;

        if (focusedElement is Canvas) return true;
        if (focusedElement == null) return true;

        if (key == Key.Escape)
        {
            FocusManager.SetFocusedElement(window, window);
            return false;
        }

        if (focusedElement is ListBox || focusedElement is ListBoxItem)
        {
            if (key == Key.Up || key == Key.Down) return false;
            e.Handled = true;
            return true;
        }
        if (focusedElement is TextBox textBox && textBox.IsEnabled && !textBox.IsReadOnly)
        {
            return false;
        }
        if (focusedElement is CheckBox)
        {
            if (key == Key.Space) return false;
            e.Handled = true;
            return true;
        }

        e.Handled = true;
        return true;
    }

    public static void RestartApplication(params string[] launchArgs)
    {
        // Get the current process
        var currentProcess = Process.GetCurrentProcess();

        // Get the path to the current executable
        var executablePath = currentProcess.MainModule?.FileName;
        if (string.IsNullOrEmpty(executablePath))
        {
            Prompt.ShowError("Unable to restart the application.");
            return;
        }

        // Start a new instance of the application
        Process.Start(new ProcessStartInfo
        {
            FileName = executablePath,
            UseShellExecute = true, // Ensures the process is started in the correct context
            Arguments = LaunchArgs.GetBundledArgs(launchArgs)
        });

        // Shut down the current application
        Application.Current.Shutdown();
    }

    public static bool ShiftHeld()
    {
        return (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));
    }

    public static bool ControlHeld()
    {
        return (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
    }

    public static bool SpaceHeld()
    {
        return Keyboard.IsKeyDown(Key.Space);
    }

    public static bool AltHeld()
    {
        return Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
    }

    public static string ColorToHexString(System.Drawing.Color color)
    {
        return (color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2")).ToLowerInvariant();
    }

    public static System.Drawing.Color HexStringToColor(string hexString)
    {
        hexString = hexString.TrimStart('#');
        if (hexString.Length != 6)
        {
            throw new ArgumentException("Hex string must be 6 characters long.");
        }

        byte r = byte.Parse(hexString.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hexString.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hexString.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return System.Drawing.Color.FromArgb(r, g, b);
    }

    public static void AssertFailed(string message)
    {
#if DEBUG
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
        else
        {
            Prompt.ShowError(message);
        }
#endif
    }
}
