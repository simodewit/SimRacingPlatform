using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using SimRacingPlatform.Utilities;
using SimRacingPlatform.Windows;
using System;

namespace SimRacingPlatform.Pages
{
    public sealed partial class AccountPage : Page
    {
        public AccountViewModel ViewModel { get; } = new();

        public AccountPage()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.instance.NavigateBack();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            FirebaseUtility.instance.Logout();
            MainWindow.instance.NavigateTo(typeof(LoginPage));
        }

        private void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditProfile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ViewSessions_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ManageNotifications_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class AccountViewModel : DependencyObject
    {
        public BitmapImage ProfileImage { get; } =
            new BitmapImage(new Uri("ms-appx:///Assets/DefaultProfile.png"));

        public string DisplayName { get; set; } = "Jane Doe";
        public string Email { get; set; } = "jane.doe@example.com";
        public string Username { get; set; } = "@janedoe";
        public string AccountStatus { get; set; } = "Active";

        public bool IsTwoFactorEnabled
        {
            get => (bool)GetValue(IsTwoFactorEnabledProperty);
            set => SetValue(IsTwoFactorEnabledProperty, value);
        }
        public static readonly DependencyProperty IsTwoFactorEnabledProperty =
            DependencyProperty.Register(nameof(IsTwoFactorEnabled), typeof(bool), typeof(AccountViewModel), new PropertyMetadata(false));

        public bool SignInAlertsEnabled
        {
            get => (bool)GetValue(SignInAlertsEnabledProperty);
            set => SetValue(SignInAlertsEnabledProperty, value);
        }
        public static readonly DependencyProperty SignInAlertsEnabledProperty =
            DependencyProperty.Register(nameof(SignInAlertsEnabled), typeof(bool), typeof(AccountViewModel), new PropertyMetadata(true));

        public bool NewsletterEnabled
        {
            get => (bool)GetValue(NewsletterEnabledProperty);
            set => SetValue(NewsletterEnabledProperty, value);
        }
        public static readonly DependencyProperty NewsletterEnabledProperty =
            DependencyProperty.Register(nameof(NewsletterEnabled), typeof(bool), typeof(AccountViewModel), new PropertyMetadata(false));

        public string SelectedTheme
        {
            get => (string)GetValue(SelectedThemeProperty);
            set => SetValue(SelectedThemeProperty, value);
        }
        public static readonly DependencyProperty SelectedThemeProperty =
            DependencyProperty.Register(nameof(SelectedTheme), typeof(string), typeof(AccountViewModel), new PropertyMetadata("System"));
    }
}
