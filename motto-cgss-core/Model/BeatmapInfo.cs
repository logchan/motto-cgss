using System;

namespace motto_cgss_core.Model
{
    public class BeatmapInfo
    {
        private double _beatFactor;

        public BeatmapInfo(string[] lines, string fileName, string path)
        {
            if (lines.Length < 1)
                throw new Exception("Empty beatmap info file.");

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                    continue;

                var arr = line.Split(':');
                if (arr.Length < 2)
                    continue;

                switch (arr[0].ToLower())
                {
                    case "title":
                        Title = arr[1].Trim();
                        break;
                    case "artist":
                        Artist = arr[1].Trim();
                        break;
                    case "bpm":
                        int bpm;
                        if (Int32.TryParse(arr[1].Trim(), out bpm))
                            Bpm = bpm;
                        break;
                }
            }

            if (Bpm == 0)
                throw new Exception("Invalid bpm value.");

            _beatFactor = 60.0 / Bpm * 1000;
            Title = Title ?? fileName;
            Artist = Artist ?? "";
            FileName = fileName;
            Path = path;
        }
    
        public string Title { get; }
        public string Artist { get; }
        public int Bpm { get; }

        public string Path { get; private set; }
        public string FileName { get; private set; }

        public int BeatToTime(int beat, int subBeat)
        {
            return (int)((beat + subBeat / 48.0) * _beatFactor);
        }
    }
}
