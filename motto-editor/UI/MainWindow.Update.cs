using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
