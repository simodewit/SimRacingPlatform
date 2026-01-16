using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Pages;
using SimRacingPlatform.Utilities;
using System;

namespace SimRacingPlatform.Windows
{
    public sealed partial class MainWindow : Window
    {
        public static MainWindow Instance;

        private static readonly Type[] AuthPages =
{
            typeof(LoginPage),
            typeof(RegisterPage),
            typeof(VerifyEmailPage),
            typeof(EmailConfirmedPage),
            typeof(ForgotPasswordPage),
            typeof(PasswordResetSentPage),
            typeof(PasswordChangedPage)
        };

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            WindowUtility.SetTitle(this, "SimRacingPlatform");
            WindowUtility.SetIcon(this, "Assets/SquareLogo.ico");
            WindowUtility.SetTitleBarColors(this);

            ContentFrame.Navigated += ContentFrame_Navigated;

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

        #region Navigation

        public void NavigateTo(Type pageType)
        {
            if (DispatcherQueue.HasThreadAccess)
            {
                ContentFrame.Navigate(pageType);
            }
            else
            {
                DispatcherQueue.TryEnqueue(() => ContentFrame.Navigate(pageType));
            }
        }

        public void NavigateBack()
        {
            if (!ContentFrame.CanGoBack)
                return;

            for (int i = ContentFrame.BackStack.Count - 1; i >= 0; i--)
            {
                var entry = ContentFrame.BackStack[i];

                if (entry.SourcePageType == typeof(AccountPage) ||
                    entry.SourcePageType == typeof(SettingsPage))
                {
                    ContentFrame.BackStack.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }

            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        #endregion

        #region Sidebar

        private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs args)
        {
            bool isAuth = IsAuthPage(args.SourcePageType);
            SetSidebarVisibility(!isAuth);
        }

        private static bool IsAuthPage(Type pageType)
        {
            foreach (var type in AuthPages)
            {
                if (type == pageType)
                {
                    return true;
                }
            }

            return false;
        }

        private void SetSidebarVisibility(bool visible)
        {
            if (visible)
            {
                SidebarControl.Visibility = Visibility.Visible;
                SidebarColumn.Width = new GridLength(260);
                ContentFrame.SetValue(Grid.ColumnProperty, 1);
            }
            else
            {
                SidebarControl.Visibility = Visibility.Collapsed;
                SidebarColumn.Width = new GridLength(0);
                ContentFrame.SetValue(Grid.ColumnProperty, 1);
            }
        }

        #endregion
    }
}
