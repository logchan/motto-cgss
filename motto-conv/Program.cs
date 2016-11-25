using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace motto_conv
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
            Console.WriteLine("Note: invalid input causes the program to crash :P");

            bool beatmapOnly = false;

            if (args.Length > 0)
            {
                if (args[0] == "--bm")
                {
                    beatmapOnly = true;
                }
            }

            var dir = Prompt("Output DIR: ");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!beatmapOnly)
            {
                var title = Prompt("Title: ");
                var artist = Prompt("Artist: ");

                File.WriteAllLines(Path.Combine(dir, "info.txt"), new[]
                {
                    $"title:{title}",
                    $"artist:{artist}"
                });

                Console.WriteLine("Success: info.txt created. Now create [filename].diff.txt files.");
            }

            var bpm = Double.Parse(Prompt("BPM: "));

            Console.WriteLine("Note: if you use same difficulty filenames, former files will be overwritten.");
            Console.WriteLine("Note: use empty difficulty name to indicate end.");
            Console.WriteLine("Note: if difficulty filename is empty, the program uses difficulty name lower case.");
            Console.WriteLine("Note: if author is empty, it will be set to CGSS officials");

            while (true)
            {
                var diff = Prompt("Difficulty name: ");
                if (String.IsNullOrEmpty(diff))
                {
                    break;
                }

                var filename = Prompt("Difficulty filename: ");
                if (String.IsNullOrEmpty(filename))
                {
                    filename = diff.ToLower();
                }

                var author = Prompt("Author: ");
                if (String.IsNullOrEmpty(author))
                {
                    author = "CGSS officials";
                }

                var inputFile = Prompt("Input: ");
                if (inputFile.StartsWith("\"") && inputFile.EndsWith("\""))
                    inputFile = inputFile.Substring(1, inputFile.Length - 2);

                var lines = File.ReadAllLines(inputFile).ToList();

                var sb = new StringBuilder();
                sb.AppendLine("offset:0");
                sb.AppendLine("buttons:5");
                sb.AppendLine($"difficulty:{diff}");
                sb.AppendLine($"author:{author}");
                sb.AppendLine($"sections:0,{bpm}");
                sb.AppendLine("NOTES");

                if (inputFile.ToLower().EndsWith(".osu"))
                {
                    sb.AppendLine(Osu2Motto.Convert(inputFile, lines, bpm));
                }
                else
                {
                    sb.AppendLine(Cgss2Motto.Convert(lines, bpm));
                }

                File.WriteAllText(Path.Combine(dir, $"{filename}.diff.txt"), sb.ToString());
                Console.WriteLine("----SUCCESS----");
                Console.WriteLine();
            }
        }
    }
}
