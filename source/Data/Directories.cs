using System;
using System.IO;

namespace romsdownload.Data
{
    public static class Directories
    {
        public static readonly string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public static readonly string DatabasePath = Path.Combine(CurrentDirectory, "database.sqlite");

        public static readonly string DownloadsPath = Path.Combine(CurrentDirectory, "Downloads");
        public static readonly string LogsPath = Path.Combine(CurrentDirectory, "Logs");
        public static readonly string CachePath = Path.Combine(CurrentDirectory, "Cache");
        public static readonly string ImageCachePath = Path.Combine(CachePath, "Images");
    }
}
