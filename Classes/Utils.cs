using System;
using System.Globalization;
using System.IO;

namespace romsdownloader.Classes
{
    internal class Utils
    {
        private NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;

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

        // Return the amount of free disk space on a given partition
        private string GetFreeDiskSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    long freeSpace = drive.AvailableFreeSpace;
                    double mbFreeSpace = (double)freeSpace / Math.Pow(1024, 2);
                    double gbFreeSpace = mbFreeSpace / 1024D;

                    if (freeSpace < Math.Pow(1024, 3))
                    {
                        return mbFreeSpace.ToString("#.00", numberFormat) + " MB";
                    }
                    return gbFreeSpace.ToString("#.00", numberFormat) + " GB";
                }
            }
            return String.Empty;
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