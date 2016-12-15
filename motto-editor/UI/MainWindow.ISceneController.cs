using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using motto_cgss_core.Model;

namespace motto_editor.UI
{
    public partial class MainWindow : ISceneController
    {
        public void AddLog(string msg)
        {
            Debug.WriteLine(msg);
        }

        public void AddNoteResult(int noteId, int timeDiff)
        {
            // ignore
        }

        public int CreateLine()
        {
            throw new NotImplementedException();
        }

        public int CreateNote(int textureId, int noteId)
        {
            throw new NotImplementedException();
        }

        public void Destroy(int handle)
        {
            throw new NotImplementedException();
        }

        public void SetButtonHit(int pos)
        {
            // ignore
        }

        public void SetLineBetweenNoteAndStart(int line, int note, int start)
        {
            throw new NotImplementedException();
        }

        public void SetLineBetweenNotes(int line, int note1, int note2)
        {
            throw new NotImplementedException();
        }

        public void SetLineOnPath(int line, int from, int to, double s, double t)
        {
            throw new NotImplementedException();
        }

        public void SetNote(int handle, int from, int to, double t)
        {
            throw new NotImplementedException();
        }
    }
}
