using System.Collections.Generic;
using motto_cgss_core.Model;

namespace motto_cgss_core
{
    public static class CurrentGame
    {
        #region Frame Data

        public static int Time { get; set; }
        public static int SeToPlay { get; set; }
        public static bool AutoPlay { get; set; }
        public static List<ButtonState> ButtonStates { get; set; }
        public static List<bool> ButtonHandled { get; set; }

        #endregion

        #region Game Data

        public static double ApproachTime { get; set; }
        public static int NumberOfButtons { get; set; }
        public static ISceneController Scene { get; set; }
        public static double SpeedFactor { get; set; }
        public static int NotesCount { get; set; }
        public static int NoteDelay { get; set; }

        public static int EarliestTime { get; set; }
        public static int MissTime { get; set; }

        #endregion
    }
}
