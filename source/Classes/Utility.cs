using MahApps.Metro.Controls.Dialogs;
using romsdownload.Data;
using romsdownloader.Views;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;

namespace romsdownloader.Classes
{
    internal class Utility
    {
        public static string GenerateRandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdfghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static void CreateDirectories()
        {
            try
            {
                var path = Directories.DownloadsPath;
                var cmd = Database.Connection().CreateCommand();
                var sql = "SELECT * FROM Downloads";
                cmd.CommandText = sql;
                SQLiteDataReader readerConfig = cmd.ExecuteReader();
                while (readerConfig.Read())
                {
                    //memorycachesize, maxdownloads, enablespeedlimit, speedlimit, startdownloadsonstartup, startimmediately, downloadpath

                    if (readerConfig.GetString(7) != string.Empty)
                        path = readerConfig.GetString(7);

                    if (path == string.Empty)
                    {
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                    }
                }

                path = Directories.LogsPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Directories.CachePath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Path.Combine(Directories.CachePath, Directories.ImageCachePath);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex) 
            {
                MainWindow.Instance.ShowMessageAsync("Error", ex.Message);
                Application.Current.Shutdown();
            }
        }

        public static void ExportException(Exception e)
        {
            var dir = Directories.LogsPath;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var file = string.Format("crash_{0:dd-MM-yyyy_HH-mm-ss}.txt", DateTime.Now);
            using (var sw = new StreamWriter(Path.Combine(dir, file)))
                sw.WriteLine(e.ToString());
        }

        public static string GetExtensionFromUrl(string url)
        {
            string ext = String.Empty;
            if (url != string.Empty)
                ext = Path.GetExtension(url);

            return ext;
        }
    }
}