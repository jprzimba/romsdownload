using System;
using System.Diagnostics;
using System.Windows;
using romsdownload.Models;
using System.Collections.Generic;
using System.Windows.Input;
using HtmlAgilityPack;
using romsdownload.Properties;
using System.Threading.Tasks;
using System.Linq;

namespace romsdownloader.Views
{
    public partial class MainWindow
    {
        private string downloadDirectory = Application.Current.StartupUri + "\\Games";

        private List<GameList> ContentList;
        private GameList Games;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += WindowLoaded;
            Closed += WindowClosed;
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

        }

        private void WindowClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void LoadGames(string page)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            string gameName = string.Empty;
            string gameUrl = string.Empty;
            string url = page;
            string coverImage = string.Empty;

            TransformControls(false);
            ContentList = new List<GameList>();
            uxLabelStatus.Text = "Loading...";
        GRAB:
            try
            {
                var Webget = new HtmlWeb();
                var doc = Webget.Load(url);

                //Search game
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//div[@class=\"roms-img\"]"))
                {
                    gameName = node.SelectSingleNode("a").Attributes["title"].Value;
                    gameName = gameName.Replace(" ROM", "");
                    gameUrl = Settings.Default.ROMSPEDIA_BASE_URL + node.SelectSingleNode("a").Attributes["href"].Value;

                    coverImage = node.SelectSingleNode("a//picture//source").Attributes["srcset"].Value;
                    if (coverImage.StartsWith("data:image"))
                        coverImage = node.SelectSingleNode("a//picture//source").Attributes["data-srcset"].Value;

                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                    Games = new GameList();
                    Games.Title = gameName;
                    Games.Image = coverImage;
                    Games.Url = gameUrl;
                    ContentList.Add(Games);
                    uxLabelStatus.Text = "Loading games...";
                }

                //Next Page
                var nextPage = string.Empty;
                var checkNextPage = string.Empty;
                foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//li[@class=\"page-item\"]//a[@class=\"page-link\"]"))
                {

                    checkNextPage = node.InnerText;
                    if (checkNextPage.Equals(">"))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(1));
                        nextPage = node.GetAttributeValue("href", "");
                        nextPage = Settings.Default.ROMSPEDIA_BASE_URL + nextPage;
                        url = nextPage;
                        goto GRAB;
                    }
                }
            }
            catch { }

            await Task.Delay(TimeSpan.FromMilliseconds(1));
            uxLabelStatus.Text = "Loading... Done!";
            uxGamesListView.ItemsSource = ContentList;
            TransformControls(true);
        }

        private void TransformControls(bool status)
        {
            uxComboPlataform.IsEnabled = status;
            uxGamesListView.IsEnabled = status;
            uxTextBoxSearch.IsEnabled = status;
            uxMainTabPanel.IsEnabled = status;
        }

        private void uxGamesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ListView list = (System.Windows.Controls.ListView)sender;
            GameList item = (GameList)list.SelectedItem;
            if (item != null)
            {
                MessageBox.Show(item.Title);
            }
        }

        private async void uxTextBoxSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string keyword = uxTextBoxSearch.Text;
            if (keyword.Length >= 1)
            {
                var s = ContentList.Where(c => c.Title.ToLower().Contains(keyword.ToLower()));
                uxGamesListView.ItemsSource = s;
            }
            else
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1));
                uxGamesListView.ItemsSource = ContentList;
            }
        }

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

        private void uxComboPlataform_DropDownClosed(object sender, EventArgs e)
        {
            string selectionText = uxComboPlataform.Text.Trim().ToUpper();
            string page = string.Empty;

            if (selectionText.Equals("3DS"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_3DS;
                LoadGames(page);
            }
            else if (selectionText.Equals("AMIGA"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_AMIGA;
                LoadGames(page);
            }
            else if (selectionText.Equals("ATARI 2600"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_ATARI_2600;
                LoadGames(page);
            }
            else if (selectionText.Equals("ATARI 5200"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_ATARI_5200;
                LoadGames(page);
            }
            else if (selectionText.Equals("ATARI 7800"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_ATARI_7800;
                LoadGames(page);
            }
            else if (selectionText.Equals("ATARI JAGUAR"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_ATARI_JAGUAR;
                LoadGames(page);
            }
            else if (selectionText.Equals("DREAMCAST"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_DREAMCAST;
                LoadGames(page);
            }
            else if (selectionText.Equals("FAMICOM"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_FAMICOM;
                LoadGames(page);
            }
            else if (selectionText.Equals("GAME CUBE"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GAMECUBE;
                LoadGames(page);
            }
            else if (selectionText.Equals("GAME GEAR"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GAMEGEAR;
                LoadGames(page);
            }
            else if (selectionText.Equals("GB"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GB;
                LoadGames(page);
            }
            else if (selectionText.Equals("GBA"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GBA;
                LoadGames(page);
            }
            else if (selectionText.Equals("GBC"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_GBC;
                LoadGames(page);
            }
            else if (selectionText.Equals("M.A.M.E"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_MAME;
                LoadGames(page);
            }
            else if (selectionText.Equals("MASTER SYSTEM"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_MASTER_SYSTEM;
                LoadGames(page);
            }
            else if (selectionText.Equals("MEGA DRIVE"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_MEGA_DRIVE;
                LoadGames(page);
            }
            else if (selectionText.Equals("NES"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_NES;
                LoadGames(page);
            }
            else if (selectionText.Equals("SNES"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_SNES;
                LoadGames(page);
            }
            else if (selectionText.Equals("N64"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_N64;
                LoadGames(page);
            }
            else if (selectionText.Equals("NDS"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_NDS;
                LoadGames(page);
            }
            else if (selectionText.Equals("PSX"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_PSX;
                LoadGames(page);
            }
            else if (selectionText.Equals("PS2"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_PS2;
                LoadGames(page);
            }
            else if (selectionText.Equals("PSP"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_PSP;
                LoadGames(page);
            }
            else if (selectionText.Equals("WII"))
            {
                page = Settings.Default.ROMSPEDIA_BASE_URL + Settings.Default.ROMSPEDIA_PATH_WII;
                LoadGames(page);
            }
        }
    } 
}