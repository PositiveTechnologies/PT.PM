using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Markup.Xaml;
using PT.PM.Common;

namespace PT.PM.PatternEditor
{
    class App : Application
    {
        public override void Initialize()
        {
            ServiceLocator.Settings = Settings.Load();

            AvaloniaXamlLoader.Load(this);
            base.Initialize();
        }

        static void Main(string[] args)
        {
            // TODO: use it when Skia will be work on Linux
            // var appBuilder = AppBuilder.Configure<App>().UseSkia();
            // appBuilder = Helper.IsRunningOnLinux ? appBuilder.UseGtk() : appBuilder.UseWin32();
            var appBuilder = AppBuilder.Configure<App>();
            if (Helper.IsRunningOnLinux)
            {
                appBuilder.UsePlatformDetect();
            }
            else
            {
                appBuilder.UseSkia().UseWin32();
            }
            appBuilder.Start<MainWindow>();
        }

        public static void AttachDevTools(Window window)
        {
#if DEBUG
            DevTools.Attach(window);
#endif
        }
    }
}
