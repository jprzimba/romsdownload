using ControlzEx.Theming;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using romsdownload.Data;
using romsdownloader.Classes;
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

        public General()
        {
            InitializeComponent();
            Instance = this;
        }

        private void Style_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var theme in _style)
                uxComboStyle.Items.Add(theme);

            IniFile config = new IniFile(Directories.ConfigFilePath);
            if (config.KeyExists("SelectedStyle", "Theme"))
               uxComboStyle.SelectedItem = config.Read("SelectedStyle", "Theme");

            if (uxComboStyle.SelectedIndex == -1)
                uxComboStyle.SelectedIndex = uxComboStyle.Items.IndexOf("Light");
        }


        private void Color_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var color in _accentColors)
                uxComboColor.Items.Add(color);

            IniFile config = new IniFile(Directories.ConfigFilePath);
            if (config.KeyExists("SelectedColor", "Theme"))
                uxComboColor.SelectedItem = config.Read("SelectedColor", "Theme");

            if (uxComboColor.SelectedIndex == -1)
                uxComboColor.SelectedIndex = uxComboColor.Items.IndexOf("Blue");
        }

        private void Color_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var color = uxComboColor.SelectedValue.ToString();
            if (_accentColors.Contains(color))
            {
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, color);
                IniFile config = new IniFile(Directories.ConfigFilePath);
                config.Write("SelectedColor", color,  "Theme");
            }
        }

        private void Style_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var style = uxComboStyle.SelectedValue.ToString();
            if (_style.Contains(style))
            {
                ThemeManager.Current.ChangeThemeBaseColor(Application.Current, style);
                IniFile config = new IniFile(Directories.ConfigFilePath);
                config.Write("SelectedStyle", style, "Theme");
            }
        }
    }
}
