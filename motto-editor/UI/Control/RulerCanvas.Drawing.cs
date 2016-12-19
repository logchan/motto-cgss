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
        private const int BeatLineThickness = 4;
        private const int SubBeatLineThickness = 2;

        private static readonly SolidColorBrush BackgroundFillBrush = Brushes.Black;
        private static readonly Pen BeatLinePen = new Pen(Brushes.White, BeatLineThickness);
        private static readonly Pen SubBeatLinePen = new Pen(Brushes.Yellow, SubBeatLineThickness);
        private static readonly Pen StartBeatLinePen = new Pen(Brushes.LightGray, BeatLineThickness);
        private static readonly Pen NegativeBeatLinePen = new Pen(Brushes.LightGray, SubBeatLineThickness);
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
            var section = bm.Sections[status.CurrentSection];
                
            var centerX = w*CurrentBeatLinePosition;

            var step = 48/BeatDivision;
            var stepWidth = BeatWidth/BeatDivision;
            var diff = status.CurrentSubBeat % step;

            var beat = status.CurrentBeat;
            var subBeat = status.CurrentSubBeat - diff;
            var x = centerX - stepWidth*(diff/(double) step);

            // TODO: handle cross-section

            // to the left...
            while (x >= 0 && section.BeatToTime(beat, subBeat) >= 0)
            {
                dc.DrawLine(beat >= 0 ? (subBeat == 0 ? BeatLinePen : SubBeatLinePen) : NegativeBeatLinePen, new Point(x, 0), new Point(x, h));

                subBeat -= step;
                if (subBeat < 0)
                {
                    subBeat += 48;
                    --beat;
                }

                x -= stepWidth;
            }

            // music start line
            int zeroBeat, zeroSubBeat;
            section.TimeToBeats(0, out zeroBeat, out zeroSubBeat);
            x = centerX - stepWidth*(status.CurrentBeat*48+status.CurrentSubBeat-zeroBeat*48-zeroSubBeat)/step;
            if (x >= 0)
            {
                dc.DrawLine(StartBeatLinePen, new Point(x, 0), new Point(x, h));
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

                dc.DrawLine(beat >= 0 ? (subBeat == 0 ? BeatLinePen : SubBeatLinePen) : NegativeBeatLinePen, new Point(x, 0), new Point(x, h));

                subBeat += step;
                x += stepWidth;
            }

            dc.DrawLine(CurrentBeatLinePen, new Point(centerX, 0), new Point(centerX, h));

            _renderHandle.Set();
        }
    }
}
