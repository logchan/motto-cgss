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
        #region Commands
        public static readonly RoutedUICommand OpenDifficultyCommand = new RoutedUICommand("OpenDifficulty", "OpenDifficulty", typeof(MainWindow),
            new InputGestureCollection
            {
                new KeyGesture(Key.O, ModifierKeys.Shift | ModifierKeys.Control)
            });

        public static readonly RoutedUICommand PlayPauseCommand = new RoutedUICommand("PlayPause", "PlayPause", typeof(MainWindow),new InputGestureCollection { new KeyGesture(Key.Space)});

        // TODO: change key to arrows after removing slider
        public static readonly RoutedUICommand LeftCommand = new RoutedUICommand("Left", "Left", typeof(MainWindow), new InputGestureCollection {new KeyGesture(Key.OemMinus)});

        public static readonly RoutedUICommand ShiftLeftCommand = new RoutedUICommand("ShiftLeft", "ShiftLeft", typeof(MainWindow), new InputGestureCollection { new KeyGesture(Key.Left, ModifierKeys.Shift) });

        public static readonly RoutedUICommand RightCommand = new RoutedUICommand("Right", "Right", typeof(MainWindow), new InputGestureCollection { new KeyGesture(Key.OemPlus) });

        public static readonly RoutedUICommand ShiftRightCommand = new RoutedUICommand("ShiftRight", "ShiftRight", typeof(MainWindow), new InputGestureCollection { new KeyGesture(Key.Right, ModifierKeys.Shift) });

        #endregion

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

        private void ExecuteCmdLeft(object sender, ExecutedRoutedEventArgs args)
        {
            var status = EditorStatus.Current;
            if (status.EditingMap == null) return;

            if (status.CurrentSubBeat < 12)
            {
                if (status.CurrentBeat == 0)
                {
                    if (status.CurrentSection == 0)
                    {
                        status.CurrentSubBeat = 0;
                    }
                    else
                    {
                        var currSection = status.EditingMap.Sections[status.CurrentSection];
                        --status.CurrentSection;
                        var prevSection = status.EditingMap.Sections[status.CurrentSection];
                        int beat, subBeat;
                        prevSection.TimeToBeats(currSection.StartTime, out beat, out subBeat);
                        subBeat = subBeat/12*12;
                        status.CurrentBeat = beat;
                        status.CurrentSubBeat = subBeat;
                    }
                }
                else
                {
                    status.CurrentSubBeat += 36;
                    status.CurrentBeat -= 1;
                }
            }
            else
            {
                status.CurrentSubBeat -= 12;
            }

            SetCurrentTimeFromBeat();
        }

        private void ExecuteCmdShiftLeft(object sender, ExecutedRoutedEventArgs args)
        {

        }

        private void ExecuteCmdRight(object sender, ExecutedRoutedEventArgs args)
        {
            var status = EditorStatus.Current;
            if (status.EditingMap == null) return;

            var beat = status.CurrentBeat;
            var subBeat = status.CurrentSubBeat;

            subBeat += 12 - subBeat % 12;
            if (subBeat > 47)
            {
                beat += 1;
                subBeat -= 48;
            }

            // TODO: handle cross section

            status.CurrentBeat = beat;
            status.CurrentSubBeat = subBeat;

            SetCurrentTimeFromBeat();
        }

        private void ExecuteCmdShiftRight(object sender, ExecutedRoutedEventArgs args)
        {

        }
    }
}
