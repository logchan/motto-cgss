namespace motto_cgss_core.Model
{
    public class BeatmapEvent
    {
        private readonly Beatmap _beatmap;

        public Beatmap Beatmap => _beatmap;
        public int SectionId { get; set; }
        public BeatmapSection Section => _beatmap.Sections[SectionId];

        public int Beat { get; set; }
        public int SubBeat { get; set; }
        public int Time => (int)(1000 * Section.BeatToTime(Beat, SubBeat));

        public int EventId { get; set; }
        public string EventArgs { get; set; }

        public BeatmapEvent(Beatmap beatmap, int sectionId)
        {
            _beatmap = beatmap;
            SectionId = sectionId;
        }
    }
}
