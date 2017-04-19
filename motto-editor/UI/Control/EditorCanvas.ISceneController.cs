using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using motto_cgss_core.Model;

namespace motto_editor.UI.Control
{
    internal partial class EditorCanvas : ISceneController
    {
        public void AddLog(string msg)
        {
            // TODO
        }

        public void AddNoteResult(int noteId, int timeDiff)
        {
            // ignore
        }

        public int CreateLine()
        {
            int handle;
            var line = new NLine();
            if (_linePool.Count > 0)
            {
                handle = _linePool.Dequeue();
                _lines[(handle - 1) / 2] = line;
            }
            else
            {
                handle = _nextLineHandle;
                _nextLineHandle += 2;
                _lines.Add(line);
            }

            return handle;
        }

        public int CreateNote(int textureId, int noteId)
        {
            int handle;
            var note = new Note(textureId, noteId);
            if (_notePool.Count > 0)
            {
                handle = _notePool.Dequeue();
                _notes[handle / 2] = note;
            }
            else
            {
                handle = _nextNoteHandle;
                _nextNoteHandle += 2;
                _notes.Add(note);
            }

            return handle;
        }

        public void Destroy(int handle)
        {
            if (handle < 1)
                return;

            var index = (handle - 1)/2;
            var list = handle % 2 == 1 ? (IList)_notes : _lines;
            var queue = handle%2 == 1 ? _notePool : _linePool;
            if (index >= list.Count || list[index] == null)
                return;

            list[index] = null;
            queue.Enqueue(handle);
        }

        public void SetButtonHit(int pos)
        {
            // ignore
        }

        public void SetLineBetweenNoteAndStart(int lineHandle, int noteHandle, int start)
        {
            var line = GetLine(lineHandle);
            var note = GetNote(noteHandle);

            line.X1 = note.X;
            line.Y1 = note.Y;
            line.X2 = _startX[start];
            line.Y2 = _startY;
        }

        public void SetLineBetweenNotes(int lineHandle, int noteHandle1, int noteHandle2)
        {
            var line = GetLine(lineHandle);
            var n1 = GetNote(noteHandle1);
            var n2 = GetNote(noteHandle2);

            line.X1 = n1.X;
            line.Y1 = n1.Y;
            line.X2 = n2.X;
            line.Y2 = n2.Y;
        }

        public void SetLineOnPath(int handle, int from, int to, double s, double t)
        {
            var line = GetLine(handle);

            line.IsOnPath = true;
            line.PathStart = s;
            line.PathEnd = t;
            line.PathFrom = from;
            line.PathTo = to;
        }

        public void SetNote(int handle, int from, int to, double t)
        {
            var note = GetNote(handle);

            double x, y;
            ComputeNotePosition(t, from, to, out x, out y);
            note.X = x;
            note.Y = y;
            note.Radius = ComputeScale(t)*_noteRadius;
        }
    }
}
