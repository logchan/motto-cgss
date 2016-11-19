using System;

namespace motto_cgss_core.Model
{
    public class BeatmapInfo
    {
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
                }
            }

            Title = Title ?? fileName;
            Artist = Artist ?? "";
            FileName = fileName;
            Path = path;
        }
    
        public string Title { get; set; }
        public string Artist { get; set; }

        public string Path { get; set; }
        public string FileName { get; set; }
    }
}
