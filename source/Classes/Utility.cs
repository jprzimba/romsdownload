using MahApps.Metro.Controls.Dialogs;
using romsdownload.Data;
using romsdownloader.Views;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace romsdownloader.Classes
{
    internal class Utility
    {
        private NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;

        public static void CreateFileFromResource(string path, string resource, bool overwrite = false)
        {
            if (!overwrite && File.Exists(path))
            {
                return;
            }
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        using (var sw = new StreamWriter(path, false, Encoding.UTF8))
                        {
                            sw.Write(reader.ReadToEnd());
                        }
                    }
                }
            }
        }

        public static void MapClassToXmlFile(Type type, object obj, string path)
        {
            var serializer = new XmlSerializer(type);
            using (var sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                serializer.Serialize(sw, obj);
            }
        }

        public static object MapXmlFileToClass(Type type, string path)
        {
            var serializer = new XmlSerializer(type);
            using (var reader = new StreamReader(path, Encoding.UTF8))
            {
                return serializer.Deserialize(reader);
            }
        }

        public static void CreateDirectories()
        {
            try
            {
                var path = Directories.DownloadsPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Directories.LogsPath;
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Directories.CachePath;
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