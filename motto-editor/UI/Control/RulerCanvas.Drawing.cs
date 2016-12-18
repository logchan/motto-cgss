using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace motto_editor.UI.Control
{
    internal partial class RulerCanvas
    {
        private readonly EventWaitHandle _renderHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private const double CurrentBeatLinePosition = 0.382;
        private const int BeatLineThickness = 3;
        private const int SubBeatLineThickness = 1;

        private static readonly SolidColorBrush BackgroundFillBrush = Brushes.DarkBlue;
        private static readonly Pen BeatLinePen = new Pen(Brushes.White, BeatLineThickness);
        private static readonly Pen SubBeatLinePen = new Pen(Brushes.Yellow, SubBeatLineThickness);
        private static readonly Pen CurrentBeatLinePen = new Pen(Brushes.Red, SubBeatLineThickness);

        public void Render()
        {
            Dispatcher.Invoke(InvalidateVisual);
            _renderHandle.WaitOne();
        }

        protected override void OnRender(DrawingContext dc)
        {
            var w = ActualWidth;
            var h = ActualHeight;
            dc.DrawRectangle(BackgroundFillBrush, null, new Rect(0, 0, w, h));

            var status = EditorStatus.Current;
            var bm = status.EditingMap;
            if (bm == null)
            {
                _renderHandle.Set();
                return;
            }
                
            var centerX = w*CurrentBeatLinePosition;

            var step = 48/BeatDivision;
            var stepWidth = BeatWidth/BeatDivision;
            var diff = status.CurrentSubBeat % step;

            var beat = status.CurrentBeat;
            var subBeat = status.CurrentSubBeat - diff;
            var x = centerX - stepWidth*(diff/(double) step);

            // TODO: handle cross-section

            // to the left...
            while (x >= 0 && beat >= 0)
            {
                dc.DrawLine(subBeat == 0 ? BeatLinePen : SubBeatLinePen, new Point(x, 0), new Point(x, h));

                subBeat -= step;
                if (subBeat < 0)
                {
                    subBeat += 48;
                    --beat;
                }

                x -= stepWidth;
            }

            // to the right...
            beat = status.CurrentBeat;
            subBeat = status.CurrentSubBeat + (step - diff);
            x = centerX + stepWidth*((step - diff)/(double) step);
            while (x <= w)
            {
                if (subBeat >= 48)
                {
                    subBeat -= 48;
                    ++beat;
                }

                dc.DrawLine(subBeat == 0 ? BeatLinePen : SubBeatLinePen, new Point(x, 0), new Point(x, h));

                subBeat += step;
                x += stepWidth;
            }

            dc.DrawLine(CurrentBeatLinePen, new Point(centerX, 0), new Point(centerX, h));

            _renderHandle.Set();
        }
    }
}
