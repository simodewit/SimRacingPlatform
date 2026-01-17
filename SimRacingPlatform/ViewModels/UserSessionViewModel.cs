using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimRacingPlatform.ViewModels
{
    public sealed class UserSessionViewModel : INotifyPropertyChanged
    {
        public static UserSessionViewModel Instance { get; } = new();

        private UserSessionViewModel() { }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void Raise([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private string _displayName = "Guest";
        public string DisplayName
        {
            get => _displayName;
            set { if (_displayName != value) { _displayName = value; Raise(); Raise(nameof(Initials)); } }
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set { if (_email != value) { _email = value; Raise(); } }
        }

        private string _uid = "";
        public string Uid
        {
            get => _uid;
            set { if (_uid != value) { _uid = value; Raise(); } }
        }

        // Use this for AccountPage Image control
        private BitmapImage _profileImage = new(new Uri("ms-appx:///Assets/SquareLogo.png"));
        public BitmapImage ProfileImage
        {
            get => _profileImage;
            set { _profileImage = value; Raise(); }
        }

        // PersonPicture can’t bind to ImageSource directly, so we’ll bind this URL string and show Initials fallback.
        private string? _profilePhotoUrl;
        public string? ProfilePhotoUrl
        {
            get => _profilePhotoUrl;
            set { if (_profilePhotoUrl != value) { _profilePhotoUrl = value; Raise(); Raise(nameof(HasProfilePhoto)); } }
        }

        public bool HasProfilePhoto => !string.IsNullOrWhiteSpace(ProfilePhotoUrl);

        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DisplayName)) return "";
                var parts = DisplayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1) return parts[0].Length >= 2 ? parts[0][..2].ToUpperInvariant() : parts[0].ToUpperInvariant();
                return (parts[0][0].ToString() + parts[^1][0]).ToUpperInvariant();
            }
        }

        public void Clear()
        {
            DisplayName = "Guest";
            Email = "";
            Uid = "";
            ProfilePhotoUrl = null;
            ProfileImage = new BitmapImage(new Uri("ms-appx:///Assets/SquareLogo.png"));
        }
    }
}
