using System;

namespace motto_cgss_core.Model
{
    public class HitNote : Note
    {
        public HitNote(Beatmap beatmap) : base(beatmap)
        {
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

            var expected = ButtonState.Hit;
            if (state == expected)
            {
                NoteHit = true;
                _hitTime = CurrentGame.Time;
                CurrentGame.SeToPlay |= HitsoundId;
                _scene.SetButtonHit(_touchPosition);
                _scene.AddNoteResult(Id, timeDiff);

                Status = NoteStatus.Done;
            }
            else
            {
                if (timeDiff < -CurrentGame.MissTime)
                {
                    _scene.AddNoteResult(Id, Int32.MinValue);
                    Status = NoteStatus.Done;
                }
            }
        }
    }
}

