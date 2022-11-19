using System;
using System.IO;

namespace romsdownloader.Classes
{
    internal class Utils
    {
        public static void CreateDownloadDirectory()
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "Downloads");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
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