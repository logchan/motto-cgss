using System;

namespace motto_cgss_core.Model
{
    public class SwipeNote : Note
    {

        public SwipeNote(Beatmap beatmap) : base(beatmap)
        {
        }

        public int Direction { get; set; }
        protected override int TextureId => Constants.SwipeTextureLeft + Direction;
        protected override int HitsoundId => Constants.SwipeSound;

        protected override bool SelfInitialize(string[] arr, int offset)
        {
            if (arr.Length - offset < 1)
                return false;

            try
            {
                Direction = Int32.Parse(arr[offset]);
                Direction = Direction == 0 ? 0 : 1;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
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

            var expected = Direction == 0 ? ButtonState.Left : ButtonState.Right;
            if (state == expected)
            {
                NoteHit = true;
                _hitTime = CurrentGame.Time;
                CurrentGame.SeToPlay |= HitsoundId;
                _scene.SetButtonHit(TouchPosition);
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

        public override string ToString()
        {
            return $"{base.ToString()},{Direction}";
        }
    }
}