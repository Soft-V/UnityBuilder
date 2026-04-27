using Avalonia;
using System;
using System.Threading;

namespace UnityBuilder.Desktop
{
    internal sealed class Program
    {
        private static Mutex? _mutex;
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            _mutex = new Mutex(true, "UnityBuilder.Desktop", out var isNewInstance);
            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            finally
            {
                _mutex?.ReleaseMutex();
                _mutex?.Dispose();
            }

        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
