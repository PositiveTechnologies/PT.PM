using Avalonia;
using Avalonia.Controls;
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
            var appBuilder = AppBuilder.Configure<App>().UseSkia();
            appBuilder = Helper.IsRunningOnLinux ? appBuilder.UseGtk() : appBuilder.UseWin32();
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
