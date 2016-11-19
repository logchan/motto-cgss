using System;
using motto_cgss_core.Utility;

namespace motto_cgss_core.Model
{
    public abstract class Note
    {
        #region Information

        protected Beatmap _beatmap;
        protected int _id;
        protected int _startPosition;
        protected int _time;
        protected int _touchPosition;
        protected Note _syncedNote;
        protected int _groupedNoteId;
        protected Note _groupedNote;

        #endregion

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

        public int Id => _id;

        public Note GroupedNote => _groupedNote;

        public int TypeId { get; set; }

        public int Beat { get; set; }

        public int SubBeat { get; set; }

        public int Index { get; set; }

        public bool FrameComputed { get; set; }

        public int StartPosition => _startPosition;
        public int TouchPosition => _touchPosition;
        public Note SyncedNote => _syncedNote;
        public int Time => _time;

        public bool NoteHit { get; set; }

        public NoteStatus Status { get; protected set; }

        protected virtual int TextureId => Constants.NoteTexture;

        protected virtual int HitsoundId => Constants.HitSound;

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

            int beat;
            int subBeat;

            if (!Int32.TryParse(arr[0], out note._id) ||
                !Int32.TryParse(arr[2], out note._startPosition) ||
                note._startPosition >= bm.NumberOfButtons ||
                !Int32.TryParse(arr[3], out note._touchPosition) ||
                note._touchPosition >= bm.NumberOfButtons ||
                !Int32.TryParse(arr[4], out beat) ||
                !Int32.TryParse(arr[5], out subBeat) ||
                subBeat >= 48 ||
                !Int32.TryParse(arr[6], out note._groupedNoteId))
                return null;

            note.Beat = beat;
            note.SubBeat = subBeat;
            note._time = bm.Info.BeatToTime(beat, subBeat);

            if (!note.SelfInitialize(arr, 7))
                return null;

            return note;
        }

        public virtual bool PostInitialize()
        {
            // update the sync with next note
            if (Index < _beatmap.Notes.Count - 1)
            {
                var next = _beatmap.Notes[Index + 1];
                if (next.IsSyncTime(_time))
                {
                    next.SetSyncedNote(_time, this);
                }
            }

            if (_beatmap.NotesMap.ContainsKey(_groupedNoteId))
                _groupedNote = _beatmap.NotesMap[_groupedNoteId];

            return true;
        }

        protected virtual bool SelfInitialize(string[] arr, int offset)
        {
            return true;
        }

        #endregion

        #region Sync Line

        public virtual void SetSyncedNote(int time, Note note)
        {
            _syncedNote = note;
        }

        public virtual int GetSyncedNoteHandle(int time)
        {
            return _noteHandle;
        }

        public virtual bool IsSyncTime(int time)
        {
            return time == _time;
        }

        protected virtual void DrawSyncLine()
        {
            if (_syncedNote == null)
                return;

            if (!NoteHit && _syncedNote.Status == NoteStatus.Shown && Status == NoteStatus.Shown)
            {
                if (_lineHandle == 0)
                    _lineHandle = _scene.CreateLine();
                _scene.SetLineBetweenNotes(_lineHandle, _noteHandle, _syncedNote.GetSyncedNoteHandle(_time));
            }
            else if (Status == NoteStatus.Done || NoteHit)
            {
                Destroy(ref _lineHandle);
            }
        }

        #endregion

        #region Group Line

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
            if (_groupedNote == null)
                return;

            if (Status == NoteStatus.Shown && !ShallRemoveGroupLine())
            {
                var gpHandle = GetObjectHandleForGroup();
                if (gpHandle == 0)
                    return;

                _groupedNote.DrawSelf();

                if (_gpLineHandle == 0)
                    _gpLineHandle = _scene.CreateLine();

                if (_groupedNote.Status == NoteStatus.Shown)
                {
                    _scene.SetLineBetweenNotes(_gpLineHandle, gpHandle, _groupedNote.GetObjectHandleForGroup());
                }
                else
                {
                    _scene.SetLineBetweenNoteAndStart(_gpLineHandle, gpHandle, _groupedNote._startPosition);
                }
            }
            else if (Status == NoteStatus.Done || ShallRemoveGroupLine())
            {
                Destroy(ref _gpLineHandle);
            }
        }

        #endregion

        protected abstract void HandleButton(int timeDiff);

        public virtual void ComputeSelf()
        {
            var timeDiff = _time - CurrentGame.Time;
            if (timeDiff < CurrentGame.ApproachTime)
            {
                Status = NoteStatus.Shown;

                if (CurrentGame.AutoPlay)
                {
                    if (timeDiff <= 0 && _prevTimeDiff > 0)
                    {
                        NoteHit = true;
                        _hitTime = CurrentGame.Time;
                        CurrentGame.SeToPlay |= HitsoundId;
                        _scene.SetButtonHit(_touchPosition);
                        _scene.AddNoteResult(Id, timeDiff);
                    }
                }
                else if(!CurrentGame.ButtonHandled[_touchPosition])
                {
                    HandleButton(timeDiff);
                }

                if (Status == NoteStatus.Shown)
                {
                    _t = MathHelper.Clamp(1 - timeDiff / CurrentGame.ApproachTime, 0, 1);
                }

                if (NoteHit)
                {
                    Status = NoteStatus.Done;
                }
            }
            _prevTimeDiff = timeDiff;
            FrameComputed = true;
            _computationDrawn = false;
        }

        protected virtual void DrawSelf()
        {
            if (_computationDrawn)
                return;

            if (Status == NoteStatus.Done)
            {
                Destroy(ref _noteHandle);
            }
            else if (Status == NoteStatus.Shown)
            {
                if (_noteHandle == 0)
                    _noteHandle = _scene.CreateNote(TextureId, Index);
                _scene.SetNote(_noteHandle, _startPosition, _touchPosition, _t);
            }
            _computationDrawn = true;
        }

        public void ComputeNote()
        {
            if (Status == NoteStatus.Done)
                return;

            if (!FrameComputed)
                ComputeSelf();
        }

        public void DrawNote()
        {
            DrawSelf();

            DrawSyncLine();

            DrawGroupLine();
        }

        public override string ToString()
        {
            return $"{Id},{TypeId},{_startPosition},{_touchPosition},{Beat},{SubBeat},{_groupedNoteId}";
        }
    }
}
