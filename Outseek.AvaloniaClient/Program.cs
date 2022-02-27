using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.ReactiveUI;

namespace Outseek.AvaloniaClient;

class Program
{
    [DllImport("kernel32", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread] // required for drag&drop of external files, see https://github.com/AvaloniaUI/Avalonia/issues/2635
    public static int Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            // makes output be printed to the console on windows
            AttachConsole(-1);
        return BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
}
