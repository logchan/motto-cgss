using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using motto_cgss_core;
using motto_cgss_core.Model;
using motto_editor.Utility;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace motto_editor.UI
{
    public partial class MainWindow
    {
        private void OpenBeatmap()
        {
            MessageBox.Show("Coming soon (gugugu)", "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenDifficulty()
        {
            // TODO: prompt save
            CloseBeatmap();

            var dialog = new OpenFileDialog
            {
                Title = "Choose a difficulty file",
                Filter = "DiffTxt file (*.diff.txt)|*.diff.txt"
            };

            if (!(dialog.ShowDialog() ?? false))
                return;

            var bmfile = dialog.FileName;

            dialog.Title = "Choose a music file";
            dialog.Filter = "WAV file (*.wav)|*.wav";

            if (!(dialog.ShowDialog() ?? false))
                return;

            var wavfile = dialog.FileName;

            try
            {
                var bm = new Beatmap(new BeatmapInfo());
                bm.Parse(File.ReadAllLines(bmfile));

                var wav = FileHelper.ReadFileToMemoryStream(wavfile);
                _musicWave = new WaveFileReader(wav);
                _musicOut = new WasapiOut(AudioClientShareMode.Shared, 0);
                _musicOut.Init(_musicWave);

                // TODO: refactor
                CurrentGame.ButtonHandled = new List<bool>();
                CurrentGame.ButtonStates = new List<ButtonState>();
                CurrentGame.NotesCount = bm.Notes.Count;
                CurrentGame.NumberOfButtons = bm.NumberOfButtons;
                for (int i = 0; i < bm.NumberOfButtons; ++i)
                {
                    CurrentGame.ButtonHandled.Add(false);
                    CurrentGame.ButtonStates.Add(ButtonState.None);
                }

                EditorStatus.Current.SelectedNote = null;
                EditorStatus.Current.CurrentBeatmap = null;
                EditorStatus.Current.EditingMap = bm;
                EditorStatus.Current.MusicLength = (int)_musicWave.TotalTime.TotalMilliseconds;
                EditorStatus.Current.CurrentTime = 0;

                EditorCanvas.RecomputePositions();

                foreach (var note in bm.Notes)
                {
                    note.GameInit();
                }

                Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseBeatmap()
        {
            _musicOut?.Stop();
            _musicWave?.Dispose();
            _musicOut = null;
            _musicWave = null;
        }
    }
}
