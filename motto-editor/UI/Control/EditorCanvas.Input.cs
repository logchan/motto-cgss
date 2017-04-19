using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace motto_editor.UI.Control
{
    internal partial class EditorCanvas
    {
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var hit = false;
            var pos = e.GetPosition(this);
            foreach (var note in _notes)
            {
                if (note == null)
                    continue;

                var dist = Math.Pow(pos.X - note.X, 2) + Math.Pow(pos.Y - note.Y, 2);
                if (dist < note.Radius*note.Radius)
                {
                    hit = true;
                    EditorStatus.Current.SelectedNote = EditorStatus.Current.EditingMap.NotesMap[note.Id];
                    break;
                }
            }

            if (!hit)
            {
                EditorStatus.Current.SelectedNote = null;
            }

            InvalidateVisual();
        }
    }
}
