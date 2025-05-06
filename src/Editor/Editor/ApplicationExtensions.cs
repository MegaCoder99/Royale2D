using System.Windows;
using System.Windows.Threading;

namespace Editor;

public static class ApplicationExtensions
{
    public static void BindExceptionHandlers(this Application application)
    {
        // Handle exceptions thrown on the main UI thread.
        application.DispatcherUnhandledException += App_DispatcherUnhandledException;

        // Handle exceptions thrown on non-UI threads.
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        // Handle exceptions thrown in tasks
        TaskScheduler.UnobservedTaskException += (sender, e) =>
        {
            e.SetObserved();
            Prompt.ShowError($"UnobservedTaskException: {e.Exception.Message}");
            Logger.LogException(e.Exception);
        };
    }

    private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // Prevent default unhandled exception processing
        e.Handled = true;
        Prompt.ShowError($"An unhandled exception occurred: {e.Exception.Message}");
        Logger.LogException(e.Exception);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // Log the exception, or perform other error handling here.
        if (e.ExceptionObject is Exception ex)
        {
            Prompt.ShowError($"A non-UI thread exception occurred: {ex.Message}");
            Logger.LogException(ex);
        }
    }
}
