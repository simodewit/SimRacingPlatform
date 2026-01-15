using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Windows;
using System;

namespace SimRacingPlatform.Pages
{
    public sealed partial class EmailConfirmedPage : Page
    {
        private DispatcherTimer _timer;
        private int _secondsRemaining = 5;

        public string CountdownText { get; set; } = "Redirecting in 5 seconds...";

        public EmailConfirmedPage()
        {
            InitializeComponent();
            StartCountdown();
        }

        private void StartCountdown()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        private void OnTimerTick(object sender, object e)
        {
            _secondsRemaining--;

            if (_secondsRemaining > 0)
            {
                CountdownText = $"Redirecting in {_secondsRemaining} seconds...";
            }
            else
            {
                _timer.Stop();
                CountdownText = "Redirecting now...";
                RedirectUser();
            }
        }

        private void RedirectUser()
        {
            MainWindow.Instance.NavigateTo(typeof(LandingPage));
        }
    }
}
