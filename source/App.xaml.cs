using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using ControlzEx.Standard;
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
                if (!File.Exists(Directories.DatabasePath))
                    Database.CreateDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            LoadConfigs();
        }

        private void LoadConfigs()
        {
            try
            {
                //Theme
                var cmd = Database.Connection().CreateCommand();
                var sql = "SELECT * FROM Theme";
                cmd.CommandText = sql;
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string style = reader.GetString(0);//style
                    string color = reader.GetString(1);//theme
                    ThemeManager.Current.ChangeThemeBaseColor(Application.Current, style);
                    ThemeManager.Current.ChangeThemeColorScheme(Application.Current, color);
                }

                //Startup
                Window start;
                cmd = Database.Connection().CreateCommand();
                sql = "SELECT * FROM Downloads";
                cmd.CommandText = sql;
                SQLiteDataReader readerConfig = cmd.ExecuteReader();
                while (readerConfig.Read())
                {
                    //memorycachesize, maxdownloads, enablespeedlimit, speedlimit, startdownloadsonstartup, startimmediately, downloadpath
                    string downloadpath = readerConfig.GetString(7);
                    if (downloadpath != string.Empty)
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                if (Database.Connection().State == ConnectionState.Open)
                    Database.Connection().Close();
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Utility.ExportException(e.Exception);
        }
    }
}