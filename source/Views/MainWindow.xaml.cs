using System;
using System.Diagnostics;
using System.Windows;
using romsdownload.Models;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using romsdownload.Classes;
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

namespace romsdownloader.Views
{
    public partial class MainWindow
    {
        #region Declarations 
        public static MainWindow Instance;
        public List<GameList> ContentList { get; private set; }
        public GameList Games { get; private set; }
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

                var folder = "Games";
                var dir = Path.Combine(Directory.GetCurrentDirectory(), folder);
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

            Loaded += WindowLoaded;
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

        private async void LoadDownloadsFromXml()
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

                            if (downloadClient.Status == DownloadStatus.Paused && !downloadClient.HasError && Settings.Default.StartDownloadsOnStartup)
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
                await this.ShowMessageAsync(
                    "Error",
                        "There was an error while loading the download list.");
            }
        }
        #endregion

        #region Window Events
        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            SaveDownloadsToXml();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ContentList = new List<GameList>();
            _ = Task.Delay(TimeSpan.FromMilliseconds(1));

            uxTabGameList.Visibility = Visibility.Hidden;
            uxTabCurrentDownlaods.Visibility = Visibility.Hidden;

            var folder = Directories.CachePath;
            var file = Path.Combine(folder, "GameList.json");
            if (!File.Exists(file))
                _ = DownloadFile();
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
                try
                {
                    Utility.MapClassToXmlFile(typeof(Config), Config.Instance, Directories.ConfigFilePath);
                }
                catch
                {
                    await this.ShowMessageAsync(
                        "Error",
                            "Could not write to config.xml.");
                }
                Application.Current.Shutdown();
            }

        }
        #endregion

        #region Load Games
        async Task DownloadFile()
        {
            try
            {
                TransformControls(false);
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                statusBarDownloads.Content = "Downloading cache file!";
                var GameUrlPage = string.Empty;

                //3DS
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_3DS;
                GamePlataform = "3DS";
            _3DS:
                var Webget = new HtmlWeb();
                var doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
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
                        GameUrlPage = nextPage;
                        goto _3DS;
                    }
                }

                //AMIGA
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_AMIGA;
                GamePlataform = "AMIGA";
            _AMIGA:
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _AMIGA;
                    }
                }

                //ATARI 2600
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_ATARI_2600;
                GamePlataform = "ATARI 2600";
            _ATARI_2600:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1));
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _ATARI_2600;
                    }
                }

                //ATARI 5200
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_ATARI_5200;
                GamePlataform = "ATARI 5200";
            _ATARI_5200:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _ATARI_5200;
                    }
                }

                //ATARI 7800
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_ATARI_7800;
                GamePlataform = "ATARI 7800";
            _ATARI_7800:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _ATARI_7800;
                    }
                }

                //ATARI JAGUAR
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_ATARI_JAGUAR;
                GamePlataform = "ATARI JAGUAR";
            _ATARI_JAGUAR:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _ATARI_JAGUAR;
                    }
                }

                //DREAMCAST
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_DREAMCAST;
                GamePlataform = "DREAMCAST";
            _DREAMCAST:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _DREAMCAST;
                    }
                }

                //FAMICOM
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_FAMICOM;
                GamePlataform = "FAMICOM";
            _FAMICOM:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {
                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _FAMICOM;
                    }
                }

                //GAME CUBE
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GAMECUBE;
                GamePlataform = "GAME CUBE";
            _GAMECUBE:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1));
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _GAMECUBE;
                    }
                }

                //GAME GEAR
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GAMEGEAR;
                GamePlataform = "GAME GEAR";
            _GAMEGEAR:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _GAMEGEAR;
                    }
                }

                //GAME BOY
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GB;
                GamePlataform = "GAME BOY";
            _GB:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _GB;
                    }
                }

                //GAME BOY ADVANCE
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GBA;
                GamePlataform = "GAME BOY ADVANCE";
            _GBA:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _GBA;
                    }
                }

                //GAME BOY COLOR
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GBC;
                GamePlataform = "GAME BOY COLOR";
            _GBC:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _GBC;
                    }
                }

                //M.A.M.E
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_MAME;
                GamePlataform = "M.A.M.E";
            _MAME:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _MAME;
                    }
                }

                //MASTER SYSTEM
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_MASTER_SYSTEM;
                GamePlataform = "MASTER SYSTEM";
            _MASTER_SYSTEM:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _MASTER_SYSTEM;
                    }
                }

                //MEGA DRIVE
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_MEGA_DRIVE;
                GamePlataform = "MEGA DRIVE";
            _MEGADRIVE:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _MEGADRIVE;
                    }
                }

                //N64
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_N64;
                GamePlataform = "N64";
            _N64:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _N64;
                    }
                }

                //NDS
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_NDS;
                GamePlataform = "NDS";
            _NDS:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _NDS;
                    }
                }

                //NES
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_NES;
                GamePlataform = "NES";
            _NES:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _NES;
                    }
                }

                //PS2
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_PS2;
                GamePlataform = "PS2";
            PS2:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto PS2;
                    }
                }

                //PSP
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_PSP;
                GamePlataform = "PSP";
            _PSP:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _PSP;
                    }
                }

                //PSX
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_PSX;
                GamePlataform = "PSX";
            _PSX:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _PSX;
                    }
                }

                //NDS
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_SNES;
                GamePlataform = "SNES";
            _SNES:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _SNES;
                    }
                }

                //WII
                GameUrlPage = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_WII;
                GamePlataform = "WII";
            _WII:
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                Webget = new HtmlWeb();
                doc = Webget.Load(GameUrlPage);
                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    GameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    GameName = GameName.Replace(" ROM", "");
                    GameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    CoverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (CoverImage.StartsWith("data:image"))
                        CoverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    Games = new GameList();
                    Games.Title = GameName;
                    Games.Image = CoverImage;
                    Games.Url = GameUrl;
                    Games.Plataform = GamePlataform;
                    ContentList.Add(Games);

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    statusBarDownloads.Content = "Downloading " + GamePlataform;
                }

                //Next Page
                nextPage = string.Empty;
                checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        GameUrlPage = nextPage;
                        goto _WII;
                    }
                }


                await Task.Delay(TimeSpan.FromMilliseconds(1));
                var folder = Directories.CachePath;
                var file = Path.Combine(folder, "GameList.json");
                JsonFormat.Export(file, ContentList);

                await Task.Delay(TimeSpan.FromMilliseconds(1));
                TransformControls(true);
                statusBarDownloads.Content = "Downlaod Complete!";
            }
            catch
            {
                ContentList.Clear();
            }
        }

        private async Task LoadGames(string plataform)
        {
            TransformControls(false);
            await Task.Delay(TimeSpan.FromMilliseconds(1));
            var folder = Directories.CachePath;
            var file = Path.Combine(folder, "GameList.json");
            if (!File.Exists(file))
            {
                return;
            }

            var jsonformat = JsonFormat.Import(file);

            if (jsonformat == null)
            {
                await this.ShowMessageAsync(
                    "Error",
                        "Unable to load json file.");
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(1));

            uxGamesListView.ItemsSource = jsonformat.GameList.Where(c => c.Plataform.ToUpper().Equals(plataform.ToUpper()));
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

            if (status)
                uxGamesListView.Visibility = Visibility.Visible;
            else
                uxGamesListView.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Control Events
        private async void uxGamesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1));
            ListView list = (ListView)sender;
            GameList item = (GameList)list.SelectedItem;
            if (item != null)
            {
                string url = item.Url;
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
                download.FileName = item.Title.Trim();

                // Register WebDownloadClient events
                download.DownloadProgressChanged += download.DownloadProgressChangedHandler;
                download.DownloadCompleted += download.DownloadCompletedHandler;
                download.PropertyChanged += this.PropertyChangedHandler;
                download.StatusChanged += this.StatusChangedHandler;
                download.DownloadCompleted += this.DownloadCompletedHandler;

                // Create path to temporary file
                var folder = Directories.DownloadsPath;
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
                download.Start();

                downloadsGrid.ItemsSource = DownloadManager.Instance.DownloadsList;
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

        private async void uxTextBoxSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string keyword = uxTextBoxSearch.Text;
            string plataform = uxComboPlataform.Text;
            if (string.IsNullOrEmpty(plataform))
                return;

            var folder = "Cache";
            var file = Path.Combine(folder, "GameList.json");
            if (!File.Exists(file))
            {
                return;
            }

            if (keyword.Length >= 1)
            {
                var jsonformat = JsonFormat.Import(file);

                if (jsonformat == null)
                {
                    await this.ShowMessageAsync(
                        "Error",
                            "Unable to load json file.");
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1));
                uxGamesListView.ItemsSource = jsonformat.GameList.Where(c => c.Title.ToUpper().Contains(keyword.ToUpper())).Where(c => c.Plataform.ToUpper().Equals(plataform.ToUpper()));
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
            string selectionText = uxComboPlataform.Text.Trim().ToUpper();
            if (string.IsNullOrEmpty(selectionText))
                return;

            await LoadGames(selectionText);
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
            Process.Start("https://www.paypal.com/donate/?business=XKPHGJJSTE2RQ&no_recurring=0&currency_code=BRL");
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