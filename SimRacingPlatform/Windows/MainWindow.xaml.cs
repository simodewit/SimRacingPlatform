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
            NavigateTo(typeof(LoginPage));
        }

        public void NavigateTo(Type pageType)
        {
            RootFrame.Navigate(pageType);
        }
    }
}
