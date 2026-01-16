using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Windows;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics;
using Windows.UI;
using WinRT.Interop;

namespace SimRacingPlatform.Utilities
{
    public static class WindowUtility
    {
        public static Color standardBackground = Color.FromArgb(255,43,43,43);
        public static Color standardForeground = Color.FromArgb(255,255,255,255);

        private static AppWindow GetAppWindow(Window window)
        {
            var handle = WindowNative.GetWindowHandle(window);
            var windowId = Win32Interop.GetWindowIdFromWindow(handle);
            return AppWindow.GetFromWindowId(windowId);
        }

        public static void SetTitle(Window window, string title)
        {
            window.Title = title;
        }

        public static void SetIcon(Window window, string localIconPath)
        {
            var path = Path.Combine(Package.Current.InstalledLocation.Path, localIconPath);
            var appWindow = GetAppWindow(window);
            appWindow.SetIcon(path);
        }

        public static void SetTitleBarColors(Window window, Color? background = null, Color? foreground = null)
        {
            background = standardBackground;
            foreground = standardForeground;

            var titleBar = GetAppWindow(window).TitleBar;

            titleBar.BackgroundColor = background;
            titleBar.ForegroundColor = foreground;

            titleBar.ButtonBackgroundColor = background;
            titleBar.ButtonForegroundColor = foreground;
        }

        public static void HideTitleBar(Window window)
        {
            window.ExtendsContentIntoTitleBar = true;

            if (GetAppWindow(window).Presenter is OverlappedPresenter presenter)
            {
                presenter.IsMinimizable = false;
                presenter.IsMaximizable = false;
            }
        }

        public static void ShowTitleBar(Window window)
        {
            window.ExtendsContentIntoTitleBar = false;

            if (GetAppWindow(window).Presenter is OverlappedPresenter presenter)
            {
                presenter.IsMinimizable = true;
                presenter.IsMaximizable = true;
            }
        }

        public static void DisableResizing(Window window)
        {
            if (GetAppWindow(window).Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
            }
        }

        public static void EnableResizing(Window window)
        {
            if (GetAppWindow(window).Presenter is OverlappedPresenter presenter)
            {
                presenter.IsResizable = true;
                presenter.IsMaximizable = true;
            }
        }

        public static void SetSize(Window window, int width = 1280, int height = 720)
        {
            GetAppWindow(window).Resize(new SizeInt32(width, height));
        }

        public static Task<ContentDialogResult> ShowContentDialogAsync(Func<ContentDialog> dialogFactory)
        {
            var tcs = new TaskCompletionSource<ContentDialogResult>();

            // Always use the main window's dispatcher
            var dispatcher = MainWindow.Instance.DispatcherQueue;

            dispatcher.TryEnqueue(async () =>
            {
                try
                {
                    // Create dialog on UI thread
                    var dialog = dialogFactory();

                    // Ensure XamlRoot is set
                    if (dialog.XamlRoot is null)
                    {
                        dialog.XamlRoot = MainWindow.Instance.Content.XamlRoot;
                    }

                    var result = await dialog.ShowAsync();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        public static Task<ContentDialogResult> ShowMessageAsync(
            string title,
            string message,
            string closeButtonText = "OK")
        {
            return ShowContentDialogAsync(() =>
                new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = closeButtonText,
                    DefaultButton = ContentDialogButton.Close
                });
        }

        public static Task<ContentDialogResult> ShowMessageAsync(
            string title,
            string message,
            string primaryButtonText,
            string closeButtonText)
        {
            return ShowContentDialogAsync(() =>
                new ContentDialog
                {
                    Title = title,
                    Content = message,
                    PrimaryButtonText = primaryButtonText,
                    CloseButtonText = closeButtonText,
                    DefaultButton = ContentDialogButton.Primary
                });
        }
    }
}
