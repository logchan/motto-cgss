using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osuElements.Beatmaps;

namespace motto_conv
{
    public static class Osu2Motto
    {
        private static Random _r;

        public static string Convert(string filename, List<string> lines, double bpm)
        {
            _r = null;

            int noteId = 1;
            var sb = new StringBuilder();
            var bm = new Beatmap(filename);
            bm.Bpm = bpm;
            bm.ReadFile();

            var inHitObjects = false;
            foreach (var line in lines)
            {
                if (!inHitObjects)
                {
                    if (line == "[HitObjects]")
                    {
                        inHitObjects = true;
                    }
                    continue;
                }

                var arr = line.Split('|');
                var basicarr = arr[0].Split(',');

                // seed random with bm-specific values
                if (_r == null)
                {
                    // seed = x ^ y ^ time
                    _r = new Random(
                        Int32.Parse(basicarr[0]) ^
                        Int32.Parse(basicarr[1]) ^
                        Int32.Parse(basicarr[2]));
                }

                var type = Int32.Parse(basicarr[3]);

                if ((type & 2) > 0)
                {
                    ParseSlider(bm, arr, basicarr, sb, bpm, ref noteId);
                }
                else if ((type & 8) > 0)
                {
                    // spinner
                    var start = Helpers.TimeToBeats(Int32.Parse(basicarr[2])/1000.0, bpm);
                    var end = Helpers.TimeToBeats(Int32.Parse(basicarr[5])/1000.0, bpm);
                    sb.AppendLine($"{noteId},1,2,2,0,{start.Item1},{start.Item2},0,{end.Item1},{end.Item2},0");
                    ++noteId;
                }
                else
                {
                    // note
                    // TODO: handle in groups
                    var time = Helpers.TimeToBeats(Int32.Parse(basicarr[2]) / 1000.0, bpm);
                    var startPos = Int32.Parse(basicarr[1])/102;
                    var endPos = Int32.Parse(basicarr[2])/102;
                    sb.AppendLine($"{noteId},0,{startPos},{endPos},0,{time.Item1},{time.Item2},0");
                    ++noteId;
                }
            }

            return sb.ToString();
        }

        private static void ParseSlider(Beatmap bm, string[] arr, string[] basicarr, StringBuilder sb, double bpm, ref int noteId)
        {
            var length = 0;
            var repeat = 0;

            for (int i = 1; i < arr.Length; ++i)
            {
                if (arr[i].IndexOf(',') > 0)
                {
                    var lastPointArr = arr[i].Split(',');
                    repeat = Int32.Parse(lastPointArr[1]) - 1;
                    break;
                }
            }

            var time = Int32.Parse(basicarr[2]);
            var hitobj = bm.HitObjects.FirstOrDefault(obj => obj.StartTime == time) as Slider;
            if (hitobj == null)
                return;

            length = hitobj.Duration;
            Console.WriteLine(length);

            var start = Helpers.TimeToBeats(time / 1000.0, bpm);
            time += length;
            var end = Helpers.TimeToBeats(time / 1000.0,bpm);

            bool left = true;
            for (int i = 0; i <= repeat; ++i)
            {
                int pos;
                pos = left ? _r.Next(0, 2) : _r.Next(3, 5);
                left = !left;

                sb.AppendLine($"{noteId},1,{pos},{pos},0,{start.Item1},{start.Item2},0,{end.Item1},{end.Item2},0");
                ++noteId;

                start = end;
                time += length;
                end = Helpers.TimeToBeats(time / 1000.0, bpm);
            }
        }
    }
}
