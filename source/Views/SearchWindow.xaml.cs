using System.Data;
using System.Windows.Input;

namespace romsdownload.Views
{
    /// <summary>
    /// Lógica interna para SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow
    {
        private DataSet ds;
        public static SearchWindow Instance;
        public SearchWindow(DataSet data)
        {
            InitializeComponent();
            Instance = this;
            ds = data;
            uxListViewSearchResults.ItemsSource = ds.Tables[0].DefaultView;
        }

        private void MetroWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }
    }
}
