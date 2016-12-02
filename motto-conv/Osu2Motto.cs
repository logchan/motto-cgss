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
        private static int _lastSliderEnd = -1;
        private static int _currGroupOtherPos = -1;

        public static string Convert(string filename, List<string> lines, double bpm)
        {
            _r = null;

            int noteId = 1;
            var msPerBeat = 60000 / bpm;
            var sb = new StringBuilder();
            var bm = new Beatmap(filename);
            
            bm.Bpm = bpm;
            bm.ReadFile();

            var bmm = new BeatmapManager(bm);
            bmm.SliderCalculations();

            var lastType = 0;
            var lastTime = 0;
            var lastPos = -1;

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

                var hitobjs = bmm.GetHitObjects();
                var type = Int32.Parse(basicarr[3]);
                var bmTime = Int32.Parse(basicarr[2]);
                var time = Helpers.TimeToBeats(bmTime, bpm);
                
                if ((type & 2) > 0)
                {
                    var slider = hitobjs.FirstOrDefault(o => o.StartTime == bmTime) as Slider;
                    ParseSlider(slider, arr, basicarr, sb, bpm, ref noteId);
                    lastType = 1;
                }
                else if ((type & 8) > 0)
                {
                    // spinner
                    
                    var end = Helpers.TimeToBeats(Int32.Parse(basicarr[5]), bpm);
                    sb.AppendLine($"{noteId},1,2,2,0,{time.Item1},{time.Item2},0,{end.Item1},{end.Item2},0");
                    ++noteId;
                    _lastSliderEnd = -1;
                    lastType = 2;
                }
                else
                {
                    // note
                    // TODO: handle in groups
                    var startPos = Int32.Parse(basicarr[0])/103;
                    var endPos = Int32.Parse(basicarr[1])/77;
                    var actualEndPos = endPos;
                    if (lastType == 3 && (bmTime - lastTime) < msPerBeat/3)
                    {
                        if (lastPos == endPos)
                        {
                            if (_currGroupOtherPos < 0 || _currGroupOtherPos == endPos)
                            {
                                _currGroupOtherPos = _r.Next(0, 4);
                                if (_currGroupOtherPos >= endPos)
                                    ++_currGroupOtherPos;
                            }
                            actualEndPos = _currGroupOtherPos;
                        }
                    }
                    else
                    {
                        _currGroupOtherPos = -1;
                    }

                    sb.AppendLine($"{noteId},0,{startPos},{actualEndPos},0,{time.Item1},{time.Item2},0");
                    ++noteId;

                    lastPos = actualEndPos;
                    _lastSliderEnd = -1;
                    lastType = 3;
                }

                lastTime = bmTime;
            }

            return sb.ToString();
        }

        private static void ParseSlider(Slider slider, string[] arr, string[] basicarr, StringBuilder sb, double bpm, ref int noteId)
        {
            var length = 0;
            var repeat = 1;

            for (int i = 1; i < arr.Length; ++i)
            {
                if (arr[i].IndexOf(',') > 0)
                {
                    var lastPointArr = arr[i].Split(',');
                    repeat = Int32.Parse(lastPointArr[1]);
                    break;
                }
            }

            var time = Int32.Parse(basicarr[2]);
            length = slider.Duration / repeat;

            var start = Helpers.TimeToBeats(time, bpm);
            time += length;
            var end = Helpers.TimeToBeats(time,bpm);

            bool left = true;
            if (repeat == 1)
            {
                var startPos = Int32.Parse(basicarr[1]) / 77;
                var endPos = Int32.Parse(basicarr[0]) / 103;
                if (_lastSliderEnd != -1 && endPos == _lastSliderEnd)
                {
                    endPos = _r.Next(0, 4);
                    if (endPos >= _lastSliderEnd)
                        ++endPos;
                }
                _lastSliderEnd = endPos;
                sb.AppendLine($"{noteId},1,{startPos},{endPos},0,{start.Item1},{start.Item2},0,{end.Item1},{end.Item2},0");
                ++noteId;
            }
            else
            {
                for (int i = 0; i < repeat; ++i)
                {
                    int pos = repeat > 1 ? (left ? _r.Next(0, 2) : _r.Next(3, 5)) : _r.Next(0, 5);
                    left = !left;

                    sb.AppendLine($"{noteId},1,{pos},{pos},0,{start.Item1},{start.Item2},0,{end.Item1},{end.Item2},0");
                    ++noteId;

                    start = end;
                    time += length;
                    end = Helpers.TimeToBeats(time, bpm);
                }
            }
        }
    }
}
