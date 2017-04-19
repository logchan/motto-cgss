using System;

namespace motto_cgss_core.Model
{
    public class BeatmapInfo
    {
        public void Parse(string[] lines)
        {
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
                }
            }
        }

        public string Title { get; set; } = "New beatmap";
        public string Artist { get; set; } = "Unknown";

        public string Path { get; set; }
        public string FileName { get; set; }
    }
}
