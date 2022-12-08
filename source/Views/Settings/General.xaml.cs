using ControlzEx.Theming;
using MahApps.Metro.Controls;
using romsdownload.Data;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace romsdownload.Views.Settings
{
    /// <summary>
    /// Lógica interna para General.xaml
    /// </summary>
    public partial class General : MetroWindow
    {
        public static General Instance;

        private readonly string[] _accentColors =
        {
            "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal",
            "Cyan", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber", "Yellow", "Brown", "Olive",
            "Steel", "Mauve", "Taupe", "Sienna"
        };

        private readonly string[] _style =
{
            "Light", "Dark"
        };

        private string selectedStyle;
        private string selectedColor;

        public General()
        {
            InitializeComponent();
            Instance = this;
        }

        private void Color_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedColor = uxComboColor.SelectedValue.ToString();
            if (_accentColors.Contains(selectedColor))
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, selectedColor);
        }

        private void Style_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedStyle = uxComboStyle.SelectedValue.ToString();
            if (_style.Contains(selectedStyle))
                ThemeManager.Current.ChangeThemeBaseColor(Application.Current, selectedStyle);
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            selectedStyle = uxComboStyle.SelectedValue.ToString();
            selectedColor = uxComboColor.SelectedValue.ToString();

            Database.UpdateTheme(selectedStyle, selectedColor);
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Update Theme
            foreach (var color in _accentColors)
                uxComboColor.Items.Add(color);

            foreach (var theme in _style)
                uxComboStyle.Items.Add(theme);

            var cmd = Database.Connection().CreateCommand();

            string sql = "SELECT * FROM Theme";
            cmd.CommandText = sql;
            SQLiteDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                //Theme
                string style = reader.GetString(0);//style
                string color = reader.GetString(1);//theme

                uxComboStyle.SelectedItem = style;
                uxComboColor.SelectedItem = color;
            }

            if (uxComboStyle.SelectedIndex == -1)
                uxComboStyle.SelectedIndex = uxComboStyle.Items.IndexOf("Light");

            if (uxComboColor.SelectedIndex == -1)
                uxComboColor.SelectedIndex = uxComboColor.Items.IndexOf("Blue");
        }
    }
}
