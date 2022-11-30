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