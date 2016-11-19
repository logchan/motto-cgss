using System;
using motto_cgss_core.Utility;

namespace motto_cgss_core.Model
{
    public class HoldNote : Note
    {
        #region Information

        private int _endId;
        private int _endTime;
        private Note _endSync;
        private int _duration;

        #endregion

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

        public int EndBeat { get; set; }

        public int EndSubBeat { get; set; }

        public int EndTime => _endTime;

        public int EndId => _endId;

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

        protected override bool ShallRemoveGroupLine()
        {
            return _endNoteHit;
        }

        protected override int TextureId => Constants.HoldTexture;

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
                _scene.SetLineOnPath(_trailHandle, _startPosition, _touchPosition, trailS, _t);

                if (_endNoteShown)
                {
                    if (_endHandle == 0)
                        _endHandle = _scene.CreateNote(TextureId, Index);
                    _scene.SetNote(_endHandle, _startPosition, _touchPosition, _endT);
                }
            }
            else if (Status == NoteStatus.Done)
            {
                Destroy(ref _endHandle);
                Destroy(ref _trailHandle);
            }
        }

        protected override int GetObjectHandleForGroup()
        {
            return _endHandle;
        }

        public override void SetSyncedNote(int time, Note note)
        {
            if (time == _endTime)
            {
                _endSync = note;
            }
            else
            {
                base.SetSyncedNote(time, note);
            }
        }

        protected override void DrawSyncLine()
        {
            if (_prevEndTimeDiff >= 0 && _endSync != null && _endNoteShown && _endSync.Status == NoteStatus.Shown)
            {
                if (_endLineHandle == 0)
                    _endLineHandle = _scene.CreateLine();

                _scene.SetLineBetweenNotes(_endLineHandle, _endHandle, _endSync.GetSyncedNoteHandle(_endTime));
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

        public override bool PostInitialize()
        {
            base.PostInitialize();

            if (_endId == 0)
            {
                // setup sync line for end
                // here's the logic:
                // 1. current head -- next head -> done in base, set next (base) to sync this
                // 2. current tail -- next head -> done here, set next (base) to sync this
                // 3. current tail -- next tail -> done here, set next (tail) to sync this
                for (int i = Index + 1; i < _beatmap.Notes.Count; ++i)
                {
                    var note = _beatmap.Notes[i];
                    if (note.Time > _endTime)
                        break;

                    if (note.IsSyncTime(_endTime))
                    {
                        note.SetSyncedNote(_endTime, this);
                        break;
                    }
                }

                return true;
            }

            if (!_beatmap.NotesMap.ContainsKey(_endId))
                return false;

            return true;
        }

        protected override void HandleButton(int timeDiff)
        {
            var state = CurrentGame.ButtonStates[_touchPosition];
            CurrentGame.ButtonHandled[_touchPosition] = true;

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
                    _scene.SetButtonHit(_touchPosition);
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
                    if (timeDiff + _duration < CurrentGame.MissTime)
                    {
                        // release in time
                        NoteHit = true;
                        _hitTime = CurrentGame.Time;
                        CurrentGame.SeToPlay |= HitsoundId;
                        _scene.SetButtonHit(_touchPosition);

                        if (_endId == 0)
                            _scene.AddNoteResult(Id, timeDiff + _duration);

                        if (state != ButtonState.None)
                            CurrentGame.ButtonHandled[_touchPosition] = false;
                    }
                    else
                    {
                        // release too early
                        _scene.AddNoteResult(Id, Int32.MinValue);
                    }

                    _doneByInput = true;
                    Status = NoteStatus.Done;
                }
                else if (timeDiff+_duration < -CurrentGame.MissTime)
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

            int endTimeDiff = _endTime - CurrentGame.Time;
            if (endTimeDiff < CurrentGame.ApproachTime)
            {
                _endT = MathHelper.Clamp(1 - endTimeDiff / CurrentGame.ApproachTime, 0, 1);

                if (_endId == 0)
                {
                    _endNoteShown = true;

                    if (CurrentGame.AutoPlay)
                    {
                        if (endTimeDiff <= 0 && _prevEndTimeDiff > 0)
                        {
                            CurrentGame.SeToPlay |= HitsoundId;
                            _scene.SetButtonHit(_touchPosition);
                            _scene.AddNoteResult(Id, endTimeDiff);
                        }
                    }
                }
                else
                {
                    _endNoteHit = _beatmap.NotesMap[_endId].NoteHit;
                }

                if (CurrentGame.AutoPlay && endTimeDiff <= -CurrentGame.NoteDelay)
                {
                    Status = NoteStatus.Done;
                }
            }

            _prevEndTimeDiff = endTimeDiff;
            FrameComputed = true;
        }

        public override bool IsSyncTime(int time)
        {
            return (_endId == 0 && time == _endTime) || base.IsSyncTime(time);
        }

        public override int GetSyncedNoteHandle(int time)
        {
            if (_endId == 0 && time == _endTime)
                return _endHandle;
            return base.GetSyncedNoteHandle(time);
        }

        protected override bool SelfInitialize(string[] arr, int offset)
        {
            if (arr.Length - offset < 3)
                return false;

            int beat;
            int subBeat;
            if (!Int32.TryParse(arr[offset], out beat) ||
                !Int32.TryParse(arr[offset + 1], out subBeat) ||
                !Int32.TryParse(arr[offset + 2], out _endId))
                return false;

            EndBeat = beat;
            EndSubBeat = subBeat;
            _endTime = _beatmap.Info.BeatToTime(beat, subBeat);
            _duration = _endTime - _time;

            return true;
        }

        public override string ToString()
        {
            return $"{base.ToString()},{EndBeat},{EndSubBeat},{_endId}";
        }
    }
}