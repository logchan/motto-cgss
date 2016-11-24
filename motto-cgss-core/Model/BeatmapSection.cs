namespace motto_cgss_core.Model
{
    public class BeatmapSection
    {
        public double StartTime { get; set; }
        public double Bpm { get; set; }

        public double BeatToTime(int beat, int subBeat)
        {
            return StartTime + (beat + subBeat / 48.0) * 60.0 / Bpm;
        }
    }
}
