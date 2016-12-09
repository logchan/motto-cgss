using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace motto_editor.UI
{
    public partial class MainWindow
    {
        public static readonly RoutedUICommand OpenDifficultyCommand = new RoutedUICommand("OpenDifficulty", "OpenDifficulty", typeof(MainWindow),
            new InputGestureCollection
            {
                new KeyGesture(Key.O, ModifierKeys.Shift | ModifierKeys.Control)
            });

        public static readonly RoutedUICommand PlayPauseCommand = new RoutedUICommand("PlayPause", "PlayPause", typeof(MainWindow),new InputGestureCollection { new KeyGesture(Key.Space)});

        private void ExecuteCmdOpen(object sender, ExecutedRoutedEventArgs args)
        {
            OpenBeatmap();
        }

        private void ExecuteCmdOpenDifficulty(object sender, ExecutedRoutedEventArgs args)
        {
            OpenDifficulty();
        }

        private void ExecuteCmdPlayPause(object sender, ExecutedRoutedEventArgs args)
        {
            if (PlayingMusic)
                PausePlaying();
            else
                StartPlaying(EditorStatus.Current.CurrentTime);
        }
    }
}
