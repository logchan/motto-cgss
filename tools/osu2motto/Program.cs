using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osuElements.Beatmaps;

namespace osu2motto
{
    class Program
    {
        static string Prompt(string msg)
        {
            Console.Write(msg);
            return Console.ReadLine();
        }

        static void Main(string[] args)
        {
            var bmfile = args.Length > 0 ? args[0] : Prompt("Beatmap file: ");
            var beatmap = new Beatmap(bmfile);
            beatmap.ReadFile();

            beatmap.Bpm = Double.Parse(Prompt("Bpm: "));

            Console.WriteLine($"Loaded osu! beatmap with {beatmap.HitObjects.Count} objects. Bpm: {beatmap.Bpm}. Converting...");
            var cgss = Osu2Cgss(beatmap);

            var outfile = Prompt("Output file: ");
            File.WriteAllText(outfile, cgss);

            Prompt("Press Enter to exit.");
        }

        public static string Osu2Cgss(Beatmap beatmap)
        {
            var sb = new StringBuilder();

            // TODO: implement...

            return sb.ToString();
        }
    }
}
