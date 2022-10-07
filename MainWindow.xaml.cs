using Newtonsoft.Json;
using PTFinalProject.Core.Queue;
using PTFinalProject.Core.Songs;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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

        private static object PLAY_ICON = Application.Current.Resources["Play"];
        private static object PAUSE_ICON = Application.Current.Resources["Pause"];

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
                QueueSong currentQueueSong = queueSong;

                bool isCurrent = queueSong == songsManager.Queue.GetCurrentSong();
                Song song = currentQueueSong.GetCurrentSong();

                Grid songContainer = new();
                if (isCurrent) songContainer.Background = Brushes.LightSeaGreen;

                songContainer.RowDefinitions.Add(new RowDefinition());

                songContainer.ColumnDefinitions.Add(new ColumnDefinition());
                songContainer.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                #region Song Information
                StackPanel songInformation = new() { Margin = new Thickness(16, 0, 0, 0) };

                Grid.SetColumn(songInformation, 0);
                Grid.SetRow(songInformation, 0);

                Brush LabelColor = isCurrent ? Brushes.White : Brushes.Black;

                Label songName = new() {
                    Content = song.GetName(),
                    FontWeight = FontWeight.FromOpenTypeWeight(600),
                    FontSize = 14,
                    Foreground = LabelColor
                };

                Label songArtists = new() {
                    Content = song.GetArtists(),
                    FontSize = 12,
                    Foreground = LabelColor,
                    Margin = new Thickness(0, -12, 0, 0)
                };

                songInformation.Children.Add(songName);
                songInformation.Children.Add(songArtists);

                songContainer.Children.Add(songInformation);
                #endregion

                #region Queue Actions
                StackPanel queueActions = new() {
                    Margin = new Thickness(0, 0, 16, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                Grid.SetColumn(queueActions, 1);
                Grid.SetRow(queueActions, 0);

                #region Remove from Queue
                Button removeFromQueue = new() {
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    Height = 35,
                    Width = 35
                };

                removeFromQueue.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) =>
                {
                    if (playing && player != null && songsManager.Queue.GetCurrentSong() == currentQueueSong)
                    {
                        player.Stop();
                        player.Close();

                        player = null;
                        playing = false;

                        Application.Current.Resources["PlayingMusicPlayer"] = PLAY_ICON;
                    }

                    songsManager.Queue.RemoveSong(currentQueueSong);

                    UpdateMusicPlayerControls();
                    PrintQueueSongs();
                });

                Viewbox removeFromQueueIconBox = new() { Height = 20, Width = 20 };

                Path removeFromQueueIcon = new() {
                    Data = Geometry.Parse(Application.Current.Resources["RemoveFromQueue"].ToString()),
                    Fill = isCurrent ? Brushes.White : Brushes.LightSeaGreen
                };

                removeFromQueueIconBox.Child = removeFromQueueIcon;
                removeFromQueue.Content = removeFromQueueIconBox;
                queueActions.Children.Add(removeFromQueue);
                #endregion

                songContainer.Children.Add(queueActions);
                #endregion

                queueSongs.Children.Add(songContainer);

                queueSong = queueSong.GetNextSong();
            }
        }

        private void PrintAvailableSongs()
        {
            availableSongs.Children.Clear();
            availableSongs.RowDefinitions.Clear();

            for (int rowI = 0; rowI < (songsManager.Songs.Count + 1) / 2; rowI++)
                availableSongs.RowDefinitions.Add(new RowDefinition());

            int i = 0, row = 0;
            foreach (Song song in songsManager.Songs)
            {
                bool isFirstColumn = i % 2 == 0;

                Grid songContainer = new() {
                    Margin = new Thickness(isFirstColumn ? 0 : 8, 0, isFirstColumn ? 8 : 0, 16),
                    VerticalAlignment = VerticalAlignment.Center
                };

                Grid.SetColumn(songContainer, isFirstColumn ? 0 : 1);
                Grid.SetRow(songContainer, row);

                songContainer.RowDefinitions.Add(new RowDefinition());

                songContainer.ColumnDefinitions.Add(new ColumnDefinition());
                songContainer.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                #region Song Information
                StackPanel songInformation = new() { VerticalAlignment = VerticalAlignment.Center };

                Grid.SetColumn(songInformation, 0);
                Grid.SetRow(songInformation, 0);

                Label songName = new()
                {
                    Content = song.GetName(),
                    FontWeight = FontWeight.FromOpenTypeWeight(600),
                    FontSize = 14,
                };

                Label songArtists = new()
                {
                    Content = song.GetArtists(),
                    FontSize = 12,
                    Margin = new Thickness(0, -12, 0, 0)
                };

                songInformation.Children.Add(songName);
                songInformation.Children.Add(songArtists);

                songContainer.Children.Add(songInformation);
                #endregion

                #region Song Actions
                Grid actionsContainer = new();
                Grid.SetColumn(actionsContainer, 1);
                Grid.SetRow(actionsContainer, 0);

                actionsContainer.RowDefinitions.Add(new RowDefinition());
                actionsContainer.RowDefinitions.Add(new RowDefinition());

                actionsContainer.ColumnDefinitions.Add(new ColumnDefinition());

                #region Add to Queue Button
                Button addToQueue = new() {
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    Height = 35,
                    Width = 35
                };

                addToQueue.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) =>
                {
                    songsManager.Queue.AddSong(song);

                    QueueSong? playingSong = songsManager.Queue.GetCurrentSong();
                    if (playingSong != null) UpdateMusicPlayerControls();

                    PrintQueueSongs();
                });

                Grid.SetColumn(addToQueue, 0);
                Grid.SetRow(addToQueue, 0);

                Viewbox addToQueueIconBox = new() { Height = 20, Width = 20 };

                Path addToQueueIcon = new() {
                    Data = Geometry.Parse(Application.Current.Resources["AddToQueue"].ToString()),
                    Fill = Brushes.LightSeaGreen
                };

                addToQueueIconBox.Child = addToQueueIcon;
                addToQueue.Content = addToQueueIconBox;
                actionsContainer.Children.Add(addToQueue);
                #endregion

                #region Add/Remove Favorite Button
                Button favoriteButton = new()
                {
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    Height = 35,
                    Margin = new Thickness(0, 4, 0, 0),
                    Width = 35
                };

                Grid.SetColumn(favoriteButton, 0);
                Grid.SetRow(favoriteButton, 1);

                Viewbox favoriteIconBox = new() {
                    Height = 20,
                    Margin = new Thickness(-2, -4, 0, 0),
                    Width = 20
                };

                Path favoriteIcon = new() { Fill = Brushes.Yellow };

                if (song.IsFavorite())
                {
                    favoriteButton.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) =>
                    {
                        songsManager.RemoveFavorite(song.GetId());
                        songsManager.SaveFavorites();

                        PrintAvailableSongs();
                    });

                    favoriteIcon.Data = Geometry.Parse(Application.Current.Resources["RemoveFromFavorites"].ToString());
                }
                else
                {
                    favoriteButton.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) =>
                    {
                        songsManager.AddFavorite(song.GetId());
                        songsManager.SaveFavorites();

                        PrintAvailableSongs();
                    });

                    favoriteIcon.Data = Geometry.Parse(Application.Current.Resources["AddToFavorites"].ToString());
                }

                favoriteIconBox.Child = favoriteIcon;
                favoriteButton.Content = favoriteIconBox;
                actionsContainer.Children.Add(favoriteButton);
                #endregion

                songContainer.Children.Add(actionsContainer);
                #endregion

                availableSongs.Children.Add(songContainer);

                if ((i + 1) % 2 == 0) row++;
                i++;
            }
        }

        private void UpdateMusicPlayerControls()
        {
            QueueSong? playingSong = songsManager.Queue.GetCurrentSong();
            if (playingSong == null)
            {
                labelSongName.Content = "Selecciona una canción";
                labelSongArtists.Content = "";

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
                    Application.Current.Resources["PlayingMusicPlayer"] = PLAY_ICON;
                    playing = false;
                }

                return;
            }

            QueueSong? nextSong = currentSong.GetNextSong();
            if (nextSong == null)
            {
                if (playing)
                {
                    Application.Current.Resources["PlayingMusicPlayer"] = PLAY_ICON;
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
            player = new();

            player.MediaEnded += new EventHandler((object? sender, EventArgs e) =>
            {
                Application.Current.Resources["PlayingMusicPlayer"] = PLAY_ICON;

                PlayNextSong();
            });

            if (song.IsLocalSource())
                player.Open(new Uri(song.GetId(), UriKind.RelativeOrAbsolute));
            else
                player.Open(new Uri("http://localhost:3005/stream/" + song.GetUrl()));

            player.Play();
            playing = true;

            Application.Current.Resources["PlayingMusicPlayer"] = PAUSE_ICON;
        }

        #region Buttons
        private void ButtonSearchYouTube_Click(object sender, RoutedEventArgs e)
        {
            string youTubeUrl = inputYouTubeSearchText.Text;
            if (
                !youTubeUrl.StartsWith("https://youtube.com") &&
                !youTubeUrl.StartsWith("https://music.youtube.com") &&
                !youTubeUrl.Contains("?v=")
            ) {
                MessageBox.Show("Debes ingresar una URL de YouTube válida.");
                inputYouTubeSearchText.Clear();
                return;
            }

            inputYouTubeSearchText.Clear();

            HttpClient client = new();

            HttpResponseMessage response = client.GetAsync("http://localhost:3005/info/" + youTubeUrl).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                string dataString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                SongYouTubeResponse? data = JsonConvert.DeserializeObject<SongYouTubeResponse>(dataString);
                if (data == null)
                {
                    MessageBox.Show("No se ha podido encontrar la canción.");
                    return;
                }

                songsManager.Songs.Add(Song.FromYouTube(data.Id, data.Name, data.Artists, 0, youTubeUrl));
                PrintAvailableSongs();
            }
            else
            {
                MessageBox.Show("No se ha podido encontrar la canción.");
            }
        }

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
                Application.Current.Resources["PlayingMusicPlayer"] = PLAY_ICON;
                player.Pause();
            }
            else
            {
                Application.Current.Resources["PlayingMusicPlayer"] = PAUSE_ICON;
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
