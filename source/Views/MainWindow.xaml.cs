using System;
using System.Diagnostics;
using System.Windows;
using romsdownload.Models;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using HtmlAgilityPack;
using romsdownload.Properties;
using romsdownloader.Classes;
using romsdownload.Download;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.Specialized;
using Microsoft.Win32;
using System.Xml.Linq;
using System.Net;
using MahApps.Metro.Controls.Dialogs;
using romsdownload.Views.Settings;
using romsdownload.Data;
using System.Data;
using romsdownload.Views;
using System.Data.SQLite;

namespace romsdownloader.Views
{
    public partial class MainWindow
    {
        #region Declarations 
        public static MainWindow Instance;

        public string GamePlataform { get; private set; }
        public string GameName { get; private set; }
        public string GameUrl { get; private set; }
        public string CoverImage { get; private set; }

        private List<string> propertyNames;
        private List<string> propertyValues;
        private List<PropertyModel> propertiesList;

        private bool _shutdown;

        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            Utility.CreateDirectories();
            // Bind DownloadsList to downloadsGrid
            downloadsGrid.ItemsSource = DownloadManager.Instance.DownloadsList;
            DownloadManager.Instance.DownloadsList.CollectionChanged += new NotifyCollectionChangedEventHandler(DownloadsList_CollectionChanged);

            // In case of computer shutdown or restart, save current list of downloads to an XML file
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);

            propertyNames = new List<string>();
            propertyNames.Add("URL");
            propertyNames.Add("Supports Resume");
            propertyNames.Add("File Type");
            propertyNames.Add("Download Folder");
            propertyNames.Add("Average Speed");
            propertyNames.Add("Total Time");

            propertyValues = new List<string>();
            propertiesList = new List<PropertyModel>();

            // Load downloads from the XML file
            LoadDownloadsFromXml();

            if (DownloadManager.Instance.TotalDownloads == 0)
            {
                //EnableMenuItems(false);

                var folder = Directories.DownloadsPath;
                var dir = Path.Combine(Directory.GetCurrentDirectory(), folder);

                var cmd = Database.Connection().CreateCommand();
                var sql = "SELECT * FROM Downloads";
                cmd.CommandText = sql;
                SQLiteDataReader readerConfig = cmd.ExecuteReader();
                while (readerConfig.Read())
                {
                    //memorycachesize, maxdownloads, enablespeedlimit, speedlimit, startdownloadsonstartup, startimmediately, downloadpath
                    if (readerConfig.GetString(7) != String.Empty)
                    {
                        folder = readerConfig.GetString(7);
                        dir = folder;
                    }
                }

                // Clean temporary files in the download directory if no downloads were loaded
                if (Directory.Exists(dir))
                {
                    DirectoryInfo downloadLocation = new DirectoryInfo(dir);
                    foreach (FileInfo file in downloadLocation.GetFiles())
                    {
                        if (file.FullName.EndsWith(".tmp"))
                            file.Delete();
                    }
                }
            }

            uxTabGameList.Visibility = Visibility.Hidden;
            uxTabCurrentDownloads.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Load & Save
        private void PauseAllDownloads()
        {
            if (downloadsGrid.Items.Count > 0)
            {
                foreach (WebDownloadClient download in DownloadManager.Instance.DownloadsList)
                {
                    download.Pause();
                }
            }
        }

