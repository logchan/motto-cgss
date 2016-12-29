using System;
using motto_cgss_core.Utility;

namespace motto_cgss_core.Model
{
    public abstract class Note
    {
        protected Beatmap _beatmap;

        #region Computation and Drawing

        protected ISceneController _scene;
        protected int _prevTimeDiff;
        protected int _hitTime;
        protected double _t;
        protected bool _computationDrawn;
        protected int _noteHandle;
        protected int _lineHandle;
        protected int _gpLineHandle;

        #endregion

        protected Note(Beatmap beatmap)
        {
            _beatmap = beatmap;
            Status = NoteStatus.NotShown;
        }

        // information
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int Beat { get; set; }
        public int SubBeat { get; set; }
        public int Index { get; set; }
        public int StartPosition { get; set; }
        public int TouchPosition { get; set; }
        public int SectionId { get; set; }
        public BeatmapSection Section => _beatmap.Sections[SectionId];
        public int Time => (int)(1000.0 * + Section.BeatToTime(Beat, SubBeat));

        // relationships
        public Note GroupedNote { get; set; }
        public int GroupedNoteId { get; set; }
        public Note SyncedNote { get; set; }

        // computation
        public bool FrameComputed { get; set; }
        public bool NoteHit { get; set; }
        public NoteStatus Status { get; set; }

        // skinning
        protected virtual int TextureId => Constants.NoteTexture;
        protected virtual int HitsoundId => Constants.HitSound;


        #region Loading

        public static Note ParseLine(string line, Beatmap bm)
        {
            var arr = line.Split(',');
            if (arr.Length < 7)
                return null;

            Note note;

            switch (arr[1].Trim())
            {
                case "0":
                    note = new HitNote(bm);
                    break;
                case "1":
                    note = new HoldNote(bm);
                    break;
                case "2":
                    note = new SwipeNote(bm);
                    break;
                default:
                    return null;
            }

            try
            {
                note.Id = Int32.Parse(arr[0]);
                note.StartPosition = Int32.Parse(arr[2]);
                note.TouchPosition = Int32.Parse(arr[3]);
                note.SectionId = Int32.Parse(arr[4]);
                note.Beat = Int32.Parse(arr[5]);
                note.SubBeat = Int32.Parse(arr[6]);
                note.GroupedNoteId = Int32.Parse(arr[7]);
            }
            catch (Exception)
            {
                return null;
            }

            if (!note.SelfInitialize(arr, 8))
                return null;

            return note;
        }

        public virtual bool PostInitialize()
        {
            // update the sync with next note
            if (Index < _beatmap.Notes.Count - 1)
            {
                var next = _beatmap.Notes[Index + 1];
                if (next.IsSyncTime(Time))
                {
                    next.SetSyncedNote(Time, this);
                }
            }

            if (_beatmap.NotesMap.ContainsKey(GroupedNoteId))
                GroupedNote = _beatmap.NotesMap[GroupedNoteId];

            return true;
        }

        protected virtual bool SelfInitialize(string[] arr, int offset)
        {
            return true;
        }

        #endregion

        #region Sync line

        public virtual void SetSyncedNote(int time, Note note)
        {
            SyncedNote = note;
        }

        public virtual int GetSyncedNoteHandle(int time)
        {
            return _noteHandle;
        }

        public virtual bool IsSyncTime(int time)
        {
            return time == Time;
        }

        protected virtual void DrawSyncLine()
        {
            if (SyncedNote == null)
                return;

            if (!NoteHit && SyncedNote.Status == NoteStatus.Shown && Status == NoteStatus.Shown)
            {
                if (_lineHandle == 0)
                    _lineHandle = _scene.CreateLine();
                _scene.SetLineBetweenNotes(_lineHandle, _noteHandle, SyncedNote.GetSyncedNoteHandle(Time));
            }
            else if (Status == NoteStatus.Done || NoteHit)
            {
                Destroy(ref _lineHandle);
            }
        }

        #endregion

        #region Group line

