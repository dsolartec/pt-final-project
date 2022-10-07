using System;
using System.Diagnostics;
using System.Text;

namespace PTFinalProject.Core.Songs
{
    enum SongSource
    {
        Local,
        YouTube,
    }

    class SongYouTubeResponse
    {
        public string Artists { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }

    class Song
    {
        private readonly string Id;

        private readonly string Name;
        private readonly string Artists;
        private readonly double Duration = 0;

        private readonly SongSource Source;
        private readonly TagLib.Tag? Tags = null;
        private readonly string? Url = null;

        private bool Favorite = false;

        private Song(
            string SongId,
            string SongName,
            string SongArtists,
            double SongDuration,
            SongSource Source,
            TagLib.Tag? Tags = null,
            string? YouTubeURL = null
        ) {
            Id = SongId;

            Name = SongName;
            Duration = SongDuration;
            Artists = SongArtists;

            this.Source = Source;
            this.Tags = Tags;
            Url = YouTubeURL;
        }

        public static Song FromLocal(
            string SongId,
            string SongName,
            string SongArtists,
            double SongDuration,
            TagLib.Tag Tags
        ) {
            return new Song(SongId, SongName, SongArtists, SongDuration, SongSource.Local, Tags);
        }

        public static Song FromYouTube(
            string SongId,
            string SongName,
            string SongArtists,
            double SongDuration,
            string Url
        ) {
            return new Song(SongId, SongName, SongArtists, SongDuration, SongSource.YouTube, null, Url);
        }
        
        public string GetId() { return Id; }

        public string GetName() { return Name; }
        public string GetArtists() { return Artists; }
        public double GetDuration() { return Duration; }

        public bool IsLocalSource() { return Source == SongSource.Local; }
        public bool IsYouTubeSource() { return Source == SongSource.YouTube; }
        public TagLib.Tag? GetTags() { return Tags; }
        public string? GetUrl() { return Url; }

        public bool IsFavorite() { return Favorite; }
        public void SetFavorite(bool favorite) { Favorite = favorite; }
    }
}