        private void SaveDownloadsToXml()
        {
            if (DownloadManager.Instance.TotalDownloads > 0)
            {
                // Pause downloads
                PauseAllDownloads();

                XElement root = new XElement("downloads");

                foreach (WebDownloadClient download in DownloadManager.Instance.DownloadsList)
                {
                    string username = String.Empty;
                    string password = String.Empty;
                    if (download.ServerLogin != null)
                    {
                        username = download.ServerLogin.UserName;
                        password = download.ServerLogin.Password;
                    }

                    XElement xdl = new XElement("download",
                                        new XElement("file_name", download.FileName),
                                        new XElement("url", download.Url.ToString()),
                                        new XElement("username", username),
                                        new XElement("password", password),
                                        new XElement("temp_path", download.TempDownloadPath),
                                        new XElement("file_size", download.FileSize),
                                        new XElement("downloaded_size", download.DownloadedSize),
                                        new XElement("status", download.Status.ToString()),
                                        new XElement("status_text", download.StatusText),
                                        new XElement("total_time", download.TotalElapsedTime.ToString()),
                                        new XElement("added_on", download.AddedOn.ToString()),
                                        new XElement("completed_on", download.CompletedOn.ToString()),
                                        new XElement("supports_resume", download.SupportsRange.ToString()),
                                        new XElement("has_error", download.HasError.ToString()),
                                        new XElement("open_file", download.OpenFileOnCompletion.ToString()),
                                        new XElement("temp_created", download.TempFileCreated.ToString()),
                                        new XElement("is_batch", download.IsBatch.ToString()),
                                        new XElement("url_checked", download.BatchUrlChecked.ToString()));
                    root.Add(xdl);
                }

                XDocument xd = new XDocument();
                xd.Add(root);
                // Save downloads to XML file
                xd.Save("Downloads.xml");
            }
        }

        private void LoadDownloadsFromXml()
        {
            try
            {
                if (File.Exists("Downloads.xml"))
                {
                    // Load downloads from XML file
                    XElement downloads = XElement.Load("Downloads.xml");
                    if (downloads.HasElements)
                    {
                        IEnumerable<XElement> downloadsList =
                            from el in downloads.Elements()
                            select el;
                        foreach (XElement download in downloadsList)
                        {
                            // Create WebDownloadClient object based on XML data
                            WebDownloadClient downloadClient = new WebDownloadClient(download.Element("url").Value);

                            downloadClient.FileName = download.Element("file_name").Value;

                            downloadClient.DownloadProgressChanged += downloadClient.DownloadProgressChangedHandler;
                            downloadClient.DownloadCompleted += downloadClient.DownloadCompletedHandler;
                            downloadClient.PropertyChanged += this.PropertyChangedHandler;
                            downloadClient.StatusChanged += this.StatusChangedHandler;
                            downloadClient.DownloadCompleted += this.DownloadCompletedHandler;

                            string username = download.Element("username").Value;
                            string password = download.Element("password").Value;
                            if (username != String.Empty && password != String.Empty)
                            {
                                downloadClient.ServerLogin = new NetworkCredential(username, password);
                            }

                            downloadClient.TempDownloadPath = download.Element("temp_path").Value;
                            downloadClient.FileSize = Convert.ToInt64(download.Element("file_size").Value);
                            downloadClient.DownloadedSize = Convert.ToInt64(download.Element("downloaded_size").Value);

                            DownloadManager.Instance.DownloadsList.Add(downloadClient);

                            if (download.Element("status").Value == "Completed")
                            {
                                downloadClient.Status = DownloadStatus.Completed;
                            }
                            else
                            {
                                downloadClient.Status = DownloadStatus.Paused;
                            }

                            downloadClient.StatusText = download.Element("status_text").Value;

                            downloadClient.ElapsedTime = TimeSpan.Parse(download.Element("total_time").Value);
                            downloadClient.AddedOn = DateTime.Parse(download.Element("added_on").Value);
                            downloadClient.CompletedOn = DateTime.Parse(download.Element("completed_on").Value);

                            downloadClient.SupportsRange = Boolean.Parse(download.Element("supports_resume").Value);
                            downloadClient.HasError = Boolean.Parse(download.Element("has_error").Value);
                            downloadClient.OpenFileOnCompletion = Boolean.Parse(download.Element("open_file").Value);
                            downloadClient.TempFileCreated = Boolean.Parse(download.Element("temp_created").Value);
                            downloadClient.IsBatch = Boolean.Parse(download.Element("is_batch").Value);
                            downloadClient.BatchUrlChecked = Boolean.Parse(download.Element("url_checked").Value);

                            bool startDownloadsOnStartup = true;
                            var cmd = Database.Connection().CreateCommand();
                            var sql = "SELECT * FROM Downloads";
                            cmd.CommandText = sql;
                            SQLiteDataReader readerConfig = cmd.ExecuteReader();
                            while (readerConfig.Read())
                            {
                                //memorycachesize, maxdownloads, enablespeedlimit, speedlimit, startdownloadsonstartup, startimmediately, downloadpath
                                
                                if (readerConfig.GetInt32(5) > 0)
                                    startDownloadsOnStartup = true;
                            }

                            if (downloadClient.Status == DownloadStatus.Paused && !downloadClient.HasError && startDownloadsOnStartup)
                            {
                                downloadClient.Start();
                            }
                        }

                        // Create empty XML file
                        XElement root = new XElement("downloads");
                        XDocument xd = new XDocument();
                        xd.Add(root);
                        xd.Save("Downloads.xml");
                    }
                }
            }
            catch (Exception)
            {
                Instance.ShowMessageAsync("Error", "There was an error while loading the download list.");
            }
        }
        #endregion

