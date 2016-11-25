using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace motto_conv
{
    public static class Helpers
    {
        public static Tuple<int, int> TimeToBeats(double time, double bpm)
        {
            var beat = (int)(time * bpm / 60000);
            var subBeat = (int)Math.Round((time * bpm / 60000 - beat) * 48);

            beat += subBeat / 48;
            subBeat %= 48;

            return new Tuple<int, int>(beat, subBeat);
        }
    }
}
