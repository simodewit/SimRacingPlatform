using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SimRacingPlatform.Pages
{
    public sealed partial class LandingPage : Page, INotifyPropertyChanged
    {
        private const string PlaceholderImage = "ms-appx:///Assets/SquareLogo.png";

        public ObservableCollection<PatchNote> PatchNotes { get; } = new();
        public ObservableCollection<Announcement> Announcements { get; } = new();
        public ObservableCollection<NewsArticle> NewsArticles { get; } = new();

        private int _selectedAnnouncementIndex;
        public int SelectedAnnouncementIndex
        {
            get => _selectedAnnouncementIndex;
            set
            {
                if (value == _selectedAnnouncementIndex) return;
                _selectedAnnouncementIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedAnnouncement));
                OnPropertyChanged(nameof(AnnouncementPositionText));
            }
        }

        public Announcement? SelectedAnnouncement =>
            (Announcements.Count > 0 &&
             SelectedAnnouncementIndex >= 0 &&
             SelectedAnnouncementIndex < Announcements.Count)
                ? Announcements[SelectedAnnouncementIndex]
                : null;

        public string AnnouncementPositionText =>
            Announcements.Count == 0 ? "0 / 0" : $"{SelectedAnnouncementIndex + 1} / {Announcements.Count}";

        public LandingPage()
        {
            InitializeComponent();
            DataContext = this;

            LoadSampleData();

            SelectedAnnouncementIndex = Announcements.Count > 0 ? 0 : -1;
        }

        private void LoadSampleData()
        {
            // --- Patch Notes (right panel) ---
            PatchNotes.Add(new PatchNote
            {
                Version = "v1.0.0",
                ShortTitle = "Initial release",
                Summary = "Account system + updater foundation + core app shell. Landing page scaffolding started.",
                Date = "2026-01-17",
                Tag = "Major"
            });

            PatchNotes.Add(new PatchNote
            {
                Version = "v1.0.1",
                ShortTitle = "Stability & polish",
                Summary = "Improved startup flow, fixed edge cases in login refresh, small UI cleanup.",
                Date = "2026-01-20",
                Tag = "Minor"
            });

            PatchNotes.Add(new PatchNote
            {
                Version = "v1.1.0",
                ShortTitle = "Content layout pass",
                Summary = "Announcement carousel + news list + patch notes sidebar. Click handlers stubbed for later routing.",
                Date = "2026-02-01",
                Tag = "Feature"
            });

            PatchNotes.Add(new PatchNote
            {
                Version = "v1.1.1",
                ShortTitle = "Landing page iteration",
                Summary = "Added card separation and clearer section boundaries. Replaced placeholders with SquareLogo asset.",
                Date = "2026-02-03",
                Tag = "UI"
            });

            // --- Announcements (top-left carousel) ---
            Announcements.Add(new Announcement
            {
                Title = "Upcoming Overlay Feature",
                Description = "The overlay button is coming soon — quick tools, shortcuts, and contextual panels.",
                ImagePath = PlaceholderImage
            });

            Announcements.Add(new Announcement
            {
                Title = "Launch Week Events",
                Description = "Special in-app events, new drops, and early preview content for first adopters.",
                ImagePath = PlaceholderImage
            });

            Announcements.Add(new Announcement
            {
                Title = "Roadmap Sneak Peek",
                Description = "Themes, custom dashboards, and deeper integrations are on the way.",
                ImagePath = PlaceholderImage
            });

            // --- News Articles (bottom-left list) ---
            NewsArticles.Add(new NewsArticle
            {
                Title = "How the Auto-Updater Works (Overview)",
                Category = "Dev Log",
                ThumbnailPath = PlaceholderImage,
                ArticleId = "auto-updater"
            });

            NewsArticles.Add(new NewsArticle
            {
                Title = "Landing Page UI: Structure & Goals",
                Category = "Design",
                ThumbnailPath = PlaceholderImage,
                ArticleId = "landing-page-structure"
            });

            NewsArticles.Add(new NewsArticle
            {
                Title = "Accounts: Security + Token Refresh Plan",
                Category = "Guide",
                ThumbnailPath = PlaceholderImage,
                ArticleId = "accounts-security"
            });

            NewsArticles.Add(new NewsArticle
            {
                Title = "What’s Next: Version 1.1 Content Pipeline",
                Category = "Roadmap",
                ThumbnailPath = PlaceholderImage,
                ArticleId = "v1-1-content"
            });
        }

        // --- Announcement navigation ---
        private void PreviousAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            if (Announcements.Count == 0) return;

            SelectedAnnouncementIndex--;
            if (SelectedAnnouncementIndex < 0)
                SelectedAnnouncementIndex = Announcements.Count - 1;
        }

        private void NextAnnouncement_Click(object sender, RoutedEventArgs e)
        {
            if (Announcements.Count == 0) return;

            SelectedAnnouncementIndex++;
            if (SelectedAnnouncementIndex >= Announcements.Count)
                SelectedAnnouncementIndex = 0;
        }

        // --- Click stubs (for later) ---
        private void PatchNotesListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // TODO: Navigate to full patch notes page
            // var note = (PatchNote)e.ClickedItem;
        }

        private void NewsArticle_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigate to article page
            // var article = (NewsArticle)((FrameworkElement)sender).DataContext;
        }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // ================== DATA MODELS ==================

    public class PatchNote
    {
        public string Version { get; set; } = "";
        public string ShortTitle { get; set; } = "";
        public string Summary { get; set; } = "";
        public string Date { get; set; } = "";
        public string Tag { get; set; } = "";
    }

    public class Announcement
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImagePath { get; set; } = "";
    }

    public class NewsArticle
    {
        public string Title { get; set; } = "";
        public string Category { get; set; } = "";
        public string ThumbnailPath { get; set; } = "";
        public string ArticleId { get; set; } = "";
    }
}
