using romsdownload.Data;
using romsdownloader.Views;
using System.IO;
using System.Threading;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace romsdownload.Views
{
    /// <summary>
    /// Lógica interna para StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow
    {
        public static StartupWindow Instance;
        private string DownlaodPath;
        public WinForms.FolderBrowserDialog FolderDialog { get; set; }

        public StartupWindow()
        {
            InitializeComponent();
            Instance = this;
        }

        private void uxBtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderDialog = new WinForms.FolderBrowserDialog();
            FolderDialog.Description = "Select download folder";
            FolderDialog.ShowNewFolderButton = true;

            WinForms.DialogResult result = FolderDialog.ShowDialog();
            if (result == WinForms.DialogResult.OK)
            {
                DownlaodPath = FolderDialog.SelectedPath;
                if (DownlaodPath.EndsWith("\\") == false)
                    DownlaodPath += "\\";

                uxDownloadPathTextBox.Text = DownlaodPath;
            }
            else
            {
                WinForms.MessageBox.Show("You need select download path!", "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Asterisk);
                Application.Current.Shutdown();
            }
        }

        private void uxBtnSavePath_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(uxDownloadPathTextBox.Text))
                return;

            try
            {
                if (!File.Exists(Directories.ConfigFilePath))
                    File.Create(Directories.ConfigFilePath);
            }
            catch
            {
                MessageBox.Show("Can't create config file!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
            }


            IniFile config = new IniFile(Directories.ConfigFilePath);
            config.Write("MemoryCacheSize", "1024", "Downloads");
            config.Write("MaxDownloads", "5", "Downloads");
            config.Write("EnableSpeedLimit", "false", "Downloads");
            config.Write("SpeedLimit", "200", "Downloads");
            config.Write("StartDownloadsOnStartup", "true", "Downloads");
            config.Write("StartImmediately", "true", "Downloads");

            config.Write("DownloadPath", DownlaodPath, "Downloads");

            this.Hide();
            MainWindow mw = new MainWindow();
            mw.Show();
        }
    }
}
