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

        public void TimeToBeats(double time, out int beat, out int subBeat)
        {
            time -= StartTime;

            // beat = (int)(time * Bpm / 60);
            subBeat = (int)(time * Bpm / 60 * 48);

            beat = subBeat / 48;
            subBeat %= 48;
        }
    }
}
