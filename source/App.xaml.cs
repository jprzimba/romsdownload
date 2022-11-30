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
            if (!File.Exists(Directories.ConfigFilePath))
            {
                using (StreamWriter writer = new StreamWriter(Directories.ConfigFilePath))
                {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    writer.WriteLine("<Config xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
                    writer.WriteLine("</Config>");
                    writer.Close();
                }
            }

            try
            {
                Config.Instance = ((Config)Utility.MapXmlFileToClass(typeof(Config), Directories.ConfigFilePath));
            }
            catch (Exception)
            {

            }


            if (Config.Instance.SelectedStyle != null)
            {
                ThemeManager.Current.ChangeThemeBaseColor(Application.Current, Config.Instance.SelectedStyle);
            }

            if (Config.Instance.SelectedColor != null)
            {
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, Config.Instance.SelectedColor);
            }

            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Utility.ExportException(e.Exception);
        }
    }
}