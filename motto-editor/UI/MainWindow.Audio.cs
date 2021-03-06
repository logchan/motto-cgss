﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using motto_cgss_core;
using NAudio.Wave;

namespace motto_editor.UI
{
    public partial class MainWindow
    {
        private WasapiOut _musicOut = null;
        private WaveFileReader _musicWave = null;

        public bool PlayingMusic => (_musicOut?.PlaybackState ?? PlaybackState.Stopped) == PlaybackState.Playing;

        private void StartPlaying(int time)
        {
            if (_musicWave == null) return;

            _musicWave.CurrentTime = TimeSpan.FromMilliseconds(time);
            _musicOut?.Play();
            StartUpdater();
        }

        private void PausePlaying()
        {
            _musicOut?.Stop();
            StopUpdater();
        }
    }
}