        #region Window Events
        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            SaveDownloadsToXml();
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !_shutdown;
            if (_shutdown) return;

            var metroDialogSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Quit",
                NegativeButtonText = "Cancel",
                AnimateShow = true,
                AnimateHide = false
            };

            var result = await this.ShowMessageAsync(
                "Quit application?",
                    "Sure you want to quit application?",
                        MessageDialogStyle.AffirmativeAndNegative, metroDialogSettings);

            _shutdown = result == MessageDialogResult.Affirmative;

            if (_shutdown)
            {
                SaveDownloadsToXml();
                Application.Current.Shutdown();
            }

        }
        #endregion

        #region Load Games
        private async Task ScrapGames(string url, string plataform)
        {
            TransformControls(false);
            GamePlataform = plataform;

        SCRAP:
            var Webget = new HtmlWeb();
            var doc = Webget.Load(url);
            //Search game
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
            {
                GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                GameName = GameName.Replace(" ROM", "");
                GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                if (CoverImage.StartsWith("data:image"))
                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                string extension = Utility.GetExtensionFromUrl(CoverImage);
                string ImagePath = Utility.GenerateRandomString(9) + "_" + GamePlataform +  extension;
                string CachedImageName = Path.Combine(Directories.ImageCachePath, ImagePath);

                if (!File.Exists(CachedImageName))
                {
                    WebClient client = new WebClient();
                    await client.DownloadFileTaskAsync(new Uri(CoverImage), CachedImageName);
                }

                Database.Add(GameName, CachedImageName, GameUrl, GamePlataform);
                statusBarDownloads.Content = "Downloading " + GameName;
            }

            //Next Page
            var nextPage = string.Empty;
            var checkNextPage = string.Empty;
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
            {

                checkNextPage = node.InnerText;
                if (checkNextPage.Equals(">"))
                {
                    nextPage = node.GetAttributeValue("href", "");
                    nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                    url = nextPage;
                    goto SCRAP;
                }
            }

            await LoadGames(GamePlataform);
            TransformControls(true);
            statusBarDownloads.Content = "Loaded " + GamePlataform + " games!";
        }

        private async Task LoadGames(string plataform)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1));
            TransformControls(false);
            DataSet ds = new DataSet();
            ds = Database.GetGamesByPlataform(plataform.ToUpper());

            uxGamesListView.ItemsSource = ds.Tables[0].DefaultView;
            statusBarDownloads.Content = "Loaded " + plataform + " games. Total Games: " + uxGamesListView.Items.Count.ToString();
            TransformControls(true);

        }
        #endregion

        #region Functions
        private void TransformControls(bool status)
        {
            uxComboPlataform.IsEnabled = status;
            uxGamesListView.IsEnabled = status;
            uxTextBoxSearch.IsEnabled = status;
            uxMainTabControl.IsEnabled = status;
        }
        #endregion

        #region Control Events
        private async void uxGamesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await GrabDownloadAndStart(sender);
        }

        private async Task GrabDownloadAndStart(object sender)
        {
            try
            {
                ListView list = (ListView)sender;
                DataRowView item = list.SelectedItem as DataRowView;
                if (item != null)
                {
                    string url = item.Row["url"].ToString();
                    var Webget = new HtmlWeb();
                    var doc = Webget.Load(url);

                    string _downloadlink = string.Empty;

                    //Search download button
                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//a[@id=\"btnDownload\"]"))
                    {
                        if (node.InnerText.EndsWith("(fast)"))
                            _downloadlink = Settings.Default.ROMSPEDIA_BASE_URL + node.GetAttributeValue("href", "");
                    }

                    //Search download link
                    Webget = new HtmlWeb();
                    doc = Webget.Load(_downloadlink);
                    _downloadlink = string.Empty;

                    foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"col-12\"]//p//a"))
                    {
                        if (node.InnerText.Equals("click here"))
                            _downloadlink = node.GetAttributeValue("href", "");
                    }

                    WebDownloadClient download = new WebDownloadClient(_downloadlink);
                    download.FileName = item.Row["title"].ToString().Trim();

                    // Register WebDownloadClient events
                    download.DownloadProgressChanged += download.DownloadProgressChangedHandler;
                    download.DownloadCompleted += download.DownloadCompletedHandler;
                    download.PropertyChanged += this.PropertyChangedHandler;
                    download.StatusChanged += this.StatusChangedHandler;
                    download.DownloadCompleted += this.DownloadCompletedHandler;

                    // Create path to temporary file
                    var folder = Directories.DownloadsPath;
                    var cmd = Database.Connection().CreateCommand();
                    var sql = "SELECT * FROM Downloads";
                    cmd.CommandText = sql;
                    SQLiteDataReader readerConfig = cmd.ExecuteReader();
                    while (readerConfig.Read())
                    {
                        //memorycachesize, maxdownloads, enablespeedlimit, speedlimit, startdownloadsonstartup, startimmediately, downloadpath
                        if (readerConfig.GetString(7) != String.Empty)
                            folder = readerConfig.GetString(7);
                    }

                    string extension = Utility.GetExtensionFromUrl(_downloadlink);

                    var realFileName = download.FileName + extension;
                    var filePath = Path.Combine(folder, realFileName);
                    string tempPath = filePath + ".tmp";

                    // Check if there is already an ongoing download on that path
                    if (File.Exists(tempPath))
                    {
                        await this.ShowMessageAsync(
                            "Error",
                                "There is already a download in progress at the specified path.");
                        return;
                    }

                    // Check if the file already exists
                    if (File.Exists(filePath))
                    {
                        var result = await this.ShowMessageAsync(
                            "Warning", "There is already a file with the same name, do you want to overwrite it? "
                                       + "If not, please change the file name or download folder.",
                                MessageDialogStyle.AffirmativeAndNegative);

                        if (result == MessageDialogResult.Affirmative)
                        {
                            File.Delete(filePath);
                        }
                        else
                            return;
                    }

                    // Check the URL
                    download.CheckUrl();
                    if (download.HasError)
                        return;

                    download.TempDownloadPath = tempPath;

                    download.AddedOn = DateTime.UtcNow;
                    download.CompletedOn = DateTime.MinValue;
                    download.OpenFileOnCompletion = false;

                    // Add the download to the downloads list
                    DownloadManager.Instance.DownloadsList.Add(download);

                    // Start downloading the file
                    bool startImmediately = true;
                    cmd = Database.Connection().CreateCommand();
                    sql = "SELECT * FROM Downloads";
                    cmd.CommandText = sql;
                    readerConfig = cmd.ExecuteReader();
                    while (readerConfig.Read())
                    {
                        //memorycachesize, maxdownloads, enablespeedlimit, speedlimit, startdownloadsonstartup, startimmediately, downloadpath
                        if (readerConfig.GetInt32(6) > 0)
                            startImmediately = true;
                    }

                    // Start downloading the file
                    if (startImmediately)
                        download.Start();
                    else
                        download.Status = DownloadStatus.Paused;

                    downloadsGrid.ItemsSource = DownloadManager.Instance.DownloadsList;
                }
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("Error", ex.Message);
                return;
            }
        }

        public void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            WebDownloadClient download = (WebDownloadClient)sender;
            if (e.PropertyName == "AverageSpeedAndTotalTime" && download.Status != DownloadStatus.Deleting)
            {
                this.Dispatcher.Invoke(new PropertyChangedEventHandler(UpdatePropertiesList), sender, e);
            }
        }

        private void DownloadsList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (DownloadManager.Instance.TotalDownloads == 1)
                this.statusBarDownloads.Content = "1 Download";
            else if (DownloadManager.Instance.TotalDownloads > 1)
                this.statusBarDownloads.Content = DownloadManager.Instance.TotalDownloads + " Downloads";
            else
                this.statusBarDownloads.Content = "Ready";
        }

        private void UpdatePropertiesList(object sender, PropertyChangedEventArgs e)
        {
            propertyValues.RemoveRange(4, 2);
            var download = (WebDownloadClient)downloadsGrid.SelectedItem;
            propertyValues.Add(download.AverageDownloadSpeed);
            propertyValues.Add(download.TotalElapsedTimeString);

            propertiesList.RemoveRange(4, 2);
            propertiesList.Add(new PropertyModel(propertyNames[4], propertyValues[4]));
            propertiesList.Add(new PropertyModel(propertyNames[5], propertyValues[5]));
            //propertiesGrid.Items.Refresh();
        }

        public void StatusChangedHandler(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new EventHandler(StatusChanged), sender, e);
        }

        private void StatusChanged(object sender, EventArgs e)
        {
            // Start the first download in the queue, if it exists
            WebDownloadClient dl = (WebDownloadClient)sender;
            if (dl.Status == DownloadStatus.Paused || dl.Status == DownloadStatus.Completed
                || dl.Status == DownloadStatus.Deleted || dl.HasError)
            {
                foreach (WebDownloadClient d in DownloadManager.Instance.DownloadsList)
                {
                    if (d.Status == DownloadStatus.Queued)
                    {
                        d.Start();
                        break;
                    }
                }
            }

            foreach (WebDownloadClient d in DownloadManager.Instance.DownloadsList)
            {
                if (d.Status == DownloadStatus.Downloading)
                {
                    d.SpeedLimitChanged = true;
                }
            }

            int active = DownloadManager.Instance.ActiveDownloads;
            int completed = DownloadManager.Instance.CompletedDownloads;

            if (active > 0)
            {
                if (completed == 0)
                    this.statusBarActive.Content = " (" + active + " Active)";
                else
                    this.statusBarActive.Content = " (" + active + " Active, ";
            }
            else
                this.statusBarActive.Content = String.Empty;

            if (completed > 0)
            {
                if (active == 0)
                    this.statusBarCompleted.Content = " (" + completed + " Completed)";
                else
                    this.statusBarCompleted.Content = completed + " Completed)";
            }
            else
                this.statusBarCompleted.Content = String.Empty;
        }

        private async void uxTextBoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            string title = uxTextBoxSearch.Text;
            string plataform = uxComboPlataform.Text;
            if (string.IsNullOrEmpty(plataform))
                return;

            if (title == String.Empty)
                return;

            if (e.Key == Key.Enter)
            {
                DataSet ds = new DataSet();
                ds = Database.GetGamesByNameAndPlataform(title, plataform.ToUpper());

                if (ds.Tables[0].Rows.Count > 0)
                {
                    if (SearchWindow.Instance == null)
                    {
                        SearchWindow.Instance = new SearchWindow(ds);
                        SearchWindow.Instance.Show();
                        SearchWindow.Instance.Closed += (o, a) => { SearchWindow.Instance = null; };
                    }
                    else if (SearchWindow.Instance != null && !SearchWindow.Instance.IsActive)
                    {
                        SearchWindow.Instance.Activate();
                    }
                }
                else
                {
                    await this.ShowMessageAsync(
                        "Error",
                            "There is no results found.");
                    return;
                }
            }
        }


        public void DownloadCompletedHandler(object sender, EventArgs e)
        {
            WebDownloadClient download = (WebDownloadClient)sender;

            if (download.Status == DownloadStatus.Completed)
            {
                string title = "Download Completed! ";
                string text = title + download.FileName + " has finished downloading.";
                this.statusBarCompleted.Content = text;
            }
        }

        private async void uxComboPlataform_DropDownClosed(object sender, EventArgs e)
        {
            string selectedPlataform = uxComboPlataform.Text;
            selectedPlataform = selectedPlataform.ToUpper();
            if (string.IsNullOrEmpty(selectedPlataform))
                return;

            try
            {
                if (Database.ColumnPlataformHasValue("Games", selectedPlataform.ToUpper()))
                {
                    TransformControls(false);
                    await LoadGames(selectedPlataform);
                    TransformControls(true);
                }
                else
                {

                    switch (selectedPlataform)
                    {
                        case "3DS":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_3DS), "3DS");
                            break;

                        case "AMIGA":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_AMIGA), "AMIGA");
                            break;

                        case "ATARI 2600":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_ATARI_2600), "ATARI 2600");
                            break;

                        case "ATARI 5200":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_ATARI_5200), "ATARI 5200");
                            break;

                        case "ATARI 7800":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_ATARI_7800), "ATARI 7800");
                            break;

                        case "ATARI JAGUAR":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_ATARI_JAGUAR), "ATARI JAGUAR");
                            break;

                        case "DREAMCAST":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_DREAMCAST), "DREAMCAST");
                            break;

                        case "FAMICOM":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_FAMICOM), "FAMICOM");
                            break;

                        case "GAME CUBE":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GAMECUBE), "GAME CUBE");
                            break;

                        case "GAME GEAR":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GAMEGEAR), "GAME GEAR");
                            break;

                        case "GAME BOY":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GAMEGEAR), "GAME GEAR");
                            break;

                        case "GAME BOY ADVANCE":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GBA), "GAME BOY ADVANCE");
                            break;

                        case "GAME BOY COLOR":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GBC), "GAME BOY COLOR");
                            break;

                        case "M.A.M.E":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_MAME), "M.A.M.E");
                            break;

                        case "MASTER SYSTEM":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_MASTER_SYSTEM), "MASTER SYSTEM");
                            break;

                        case "MEGA DRIVE":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_MEGA_DRIVE), "MEGA DRIVE");
                            break;

                        case "N64":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_N64), "N64");
                            break;

                        case "NDS":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_NDS), "NDS");
                            break;

                        case "NES":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_NES), "NES");
                            break;

                        case "PS2":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_PS2), "PS2");
                            break;

                        case "PSP":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_PSP), "PSP");
                            break;

                        case "PSX":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_PSX), "PSX");
                            break;

                        case "SNES":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_SNES), "SNES");
                            break;

                        case "WII":
                            await ScrapGames(Path.Combine(Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_WII), "WII");
                            break;
                    }
                }
            }
            catch
            {

            }
        }
        #endregion

        #region Menu
        private void uxBtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (General.Instance == null)
            {
                General.Instance = new General();
                General.Instance.Show();
                General.Instance.Closed += (o, a) => { General.Instance = null; };
            }
            else if (General.Instance != null && !General.Instance.IsActive)
            {
                General.Instance.Activate();
            }
        }

        private void uxBtnDonate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/tryller/romsdownload#donate");
        }

        private void uxBtnGitHubProject_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/tryller/romsdownload");
        }
        #endregion

        #region Context Menu
        private async void uxContextMenuDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cellInfo = downloadsGrid.SelectedCells[0];//Cell Index
                var content = (cellInfo.Column.GetCellContent(cellInfo.Item) as TextBlock).Text;
                if (downloadsGrid.SelectedItems.Count > 0)
                {
                    var result = await
                        this.ShowMessageAsync(
                            "Warning", "Are you sure you want to delete " + content.ToString() + "?",
                            MessageDialogStyle.AffirmativeAndNegative);

                    if (result == MessageDialogResult.Affirmative)
                    {
                        var selectedDownloads = downloadsGrid.SelectedItems.Cast<WebDownloadClient>();
                        var downloadsToDelete = new List<WebDownloadClient>();

                        foreach (WebDownloadClient download in selectedDownloads)
                        {
                            if (download.HasError || download.Status == DownloadStatus.Paused || download.Status == DownloadStatus.Queued)
                            {
                                if (File.Exists(download.TempDownloadPath))
                                {
                                    File.Delete(download.TempDownloadPath);
                                }
                                download.Status = DownloadStatus.Deleting;
                                downloadsToDelete.Add(download);
                            }
                            else if (download.Status == DownloadStatus.Completed)
                            {
                                download.Status = DownloadStatus.Deleting;
                                downloadsToDelete.Add(download);
                            }
                            else
                            {
                                download.Status = DownloadStatus.Deleting;
                                while (true)
                                {
                                    if (download.DownloadThread.ThreadState == System.Threading.ThreadState.Stopped)
                                    {
                                        if (File.Exists(download.TempDownloadPath))
                                        {
                                            File.Delete(download.TempDownloadPath);
                                        }
                                        downloadsToDelete.Add(download);
                                        break;
                                    }
                                }
                            }
                        }

                        foreach (var download in downloadsToDelete)
                        {
                            download.Status = DownloadStatus.Deleted;
                            DownloadManager.Instance.DownloadsList.Remove(download);
                        }
                    }
                }
            }
            catch { }
        }

        private void uxContextMenuClearCompleteds_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DownloadManager.Instance.TotalDownloads > 0)
                {
                    var downloadsToClear = new List<WebDownloadClient>();

                    foreach (var download in DownloadManager.Instance.DownloadsList)
                    {
                        if (download.Status == DownloadStatus.Completed)
                        {
                            download.Status = DownloadStatus.Deleting;
                            downloadsToClear.Add(download);
                        }
                    }

                    foreach (var download in downloadsToClear)
                    {
                        download.Status = DownloadStatus.Deleted;
                        DownloadManager.Instance.DownloadsList.Remove(download);
                    }
                }
            }
            catch { }
        }

        private void uxContextMenuPlayPause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (downloadsGrid.SelectedItems.Count > 0)
                {
                    var selectedDownloads = downloadsGrid.SelectedItems.Cast<WebDownloadClient>();

                    foreach (WebDownloadClient download in selectedDownloads)
                    {
                        if (download.Status == DownloadStatus.Paused || download.HasError)
                        {
                            download.Start();
                            uxContextMenuPlayPause.Header = "Pause";
                        }
                        else
                        {
                            download.Pause();
                            uxContextMenuPlayPause.Header = "Play";
                        }
                    }
                }
            }
            catch { }
        }
        #endregion

        #region Select Tabs
        private void uxBtnGameList_Click(object sender, RoutedEventArgs e)
        {
            uxMainTabControl.SelectedIndex = 0;
        }

        private void uxBtnDownloadList_Click(object sender, RoutedEventArgs e)
        {
            uxMainTabControl.SelectedIndex = 1;
        }
        #endregion
    }
}