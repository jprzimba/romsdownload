using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using ControlzEx.Theming;
using romsdownload.Data;
using romsdownload.Views;
using romsdownloader.Classes;
using romsdownloader.Views;

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

        private void Application_Startup(object sender, StartupEventArgs e)
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

            Window start;
            IniFile config = new IniFile(Directories.ConfigFilePath);
            if (config.KeyExists("SelectedStyle", "Theme"))
                ThemeManager.Current.ChangeThemeBaseColor(Application.Current, config.Read("SelectedStyle", "Theme"));

            if (config.KeyExists("SelectedColor", "Theme"))
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, config.Read("SelectedColor", "Theme"));


            if (config.KeyExists("DownloadPath", "Downloads"))
            {
                start = new MainWindow();
                start.Show();
            }
            else
            {
                start = new StartupWindow();
                start.Show();
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Utility.ExportException(e.Exception);
        }
    }
}