﻿using System;
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

        // TODO: re-design how time changing shall be implemented :<

        private void ExecuteCmdLeft(object sender, ExecutedRoutedEventArgs args)
        {
            var status = EditorStatus.Current;
            var bm = status.EditingMap;
            if (bm == null) return;

            var step = 48/RulerCanvas.BeatDivision;
            var diff = status.CurrentSubBeat % step;
            var beat = status.CurrentBeat;
            var subBeat = status.CurrentSubBeat;

            if (diff > 0)
            {
                subBeat -= diff;
            }
            else
            {
                subBeat -= step;
                if (subBeat < 0)
                {
                    subBeat += 48;
                    --beat;
                }

                if (beat < 0)
                {
                    // TODO: handle cross-section
                    if (status.CurrentSection > 0)
                    {
                        var oldSection = bm.Sections[status.CurrentSection];
                        --status.CurrentSection;
                        var newSection = bm.Sections[status.CurrentSection];
                        newSection.TimeToBeats(oldSection.BeatToTime(beat, subBeat), out beat, out subBeat);
                    }
                }
            }

            status.CurrentBeat = beat;
            status.CurrentSubBeat = subBeat;
            SetCurrentTimeFromBeat();

            if (status.CurrentTime < 0)
            {
                status.CurrentTime = 0;
                SetCurrentBeatFromTime();
            }
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
            var step = 48 / RulerCanvas.BeatDivision;

            subBeat += step - subBeat % step;
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
