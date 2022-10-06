using PTFinalProject.Core.Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace PTFinalProject.Core.Songs
{
    class SongsManager
    {
        public readonly List<Song> Songs = new();
        private readonly List<string> favoriteIds = new();

        public readonly Queue.Queue Queue = new();

        private void LoadLocalFiles(string Folder, bool CheckSubdirectories = true)
        {
            if (!Directory.Exists(Folder)) return;

            if (CheckSubdirectories)
            {
                foreach (string dir in Directory.EnumerateDirectories(Folder))
                {
                    LoadLocalFiles(dir, CheckSubdirectories);
                }
            }

            foreach (string filePath in Directory.EnumerateFiles(Folder))
            {
                if (filePath.EndsWith(".mp3"))
                {
                    TagLib.File file = TagLib.File.Create(filePath);

                    string fileName = file.Name.Split('\\').Reverse().ToArray()[0];

                    string songName = file.Tag.Title;
                    string songArtists = string.Join(", ", file.Tag.Performers);

                    Songs.Add(Song.FromLocal(
                        file.Name,
                        songName.Length == 0 ? fileName : songName,
                        songArtists.Length == 0 ? "Desconocido" : songArtists,
                        file.Properties.Duration.TotalSeconds,
                        file.Tag
                   ));
                }
            }
        }

        private static string GetAppPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\PTFinalProject";
        }

        private static string GetFavoritesFilePath()
        {
            return GetAppPath() + "\\favorites.txt";
        }

        private void LoadFavorites()
        {
            string filePath = GetFavoritesFilePath();
            if (!File.Exists(filePath)) return;

            string[] favorites = File.ReadAllText(filePath).Split("|||");
            foreach (string favorite in favorites)
                AddFavorite(favorite);

            SaveFavorites();
        }

        public void Initialize()
        {
            string appPath = GetAppPath();
            if (!Directory.Exists(appPath)) Directory.CreateDirectory(appPath);
            
            LoadLocalFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic));
            LoadLocalFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));

            LoadFavorites();
        }

        public void AddFavorite(string SongId)
        {
            if (favoriteIds.Exists((t) => t == SongId)) return;

            Song? song = Songs.Find((s) => s.GetId() == SongId);
            if (song == null) return;

            favoriteIds.Add(SongId);
            song.SetFavorite(true);
        }

        public void RemoveFavorite(string SongId)
        {
            favoriteIds.Remove(SongId);

            Song? song = Songs.Find((s) => s.GetId() == SongId);
            if (song == null) return;

            song.SetFavorite(false);
        }

        public void SaveFavorites()
        {
            string filePath = GetFavoritesFilePath();
            if (File.Exists(filePath)) File.Delete(filePath);

            FileStream file = File.Create(filePath);

            string toSave = string.Join("|||", favoriteIds);
            file.Write(Encoding.ASCII.GetBytes(toSave));
            file.Close();
        }
    }
}
