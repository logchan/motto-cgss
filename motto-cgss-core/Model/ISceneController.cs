namespace motto_cgss_core.Model
{
    public interface ISceneController
    {
        int CreateNote(int textureId, int noteId);
        int CreateLine();
        void Destroy(int handle);

        void SetNote(int handle, int from, int to, double t);

        void SetLineBetweenNotes(int line, int note1, int note2);
        void SetLineBetweenNoteAndStart(int line, int note, int start);
        void SetLineOnPath(int line, int from, int to, double s, double t);

        void SetButtonHit(int pos);
        void AddNoteResult(int noteId, int timeDiff);

        void AddLog(string msg);
    }
}
