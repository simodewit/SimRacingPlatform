using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SimRacingPlatform.Windows;
using Windows.UI;

namespace SimRacingPlatform.Pages
{
    public sealed partial class SideBar : UserControl
    {
        public SolidColorBrush ScrollAreaBackgroundBrush { get; private set; } = new SolidColorBrush();

        public SideBar()
        {
            InitializeComponent();
            UpdateScrollAreaBrush();
            ActualThemeChanged += (_, __) => UpdateScrollAreaBrush();
        }

        private void UpdateScrollAreaBrush()
        {
            if (Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] is SolidColorBrush baseBrush)
            {
                ScrollAreaBackgroundBrush.Color = Lighten(baseBrush.Color, 0.12);
            }
            else
            {
                ScrollAreaBackgroundBrush.Color = Lighten(Colors.Black, 0.12);
            }
        }

        private static Color Lighten(Color color, double amount)
        {
            byte Lerp(byte a, byte b) => (byte)(a + (b - a) * amount);

            return Color.FromArgb(
                color.A,
                Lerp(color.R, 255),
                Lerp(color.G, 255),
                Lerp(color.B, 255)
            );
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateTo(typeof(LandingPage));
        }

        private void FirstTool_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateTo(typeof(Tool1));
        }

        private void SecondTool_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateTo(typeof(Tool2));
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateTo(typeof(AccountPage));
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.NavigateTo(typeof(SettingsPage));
        }
    }
}
