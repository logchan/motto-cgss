using System;

namespace motto_cgss_core.Model
{
    public class SwipeNote : Note
    {
        private int _direction;

        public SwipeNote(Beatmap beatmap) : base(beatmap)
        {
        }

        public int Direction => _direction;

        protected override int TextureId => Constants.SwipeTextureLeft + _direction;

        protected override int HitsoundId => Constants.SwipeSound;

        protected override bool SelfInitialize(string[] arr, int offset)
        {
            if (arr.Length - offset < 1)
                return false;

            if (!Int32.TryParse(arr[offset], out _direction))
                return false;

            _direction = _direction == 0 ? 0 : 1;

            return true;
        }

        public override string ToString()
        {
            return $"{base.ToString()},{_direction}";
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

            var expected = _direction == 0 ? ButtonState.Left : ButtonState.Right;
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