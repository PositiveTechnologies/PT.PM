using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Logging.Serilog;
using Avalonia.Markup.Xaml;
using Serilog;

namespace PT.PM.PatternEditor
{
    class App : Application
    {
        public override void Initialize()
        {
            Utils.RegisterAllParsersAndConverters();
            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }

        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            InitializeLogging();
            AppBuilder.Configure<App>()
                .UseReactiveUI()
                .UsePlatformDetect()
                .Start<MainWindow>();
        }

        public static void AttachDevTools(Window window)
        {
#if DEBUG
            DevTools.Attach(window);
#endif
        }

        private static void InitializeLogging()
        {
#if DEBUG
            SerilogLogger.Initialize(new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.Trace(outputTemplate: "{Area}: {Message}")
                .CreateLogger());
#endif
        }
    }
}
