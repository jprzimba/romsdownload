using System;
using System.Diagnostics;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using romsdownload.Models;
using System.Collections.Generic;
using System.Windows.Input;
using HtmlAgilityPack;
using MahApps.Metro.Controls;
using romsdownload.Properties;
using System.Windows.Data;
using System.Threading.Tasks;

namespace romsdownloader.Views
{
    public partial class MainWindow
    {
        public static MainWindow Instance;
        private string downloadDirectory = Application.Current.StartupUri + "\\Games";

        private List<GameList> ContentList;
        private GameList Games;

        public MainWindow()
        {
            InitializeComponent();

            Instance = this;

            Loaded += WindowLoaded;
            Closed += WindowClosed;
        }

        #region Window Events
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ContentList = new List<GameList>();
            if (GamesListView.ItemsSource != null)
            {
                CollectionView allView = (CollectionView)CollectionViewSource.GetDefaultView(GamesListView.ItemsSource);
                allView.Filter = UserFilter;
            }

            Instance.LoadGames("https://www.romspedia.com/roms/playstation-2", Instance);
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region Right Window Commands

        private void BtnDonateClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.paypal.com/");
        }

        private void BtnGithubClick(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/tryller/romsdownload/issues");
        }

        #endregion

        private async void LoadGames(string page, MetroWindow window)
        {
            string gameName = string.Empty;
            string gameUrl = string.Empty;
            string url = page;
            string coverImage = string.Empty;

            var controller = await window.ShowProgressAsync("Loading...", "Please wait...");
            await Task.Delay(TimeSpan.FromSeconds(2));
            GamesListView.Visibility = Visibility.Hidden;
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

                    Games = new GameList();
                    Games.Title = gameName;
                    Games.Image = coverImage;
                    Games.Url = gameUrl;
                    ContentList.Add(Games);
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
                        goto GRAB;
                    }
                }
            }
            catch { }


            for (int i = 0; i < 101; i++)
            {
                controller.SetProgress(i / 100.0);
                controller.SetMessage(string.Format("Loading: {0}%", i));

                if (controller.IsCanceled)
                    break;

                await Task.Delay(100);

            }

            GamesListView.ItemsSource = ContentList;
            GamesListView.Visibility = Visibility.Visible;

            await controller.CloseAsync().ConfigureAwait(false);
        }

        private void GamesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.ListView list = (System.Windows.Controls.ListView)sender;
            GameList item = (GameList)list.SelectedItem;
            if (item != null)
            {
                MessageBox.Show(item.Title);
            }
        }

        private void txtFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(GamesListView.ItemsSource).Refresh();
            CollectionViewSource.GetDefaultView(GamesListView.ItemsSource).Refresh();

        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
                return true;

            return ((item as GameList).Title.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}