        protected virtual int GetObjectHandleForGroup()
        {
            return _noteHandle;
        }

        protected virtual bool ShallRemoveGroupLine()
        {
            return NoteHit;
        }

        protected void DrawGroupLine()
        {
            if (GroupedNote == null)
                return;

            if (Status == NoteStatus.Shown && !ShallRemoveGroupLine())
            {
                var gpHandle = GetObjectHandleForGroup();
                if (gpHandle == 0)
                    return;

                GroupedNote.DrawSelf();

                if (_gpLineHandle == 0)
                    _gpLineHandle = _scene.CreateLine();

                if (GroupedNote.Status == NoteStatus.Shown)
                {
                    _scene.SetLineBetweenNotes(_gpLineHandle, gpHandle, GroupedNote.GetObjectHandleForGroup());
                }
                else
                {
                    _scene.SetLineBetweenNoteAndStart(_gpLineHandle, gpHandle, GroupedNote.StartPosition);
                }
            }
            else if (Status == NoteStatus.Done || ShallRemoveGroupLine())
            {
                Destroy(ref _gpLineHandle);
            }
        }

        #endregion

        #region Notetype-specific behaviors

        protected abstract void HandleButton(int timeDiff);

        public virtual void ComputeSelf()
        {
            var timeDiff = Time - CurrentGame.Time;
            if (timeDiff < CurrentGame.ApproachTime)
            {
                Status = NoteStatus.Shown;

                if (CurrentGame.AutoPlay)
                {
                    if (timeDiff <= 0)
                    {
                        if (_prevTimeDiff > 0)
                        {
                            NoteHit = true;
                            _hitTime = CurrentGame.Time;
                            CurrentGame.SeToPlay |= HitsoundId;
                            _scene.SetButtonHit(TouchPosition);
                            _scene.AddNoteResult(Id, timeDiff);
                        }

                        if (timeDiff < -CurrentGame.NoteDelay)
                            Status = NoteStatus.Done;
                    }
                    else
                    {
                        NoteHit = false;
                    }
                }
                else if (!CurrentGame.ButtonHandled[TouchPosition])
                {
                    HandleButton(timeDiff);
                }

                _t = MathHelper.Clamp(1 - timeDiff/CurrentGame.ApproachTime, 0, 1);
            }
            else
            {
                _t = 0;
                Status = NoteStatus.NotShown;
            }

            _prevTimeDiff = timeDiff;
            FrameComputed = true;
            _computationDrawn = false;
        }

        protected virtual void DrawSelf()
        {
            if (_computationDrawn)
                return;

            if (Status == NoteStatus.Shown)
            {
                if (_noteHandle == 0)
                    _noteHandle = _scene.CreateNote(TextureId, Id);
                _scene.SetNote(_noteHandle, StartPosition, TouchPosition, _t);
            }
            else
            {
                Destroy(ref _noteHandle);
                Destroy(ref _lineHandle);
                Destroy(ref _gpLineHandle);
            }

            _computationDrawn = true;
        }

        #endregion

        #region Gameplay control

        public virtual void GameInit()
        {
            _scene = CurrentGame.Scene;
            Status = NoteStatus.NotShown;
            FrameComputed = false;
            NoteHit = false;
            _computationDrawn = false;
            _prevTimeDiff = 0;
            _t = 0;
            _noteHandle = 0;
            _lineHandle = 0;
            _gpLineHandle = 0;
        }

        protected void Destroy(ref int handle)
        {
            if (handle == 0)
                return;

            _scene.Destroy(handle);
            handle = 0;
        }

        public void ComputeNote()
        {
            if (!FrameComputed)
                ComputeSelf();
        }

        public void DrawNote()
        {
            DrawSelf();

            DrawSyncLine();

            DrawGroupLine();
        }

        #endregion

        public override string ToString()
        {
            return $"{Id},{TypeId},{StartPosition},{TouchPosition},{SectionId},{Beat},{SubBeat},{GroupedNoteId}";
        }
    }
}
