using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using ControlzEx.Theming;
using romsdownload.Data;
using romsdownloader.Classes;

namespace romsdownloader
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                if (!File.Exists(Directories.ConfigFilePath))
                    File.Create(Directories.ConfigFilePath);
            }
            catch
            {
                MessageBox.Show("Can't create config file!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
            }

            IniFile config = new IniFile(Directories.ConfigFilePath);
            if (config.KeyExists("MemoryCacheSize", "Downloads") == false)
                config.Write("MemoryCacheSize", "1024", "Downloads");

            if (config.KeyExists("MaxDownloads", "Downloads") == false)
                config.Write("MaxDownloads", "5", "Downloads");

            if (config.KeyExists("EnableSpeedLimit", "Downloads") == false)
                config.Write("EnableSpeedLimit", "false", "Downloads");

            if (config.KeyExists("SpeedLimit", "Downloads") == false)
                config.Write("SpeedLimit", "200", "Downloads");

            if (config.KeyExists("StartDownloadsOnStartup", "Downloads") == false)
                config.Write("StartDownloadsOnStartup", "true", "Downloads");

            if (config.KeyExists("StartImmediately", "Downloads") == false)
                config.Write("StartImmediately", "true", "Downloads");

            if (config.KeyExists("SelectedStyle", "Theme"))
                ThemeManager.Current.ChangeThemeBaseColor(Application.Current, config.Read("SelectedStyle", "Theme"));

            if (config.KeyExists("SelectedColor", "Theme"))
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, config.Read("SelectedColor", "Theme"));

            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Utility.ExportException(e.Exception);
        }
    }
}