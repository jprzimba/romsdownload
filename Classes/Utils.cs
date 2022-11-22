using System;
using System.IO;

namespace romsdownloader.Classes
{
    internal class Utils
    {
        public static bool IsDirectoryCreated(string folder)
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), folder);
            return Directory.Exists(dir);
        }

        public static void CreateDirectory(string folder)
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), folder);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }

        public static void ExportException(Exception e)
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var file = string.Format("crash_{0:dd-MM-yyyy_HH-mm-ss}.txt", DateTime.Now);
            using (var sw = new StreamWriter(Path.Combine(dir, file)))
                sw.WriteLine(e.ToString());
        }
    }
}