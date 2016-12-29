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
        private static readonly Pen SelectedNoteOverlayPen = new Pen(Brushes.White, 1);
        private static readonly Pen LinePen = new Pen(Brushes.Gray, 4);
        private static readonly Pen TrailPen = new Pen(Brushes.Gray, 2);

        private static readonly Pen SelectedPathPen = new Pen(Brushes.Gray, 2)
        {
            DashStyle = DashStyles.Dash
        };

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, ActualWidth, ActualHeight));

            if (EditorStatus.Current.EditingMap == null)
                return;

            for (int i = 0; i < EditorStatus.Current.EditingMap.NumberOfButtons; ++i)
            {
                dc.DrawEllipse(null, ButtonPen, new Point(_endX[i], _endY), _noteRadius, _noteRadius);
            }

            DrawLines(dc);
            DrawNotes(dc);

            _renderHandle.Set();
        }

        private void DrawPath(DrawingContext dc, int from, int to, double startT, double endT, Pen pen)
        {
            var start = (int)(startT * PathPointNumber);
            if (start > PathPointNumber)
                start = PathPointNumber;

            var end = (int)(endT * PathPointNumber);
            if (end > PathPointNumber)
                end = PathPointNumber;

            if (end <= start)
                return;

            var idx = (from * EditorStatus.Current.EditingMap.NumberOfButtons + to) * 2;
            var listL = _pathPoints[idx];
            var listR = _pathPoints[idx + 1];

            var lstr = new StringBuilder();
            var rstr = new StringBuilder();

            lstr.Append($"M {listL[start].Item1},{listL[start].Item2} L ");
            rstr.Append($"M {listR[start].Item1},{listR[start].Item2} L ");
            for (int i = start + 1; i <= end; ++i)
            {
                lstr.Append($"{listL[i].Item1},{listL[i].Item2} ");
                rstr.Append($"{listR[i].Item1},{listR[i].Item2} ");
            }
            dc.DrawGeometry(null, pen, Geometry.Parse(lstr.ToString()));
            dc.DrawGeometry(null, pen, Geometry.Parse(rstr.ToString()));
        }

        private void DrawLines(DrawingContext dc)
        {
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
                    DrawPath(dc, line.PathFrom, line.PathTo, line.PathStart, line.PathEnd, TrailPen);
                }
            }
        }

        private void DrawNotes(DrawingContext dc)
        {
            var selectedId = EditorStatus.Current.SelectedNote?.Id ?? -1;
            Note selectedNote = null;

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
                        var p2 = new Point(note.X + r * 0.732, note.Y + r);
                        var p3 = new Point(note.X + r * 0.732, note.Y - r);

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

                if (selectedId == note.Id)
                {
                    selectedNote = note;
                }
            }

            if (selectedNote != null)
            {
                var mapNote = EditorStatus.Current.SelectedNote;

                DrawPath(dc, mapNote.StartPosition, mapNote.TouchPosition, 0.0, 1.0, SelectedPathPen);
                dc.DrawEllipse(null, SelectedNoteOverlayPen, new Point(selectedNote.X, selectedNote.Y), selectedNote.Radius + 1, selectedNote.Radius + 1);
            }
        }
    }
}
