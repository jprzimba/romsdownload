using System;
using System.IO;

namespace romsdownload.Data
{
    public static class Directories
    {
        public static readonly string CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public static readonly string ConfigFilePath = Path.Combine(CurrentDirectory, "config.xml");
    }
}
