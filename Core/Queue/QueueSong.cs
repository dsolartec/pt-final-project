using PTFinalProject.Core.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTFinalProject.Core.Queue
{
    class QueueSong
    {
        private readonly int Id;
        private readonly Song CurrentSong;

        private QueueSong? PreviousSong = null;
        private QueueSong? NextSong = null;

        public QueueSong(int id, Song song)
        {
            Id = id;
            CurrentSong = song;
        }

        public int GetId() { return Id; }
        public Song GetCurrentSong() { return CurrentSong; }

        public QueueSong? GetPreviousSong() { return PreviousSong; }

        public void SetPreviousSong(QueueSong previousSong) { PreviousSong = previousSong; }

        public QueueSong? GetNextSong() { return NextSong; }

        public void SetNextSong(QueueSong nextSong) { NextSong = nextSong; }
    }
}
