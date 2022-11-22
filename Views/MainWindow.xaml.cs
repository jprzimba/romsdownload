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

namespace romsdownloader.Views
{
    public partial class MainWindow
    {
        #region Declarations 
        public List<GameList> ContentList { get; private set; }
        public GameList Games { get; private set; }
        public string GamePlataform { get; private set; }
        public string GameName { get; private set; }
        public string GameUrl { get; private set; }
        public string CoverImage { get; private set; }
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            Loaded += WindowLoaded;
            Closed += WindowClosed;
        }
        #endregion

        #region Window Events
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            _ = Task.Delay(TimeSpan.FromMilliseconds(1));
            var folder = "Cache";
            var file = Path.Combine(folder, "GameList.json");
            if (!File.Exists(file))
            _ = DownloadFile();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region Load Games
        async Task DownloadFile()
        {
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                ContentList = new List<GameList>();
                uxLabelStatus.Text = "Loading Games... Please Wait!";

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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                    uxLabelStatus.Text = "Loading " + GameName;
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
                var folder = "Cache";
                Utils.CreateDirectory(folder);
                var file = Path.Combine(folder, "GameList.json");
                JsonFormat.Export(file, ContentList);

                await Task.Delay(TimeSpan.FromMilliseconds(1));
                uxLabelStatus.Text = "Loading Games... DONE!";
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
            var folder = "Cache";
            var file = Path.Combine(folder, "GameList.json");
            if (!File.Exists(file))
            {
                return;
            }

            var jsonformat = JsonFormat.Import(file);

            if (jsonformat == null)
            {
                MessageBox.Show(this, "Unable to load JSON file.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(1));
            uxGamesListView.ItemsSource = jsonformat.GameList.Where(c => c.Plataform.ToUpper().Equals(plataform.ToUpper()));
            uxLabelStatus.Text = "Loaded " + plataform + " games. Total Games: " + uxGamesListView.Items.Count.ToString();
            TransformControls(true);
        }
        #endregion

        #region Functions
        private void TransformControls(bool status)
        {
            uxComboPlataform.IsEnabled = status;
            uxGamesListView.IsEnabled = status;
            uxTextBoxSearch.IsEnabled = status;
            uxMainTabPanel.IsEnabled = status;

            if (status)
                uxGamesListView.Visibility = Visibility.Visible;
            else
                uxGamesListView.Visibility = Visibility.Hidden;
        }
        #endregion

        #region Control Events
        private void uxGamesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ListView list = (System.Windows.Controls.ListView)sender;
            GameList item = (GameList)list.SelectedItem;
            if (item != null)
            {
                MessageBox.Show(item.Title);
            }
        }

        private void uxTextBoxSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string keyword = uxTextBoxSearch.Text;
            if (keyword.Length >= 1)
            {
                var s = ContentList.Where(c => c.Title.ToLower().Contains(keyword.ToLower()));
                uxGamesListView.ItemsSource = s;
            }
            else
                uxGamesListView.ItemsSource = ContentList;
        }

        private async void uxComboPlataform_DropDownClosed(object sender, EventArgs e)
        {
            string selectionText = uxComboPlataform.Text.Trim().ToUpper();
            await LoadGames(selectionText);
        }
        #endregion

        #region Menu
        private void uxBtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void uxBtnDonate_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://paypal.com");
        }

        private void uxBtnGitHubProject_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/tryller/romsdownload");
        }

        private void uxBtnGitHubIssues_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/tryller/romsdownload/issues");
        }
        #endregion
    } 
}