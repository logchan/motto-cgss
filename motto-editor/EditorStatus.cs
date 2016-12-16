using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using motto_cgss_core.Model;
using System.ComponentModel;

namespace motto_editor
{
    public class EditorStatus : INotifyPropertyChanged
    {
        public static EditorStatus Current { get; } = new EditorStatus();

        public BeatmapInfo CurrentBeatmap { get; set; }
        public Beatmap EditingMap { get; set; }
        public Note SelectedNote { get; set; }

        private int _musicLength;

        public int MusicLength
        {
            get { return _musicLength; }
            set
            {
                _musicLength = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MusicLength)));
            }
        }

        private int _currentTime;

        public int CurrentTime
        {
            get { return _currentTime; }
            set
            {
                _currentTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));
            }
        }

        private int _currentSection;

        public int CurrentSection
        {
            get { return _currentSection; }
            set
            {
                _currentSection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSection)));
            }
        }

        private int _currentBeat;

        public int CurrentBeat
        {
            get { return _currentBeat; }
            set
            {
                _currentBeat = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentBeat)));
            }
        }

        private int _currentSubBeat;

        public int CurrentSubBeat
        {
            get { return _currentSubBeat; }
            set
            {
                _currentSubBeat = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentSubBeat)));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }
}
