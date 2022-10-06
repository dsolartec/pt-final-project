using PTFinalProject.Core.Queue;
using PTFinalProject.Core.Songs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PTFinalProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SongsManager songsManager = new();

        private MediaPlayer? player = null;
        private bool playing = false;

        public MainWindow()
        {
            Application.Current.Resources["PlayingMusicPlayer"] = Application.Current.Resources["Play"];

            InitializeComponent();

            songsManager.Initialize();

            PrintQueueSongs();
            PrintAvailableSongs();
            UpdateMusicPlayerControls();
        }

        private void PrintQueueSongs()
        {
            queueSongs.Children.Clear();

            QueueSong? queueSong = songsManager.Queue.GetInitialSong();            
            while (queueSong != null)
            {
                bool isCurrent = queueSong == songsManager.Queue.GetCurrentSong();
                Song song = queueSong.GetCurrentSong();

                StackPanel songPanel = new();
                if (isCurrent) songPanel.Background = Brushes.LightSeaGreen;

                Label songName = new() { Content = song.GetName() };
                Label songArtists = new() { Content = song.GetArtists() };

                songPanel.Children.Add(songName);
                songPanel.Children.Add(songArtists);

                queueSongs.Children.Add(songPanel);
                Show();

                queueSong = queueSong.GetNextSong();
            }
        }

        private static string GetAvailableSongName(Song song)
        {
            return string.Format("aSong{0}", song.GetId().Replace('\\', '_').Replace(':', '_').Replace('.', '_').Replace(' ', '_').Trim());
        }

        private void PrintAvailableSongs()
        {
            availableSongs.Children.Clear();

            foreach (Song song in songsManager.Songs)
            {
                StackPanel songPanel = new() { Name = GetAvailableSongName(song) };

                Label songName = new() { Content = song.GetName() };
                Label songArtists = new() { Content = song.GetArtists() };

                Button SelectSong = new() { Content = "Agregar a la lista de reproducción" };
                SelectSong.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) =>
                {
                    songsManager.Queue.AddSong(song);
                    
                    QueueSong? playingSong = songsManager.Queue.GetCurrentSong();
                    if (playingSong != null) UpdateMusicPlayerControls();
                    
                    PrintQueueSongs();
                });

                songPanel.Children.Add(songName);
                songPanel.Children.Add(songArtists);
                songPanel.Children.Add(SelectSong);

                if (song.IsFavorite())
                {
                    Button removeFavorite = new() { Content = "Quitar favorito" };
                    removeFavorite.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) =>
                    {
                        songsManager.RemoveFavorite(song.GetId());
                        songsManager.SaveFavorites();

                        PrintAvailableSongs();
                    });

                    songPanel.Children.Add(removeFavorite);
                }
                else
                {
                    Button addFavorite = new() { Content = "Añadir a favoritos" };
                    addFavorite.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) =>
                    {
                        songsManager.AddFavorite(song.GetId());
                        songsManager.SaveFavorites();

                        PrintAvailableSongs();
                    });

                    songPanel.Children.Add(addFavorite);
                }

                availableSongs.Children.Add(songPanel);
            }
        }

        private void UpdateMusicPlayerControls()
        {
            QueueSong? playingSong = songsManager.Queue.GetCurrentSong();
            if (playingSong == null)
            {
                btnPreviousSong.IsEnabled = false;
                btnPlayOrStopSong.IsEnabled = false;
                btnNextSong.IsEnabled = false;
                return;
            }

            Song currentSong = playingSong.GetCurrentSong();

            labelSongName.Content = currentSong.GetName();
            labelSongArtists.Content = currentSong.GetArtists();

            btnPreviousSong.IsEnabled = playingSong.GetPreviousSong() != null;
            btnPlayOrStopSong.IsEnabled = true;
            btnNextSong.IsEnabled = playingSong.GetNextSong() != null;
        }

        private void StopPlayingSong(Song song)
        {
            if (player != null)
            {
                player.Stop();
                player.Close();

                player = null;

                if (playing) StartPlayingSong(song);
            }
        }

        private void PlayNextSong()
        {
            QueueSong? currentSong = songsManager.Queue.GetCurrentSong();
            if (currentSong == null)
            {
                if (playing)
                {
                    Application.Current.Resources["PlayingMusicPlayer"] = Application.Current.Resources["Play"];
                    playing = false;
                }

                return;
            }

            QueueSong? nextSong = currentSong.GetNextSong();
            if (nextSong == null)
            {
                if (playing)
                {
                    Application.Current.Resources["PlayingMusicPlayer"] = Application.Current.Resources["Play"];
                    playing = false;
                }

                return;
            }

            StopPlayingSong(nextSong.GetCurrentSong());

            songsManager.Queue.NextSong();

            UpdateMusicPlayerControls();
            PrintQueueSongs();
        }

        private void StartPlayingSong(Song song)
        {
            if (song.IsLocalSource())
            {
                player = new();

                //player.Open(new Uri(song.GetId(), UriKind.RelativeOrAbsolute));
                player.Open(new Uri("https://music.youtube.com/watch?v=53hsFyX4Hh"));
                player.Play();

                player.MediaEnded += new EventHandler((object? sender, EventArgs e) =>
                {
                    Application.Current.Resources["PlayingMusicPlayer"] = Application.Current.Resources["Play"];

                    PlayNextSong();
                });

                playing = true;

                Application.Current.Resources["PlayingMusicPlayer"] = Application.Current.Resources["Pause"];
            }
        }

        #region Buttons
        private void ButtonPreviousSong_Click(object sender, RoutedEventArgs e)
        {
            QueueSong? currentSong = songsManager.Queue.GetCurrentSong();
            if (currentSong == null) return;

            QueueSong? previousSong = currentSong.GetPreviousSong();
            if (previousSong == null) return;

            StopPlayingSong(previousSong.GetCurrentSong());

            songsManager.Queue.PreviousSong();

            UpdateMusicPlayerControls();
            PrintQueueSongs();
        }

        private void ButtonPlayOrStopSong_Click(object sender, RoutedEventArgs e)
        {
            if (player == null)
            {
                QueueSong? currentSong = songsManager.Queue.GetCurrentSong();
                if (currentSong == null) return;

                StartPlayingSong(currentSong.GetCurrentSong());
                return;
            }

            if (playing)
            {
                Application.Current.Resources["PlayingMusicPlayer"] = Application.Current.Resources["Play"];
                player.Pause();
            }
            else
            {
                Application.Current.Resources["PlayingMusicPlayer"] = Application.Current.Resources["Pause"];
                player.Play();
            }

            playing = !playing;
        }

        private void ButtonNextSong_Click(object sender, RoutedEventArgs e)
        {
            PlayNextSong();
        }
        #endregion
    }
}
