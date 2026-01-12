using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using Windows.UI;
using WinRT.Interop;

namespace SimRacingPlatform.Utilities
{
    public static class WindowUtility
    {
        public static Color standardBackground = Color.FromArgb(255,25,25,59);
        public static Color standardForeground = Color.FromArgb(255,46,255,255);

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

        public static void SetIcon(Window window, string iconPath)
        {
            var appWindow = GetAppWindow(window);
            appWindow.SetIcon(iconPath);
        }

        public static void SetTitleBarColors(Window window, Color background, Color foreground)
        {
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
    }
}
