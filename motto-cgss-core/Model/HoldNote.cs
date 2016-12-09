using System;
using motto_cgss_core.Utility;

namespace motto_cgss_core.Model
{
    public class HoldNote : Note
    {

        #region Computation and Drawing

        private bool _endNoteShown;
        private double _endT;
        private int _prevEndTimeDiff;
        private bool _endNoteHit;
        private int _endHandle;
        private int _endLineHandle;
        private int _trailHandle;
        private bool _holding;
        private bool _doneByInput;

        #endregion

        public HoldNote(Beatmap beatmap) : base(beatmap)
        {
        }

        // information
        public int EndBeat { get; set; }
        public int EndSubBeat { get; set; }
        public int EndTime => (int)(1000 * Section.BeatToTime(EndBeat, EndSubBeat));
        public int Duration => EndTime - Time;

        // relationships
        public int EndId { get; set; }
        public Note EndSync { get; set; }

        // 
        protected override int TextureId => Constants.HoldTexture;

        #region Loading

        protected override bool SelfInitialize(string[] arr, int offset)
        {
            if (arr.Length - offset < 3)
                return false;

            try
            {
                EndBeat = Int32.Parse(arr[offset]);
                EndSubBeat = Int32.Parse(arr[offset + 1]);
                EndId = Int32.Parse(arr[offset + 2]);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public override bool PostInitialize()
        {
            base.PostInitialize();

            if (EndId == 0)
            {
                // setup sync line for end
                // here's the logic:
                // 1. current head -- next head -> done in base, set next (base) to sync this
                // 2. current tail -- next head -> done here, set next (base) to sync this
                // 3. current tail -- next tail -> done here, set next (tail) to sync this
                for (int i = Index + 1; i < _beatmap.Notes.Count; ++i)
                {
                    var note = _beatmap.Notes[i];
                    if (note.Time > EndTime)
                        break;

                    if (note.IsSyncTime(EndTime))
                    {
                        note.SetSyncedNote(EndTime, this);
                        break;
                    }
                }

                return true;
            }

            if (!_beatmap.NotesMap.ContainsKey(EndId))
                return false;

            return true;
        }

        #endregion

        #region Sync line

        public override void SetSyncedNote(int time, Note note)
        {
            if (time == EndTime)
            {
                EndSync = note;
            }
            else
            {
                base.SetSyncedNote(time, note);
            }
        }

        public override bool IsSyncTime(int time)
        {
            return (EndId == 0 && time == EndTime) || base.IsSyncTime(time);
        }

        public override int GetSyncedNoteHandle(int time)
        {
            if (EndId == 0 && time == EndTime)
                return _endHandle;
            return base.GetSyncedNoteHandle(time);
        }

        protected override void DrawSyncLine()
        {
            if (_prevEndTimeDiff >= 0 && EndSync != null && _endNoteShown && EndSync.Status == NoteStatus.Shown)
            {
                if (_endLineHandle == 0)
                    _endLineHandle = _scene.CreateLine();

                _scene.SetLineBetweenNotes(_endLineHandle, _endHandle, EndSync.GetSyncedNoteHandle(EndTime));
            }

            if (Status == NoteStatus.Done || _endNoteHit)
            {
                if (_endLineHandle != 0)
                {
                    Destroy(ref _endLineHandle);
                }
            }

            base.DrawSyncLine();
        }

        #endregion

        #region Group Line

        protected override bool ShallRemoveGroupLine()
        {
            return _endNoteHit;
        }

        protected override int GetObjectHandleForGroup()
        {
            return _endHandle;
        }

        #endregion

        #region Notetype-specific behaviors

        protected override void DrawSelf()
        {
            base.DrawSelf();

            if (Status == NoteStatus.Shown)
            {
                if (_trailHandle == 0)
                {
                    _trailHandle = _scene.CreateLine();
                }

                var trailS = _prevEndTimeDiff >= CurrentGame.ApproachTime ? 0 : 1 - _prevEndTimeDiff / CurrentGame.ApproachTime;
                _scene.SetLineOnPath(_trailHandle, StartPosition, TouchPosition, trailS, _t);

                if (_endNoteShown)
                {
                    if (_endHandle == 0)
                        _endHandle = _scene.CreateNote(TextureId, Index);
                    _scene.SetNote(_endHandle, StartPosition, TouchPosition, _endT);
                }
            }
            else
            {
                Destroy(ref _endHandle);
                Destroy(ref _trailHandle);
            }
        }

        protected override void HandleButton(int timeDiff)
        {
            var state = CurrentGame.ButtonStates[TouchPosition];
            CurrentGame.ButtonHandled[TouchPosition] = true;

            if (timeDiff > CurrentGame.EarliestTime)
            {
                // too early to handle, ignore
                return;
            }

            if (!_holding)
            {
                if (state == ButtonState.Hit)
                {
                    NoteHit = true;
                    _hitTime = CurrentGame.Time;
                    CurrentGame.SeToPlay |= HitsoundId;
                    _scene.SetButtonHit(TouchPosition);
                    _scene.AddNoteResult(Id, timeDiff);
                    _holding = true;
                }
                else if (timeDiff < -CurrentGame.MissTime)
                {
                    _doneByInput = true;
                    _scene.AddNoteResult(Id, Int32.MinValue);
                    Status = NoteStatus.Done;
                }
            }
            else
            {
                if (state != ButtonState.Hold)
                {
                    if (timeDiff + Duration < CurrentGame.MissTime)
                    {
                        // release in time
                        NoteHit = true;
                        _hitTime = CurrentGame.Time;

                        if (EndId == 0)
                        {
                            CurrentGame.SeToPlay |= HitsoundId;
                            _scene.SetButtonHit(TouchPosition);
                            _scene.AddNoteResult(Id, timeDiff + Duration);
                        }
                            
                        if (state != ButtonState.None)
                            CurrentGame.ButtonHandled[TouchPosition] = false;
                    }
                    else
                    {
                        // release too early
                        _scene.AddNoteResult(Id, Int32.MinValue);
                    }

                    _doneByInput = true;
                    Status = NoteStatus.Done;
                }
                else if (timeDiff + Duration < -CurrentGame.MissTime)
                {
                    // release too late
                    _doneByInput = true;
                    _scene.AddNoteResult(Id, Int32.MinValue);
                    Status = NoteStatus.Done;
                }
            }
        }

        public override void ComputeSelf()
        {
            base.ComputeSelf();

            if (Status == NoteStatus.Done && !_doneByInput)
            {
                Status = NoteStatus.Shown;
            }

            int endTimeDiff = EndTime - CurrentGame.Time;
            if (endTimeDiff < CurrentGame.ApproachTime)
            {
                _endT = MathHelper.Clamp(1 - endTimeDiff/CurrentGame.ApproachTime, 0, 1);

                if (EndId == 0)
                {
                    _endNoteShown = true;

                    if (CurrentGame.AutoPlay)
                    {
                        if (endTimeDiff <= 0 && _prevEndTimeDiff > 0)
                        {
                            if (EndId == 0)
                            {
                                CurrentGame.SeToPlay |= HitsoundId;
                                _scene.SetButtonHit(TouchPosition);
                            }
                            _scene.AddNoteResult(Id, endTimeDiff);
                        }
                    }
                }
                else
                {
                    _endNoteHit = _beatmap.NotesMap[EndId].NoteHit;
                }

                if (CurrentGame.AutoPlay && endTimeDiff <= -CurrentGame.NoteDelay)
                {
                    Status = NoteStatus.Done;
                }
            }
            else
            {
                _endT = 0;
                _endNoteShown = false;
            }

            _prevEndTimeDiff = endTimeDiff;
            FrameComputed = true;
        }

        #endregion

        #region Gameplay control

        public override void GameInit()
        {
            base.GameInit();
            _endNoteShown = false;
            _endNoteHit = false;
            _endT = 0;
            _prevEndTimeDiff = 0;
            _endHandle = 0;
            _endLineHandle = 0;
            _trailHandle = 0;
            _holding = false;
            _doneByInput = false;
        }

        #endregion

        public override string ToString()
        {
            return $"{base.ToString()},{EndBeat},{EndSubBeat},{EndId}";
        }
    }
}