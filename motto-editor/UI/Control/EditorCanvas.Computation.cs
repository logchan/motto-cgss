using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using motto_cgss_core;
using motto_cgss_core.Model;

namespace motto_editor.UI.Control
{
    internal partial class EditorCanvas
    {
        private readonly EventWaitHandle _renderHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private int _noteHead = 0;
        private int _noteTail = -1;
        private int _lastTime = -1;

        internal void Compute()
        {
            var bm = EditorStatus.Current.EditingMap;

            var time = CurrentGame.Time = EditorStatus.Current.CurrentTime;

            bool headUpdated = false;

            int start = _noteHead;
            int end = -1;

            if (time < _lastTime)
            {
                for (int i = 0; i < _noteHead; ++i)
                {
                    var note = bm.Notes[i];

                    note.FrameComputed = false;
                    note.ComputeNote();
                    if (note.Status != NoteStatus.Done)
                    {
                        start = i;
                        Debug.WriteLine($"Start = {start}, _noteHead = {_noteHead}");
                        break;
                    }
                }
            }

            for (int i = start; i < bm.Notes.Count; ++i)
            {
                var note = bm.Notes[i];

                note.ComputeNote();
                end = i;

                if (note.Status == NoteStatus.NotShown)
                {
                    if (end < 0)
                        end = i - 1;
                    if (i >= _noteTail)
                    {
                        break;
                    }
                }

                if (!headUpdated && note.Status != NoteStatus.Done)
                {
                    headUpdated = true;
                    _noteHead = i;
                }
            }

            for (int i = start; i <= _noteTail; ++i)
            {
                bm.Notes[i].DrawNote();
            }

            for (int i = start; i <= _noteTail; ++i)
            {
                bm.Notes[i].FrameComputed = false;
            }

            _noteTail = end >= 0 ? end : bm.Notes.Count - 1;
            _lastTime = time;

        }

        internal void ComputeAndRender()
        {
            Compute();

            Dispatcher.Invoke(InvalidateVisual);
            _renderHandle.WaitOne();
        }
    }
}
