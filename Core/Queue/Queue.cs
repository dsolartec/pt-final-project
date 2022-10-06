using PTFinalProject.Core.Songs;

namespace PTFinalProject.Core.Queue
{
    class Queue
    {
        private int LastId = 0;
        private QueueSong? Initial = null;
        private QueueSong? Current = null;

        public QueueSong? GetInitialSong() { return Initial; }
        public QueueSong? GetCurrentSong() { return Current; }

        public void AddSong(Song song)
        {
            QueueSong newSong = new(++LastId, song);

            Current ??= newSong;

            if (Initial == null)
            {
                Initial = newSong;
                return;
            }

            QueueSong? toSet = Initial;
            while (toSet != null && toSet.GetNextSong() != null)
                toSet = toSet.GetNextSong();

            if (toSet != null)
            {
                newSong.SetPreviousSong(toSet);
                toSet.SetNextSong(newSong);

                if (Current.GetId() == toSet.GetId()) Current = toSet;
            }
        }

        public void Clear()
        {
            Initial = null;
            Current = null;
        }
    
        public void PreviousSong()
        {
            if (Current == null || Current.GetPreviousSong() == null) return;

            Current = Current.GetPreviousSong();
        }

        public void NextSong()
        {
            if (Current == null || Current.GetNextSong() == null) return;

            Current = Current.GetNextSong();
        }
    }
}
