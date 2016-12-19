using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using motto_cgss_core.Model;

namespace motto_editor.UI
{
    public partial class MainWindow
    {
        private const double TargetFps = 240;

        private bool _autoUpdate = true;
        private volatile bool _updaterRunning;

        private void Update()
        {
            EditorCanvas.Compute();
            EditorCanvas.InvalidateVisual();
            RulerCanvas.InvalidateVisual();
        }

        private void SetCurrentBeatFromTime()
        {
            var status = EditorStatus.Current;
            var bm = status.EditingMap;
            if (bm == null)
                return;

            var time = status.CurrentTime;
            var sectionId = 0;
            for (int i = 1; i < bm.Sections.Count; ++i)
            {
                if (bm.Sections[i].StartTime > time)
                {
                    sectionId = i - 1;
                    break;
                }
            }

            int beat, subBeat;
            bm.Sections[sectionId].TimeToBeats(time/1000.0, out beat, out subBeat);


            status.CurrentBeat = beat;
            status.CurrentSubBeat = subBeat;
            status.CurrentSection = sectionId;
        }

        private void SetCurrentTimeFromBeat()
        {
            var status = EditorStatus.Current;
            status.CurrentTime = (int)(1000 * status.EditingMap.Sections[status.CurrentSection].BeatToTime(status.CurrentBeat, status.CurrentSubBeat));
        }

        private void CurrentTimeChanged(object sender, PropertyChangedEventArgs args)
        {
            if (_autoUpdate && args.PropertyName == nameof(EditorStatus.CurrentTime))
            {
                Update();
            }
        }

        private void StartUpdater()
        {
            if (_updaterRunning)
                return;

            var task = new Task(UpdaterTask);
            task.Start();
        }

        private void StopUpdater()
        {
            _updaterRunning = false;
        }

        private void UpdaterTask()
        {
            _updaterRunning = true;
            _autoUpdate = false;

            var targetTpf = 1000/TargetFps;
            var lastMusicTime = 0;
            var lastComputedTime = 0;
            var lastStartTime = DateTime.UtcNow;

            while (_updaterRunning)
            {
                var startTime = DateTime.UtcNow;

                var musicTime = (int)_musicWave.CurrentTime.TotalMilliseconds;
                if (musicTime == lastMusicTime)
                {
                    musicTime = lastComputedTime + (int) (lastStartTime - startTime).TotalMilliseconds;
                }
                else
                {
                    lastMusicTime = musicTime;
                }
                if (musicTime > lastComputedTime)
                    lastComputedTime = musicTime;

                lastStartTime = startTime;

                EditorStatus.Current.CurrentTime = musicTime;
                SetCurrentBeatFromTime();

                RulerCanvas.Render();
                EditorCanvas.ComputeAndRender();

                var endTime = DateTime.UtcNow;
                var elapsedMs = (endTime - startTime).TotalMilliseconds;
                if (elapsedMs < targetTpf)
                {
                    Thread.Sleep((int)(targetTpf - elapsedMs));
                }
            }

            _autoUpdate = true;
        }
    }
}
