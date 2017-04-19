using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace motto_editor.UI.Control
{
    internal partial class EditorCanvas
    {
        private class Note
        {
            public Note(int texture, int id, double x, double y)
            {
                Texture = texture;
                Id = id;
                X = x;
                Y = y;
            }

            public Note(int texture, int id) : this(texture, id, 0, 0) { }
            public int Texture { get; set; }
            public int Id { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Radius { get; set; }
        }

        private class NLine
        {
            public NLine(double x1, double y1, double x2, double y2)
            {
                X1 = x1;
                Y1 = y1;
                X2 = x2;
                Y2 = y2;
            }

            public NLine() : this(0, 0, 0, 0) { }

            public double X1 { get; set; }
            public double Y1 { get; set; }
            public double X2 { get; set; }
            public double Y2 { get; set; }
            public bool IsOnPath { get; set; }
            public int PathFrom { get; set; }
            public int PathTo { get; set; }
            public double PathStart { get; set; }
            public double PathEnd { get; set; }
        }

        private Note GetNote(int handle)
        {
            return _notes[handle / 2];
        }

        private NLine GetLine(int handle)
        {
            return _lines[(handle - 1) / 2];
        }

        // note handles are odd, line handles are even
        private int _nextNoteHandle = 1;
        private List<Note> _notes = new List<Note>();
        private Queue<int> _notePool = new Queue<int>();

        private int _nextLineHandle = 2;
        private List<NLine> _lines = new List<NLine>();
        private Queue<int> _linePool = new Queue<int>();
    }
}
