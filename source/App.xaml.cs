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
            Utility.CreateFileFromResource(Directories.ConfigFilePath, "romsdownload.Resources.config.xml");


            var configCorrupted = false;
            try
            {
                Config.Instance = ((Config)Utility.MapXmlFileToClass(typeof(Config), Directories.ConfigFilePath));
            }
            catch (Exception)
            {
                configCorrupted = true;
            }


            if (!configCorrupted)
            {
                try
                {
                    if (File.Exists(Directories.ConfigFilePath + ".bak"))
                    {
                        File.Delete(Directories.ConfigFilePath + ".bak");
                    }
                    File.Copy(Directories.ConfigFilePath, Directories.ConfigFilePath + ".bak");
                    File.SetAttributes(Directories.ConfigFilePath + ".bak", FileAttributes.Hidden);
                }
                catch (Exception)
                {
                    //ignore
                }
            }
            else
            {
                try
                {
                    Config.Instance = ((Config)Utility.MapXmlFileToClass(typeof(Config), Directories.ConfigFilePath + ".bak"));
                    File.Delete(Directories.ConfigFilePath);
                    File.Copy(Directories.ConfigFilePath + ".bak", Directories.ConfigFilePath);
                    File.SetAttributes(Directories.ConfigFilePath, FileAttributes.Normal);
                }
                catch (Exception)
                {
                    File.Delete(Directories.ConfigFilePath + ".bak");
                    File.Delete(Directories.ConfigFilePath);
                    MessageBox.Show("Couldn't load config.xml.");
                    Environment.Exit(0);
                }
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