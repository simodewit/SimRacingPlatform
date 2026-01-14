using Microsoft.UI.Xaml;
using SimRacingPlatform.Pages;
using System;

namespace SimRacingPlatform.Windows
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow instance;

        public MainWindow()
        {
            instance = this;
            InitializeComponent();

            var user = App.AuthService.Client.User;

            if (user != null)
            {
                NavigateTo(typeof(LandingPage));
            }
            else
            {
                NavigateTo(typeof(LoginPage));
            }
        }

        public void NavigateTo(Type pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}
