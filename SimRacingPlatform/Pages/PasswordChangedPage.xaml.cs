using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SimRacingPlatform.Windows;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimRacingPlatform.Pages
{
    public sealed partial class PasswordChangedPage : Page, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private int _secondsRemaining = 3;

        private string _countdownText = "Redirecting in 3 seconds...";
        public string CountdownText
        {
            get => _countdownText;
            set
            {
                if (_countdownText != value)
                {
                    _countdownText = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PasswordChangedPage()
        {
            InitializeComponent();
            DataContext = this;
            StartCountdown();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            MainWindow.Instance.NavigateTo(typeof(LoginPage));
        }
    }
}
