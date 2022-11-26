using ControlzEx.Theming;
using MahApps.Metro.Controls;
using romsdownload.Data;
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
            {
                uxComboStyle.Items.Add(theme);
            }

            if (Config.Instance.SelectedStyle != null)
            {
                uxComboStyle.SelectedItem = Config.Instance.SelectedStyle;
            }

            if (uxComboStyle.SelectedIndex == -1)
            {
                uxComboStyle.SelectedIndex = uxComboStyle.Items.IndexOf("Light");
            }
        }


        private void Color_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var color in _accentColors)
            {
                uxComboColor.Items.Add(color);
            }

            if (Config.Instance.SelectedColor != null)
            {
                uxComboColor.SelectedItem = Config.Instance.SelectedColor;
            }

            if (uxComboColor.SelectedIndex == -1)
            {
                uxComboColor.SelectedIndex = uxComboColor.Items.IndexOf("Blue");
            }
        }

        private void Color_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var color = uxComboColor.SelectedValue.ToString();

            if (_accentColors.Contains(color))
            {
                ThemeManager.Current.ChangeThemeColorScheme(Application.Current, color);
                Config.Instance.SelectedColor = color;
            }
        }

        private void Style_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var style = uxComboStyle.SelectedValue.ToString();

            if (_style.Contains(style))
            {
                ThemeManager.Current.ChangeThemeBaseColor(Application.Current, style);
                Config.Instance.SelectedStyle = style;
            }
        }
    }
}
