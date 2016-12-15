using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace motto_editor.UI.Control
{
    internal partial class EditorCanvas
    {
        private static readonly Pen ButtonPen = new Pen(Brushes.Black, 2);
        private static readonly Pen HitNotePen = new Pen(Brushes.Red, 2);
        private static readonly Pen HoldNotePen = new Pen(Brushes.DarkGoldenrod, 2);
        private static readonly Pen SwipeNotePen = new Pen(Brushes.Blue, 2);
        private static readonly Pen LinePen = new Pen(Brushes.Gray, 4);

        protected override void OnRender(DrawingContext dc)
        {
            if (EditorStatus.Current.EditingMap == null)
                return;

            for (int i = 0; i < EditorStatus.Current.EditingMap.NumberOfButtons; ++i)
            {
                dc.DrawEllipse(null, ButtonPen, new Point(_endX[i], _endY), _noteRadius, _noteRadius);
            }

            foreach (var note in _notes)
            {
                if (note == null)
                    continue;
                
                // TODO: draw texture
                var r = note.Radius;
                switch (note.Texture)
                {
                    case 0:
                        dc.DrawEllipse(null, HitNotePen, new Point(note.X, note.Y), r, r);
                        break;
                    case 1:
                        var p1 = new Point(note.X - r, note.Y);
                        var p2 = new Point(note.X + r*0.732, note.Y + r);
                        var p3 = new Point(note.X + r*0.732, note.Y - r);

                        dc.DrawLine(SwipeNotePen, p1, p2);
                        dc.DrawLine(SwipeNotePen, p2, p3);
                        dc.DrawLine(SwipeNotePen, p3, p1);
                        break;
                    case 2:
                        p1 = new Point(note.X + r, note.Y);
                        p2 = new Point(note.X - r * 0.732, note.Y + r);
                        p3 = new Point(note.X - r * 0.732, note.Y - r);

                        dc.DrawLine(SwipeNotePen, p1, p2);
                        dc.DrawLine(SwipeNotePen, p2, p3);
                        dc.DrawLine(SwipeNotePen, p3, p1);
                        break;
                    case 3:
                        dc.DrawEllipse(null, HoldNotePen, new Point(note.X, note.Y), r, r);
                        break;
                }
            }

            foreach (var line in _lines)
            {
                if (line == null)
                    continue;

                if (!line.IsOnPath)
                {
                    dc.DrawLine(LinePen, new Point(line.X1, line.Y1), new Point(line.X2, line.Y2));
                }
                else
                {
                    // oh shit
                }
            }

            _renderHandle.Set();
        }
    }
}
