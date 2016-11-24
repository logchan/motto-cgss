﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cgss2motto
{
    class Program
    {
        static string Prompt(string msg)
        {
            Console.Write(msg);
            return Console.ReadLine();
        }

        private class Note
        {
            public int OriginalId { get; set; }
            public int OriginalNextId { get; set; } = -1;

            public int Id { get; set; }
            public int GroupNextId { get; set; } = 0;
            public int Type { get; set; }
            public int StartPos { get; set; }
            public int EndPos { get; set; }
            public int Dir { get; set; }
            public int Group { get; set; }
            public int Beat { get; set; }
            public int SubBeat { get; set; }
            public double Time { get; set; }
            public int NextIndex { get; set; } = -1;
            public int EndBeat { get; set; }
            public int EndSubBeat { get; set; }
            public bool Skipped { get; set; } = false;
        }

        static Tuple<int, int> TimeToBeats(double time, double bpm)
        {
            var beat = (int)(time * bpm / 60000);
            var subBeat = (int)Math.Round((time * bpm / 60000 - beat) * 48);

            beat += subBeat / 48;
            subBeat %= 48;

            return new Tuple<int, int>(beat, subBeat);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Note: invalid input causes the program to crash :P");

            var dir = Prompt("Output DIR: ");
            var title = Prompt("Title: ");
            var artist = Prompt("Artist: ");
            var bpm = Double.Parse(Prompt("BPM: "));

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllLines(Path.Combine(dir, "info.txt"), new[]
            {
                $"title:{title}",
                $"artist:{artist}"
            });

            Console.WriteLine("Success: info.txt created. Now create [filename].diff.txt files.");
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

                var csvfile = Prompt("CSV: ");

                var lines = File.ReadAllLines(csvfile);
                var sb = new StringBuilder();
                var notes = new List<Note>();

                sb.AppendLine("offset:0");
                sb.AppendLine("buttons:5");
                sb.AppendLine($"difficulty:{diff}");
                sb.AppendLine($"author:{author}");
                sb.AppendLine($"sections:0,{bpm}");
                sb.AppendLine("NOTES");

                foreach (var line in lines)
                {
                    var arr = line.Split(',');
                    if (arr.Length != 8 || (arr[2] != "1" && arr[2] != "2"))
                        continue;

                    try
                    {
                        var note = new Note
                        {
                            Type = arr[2] == "2" ? 1 : (arr[5] == "0" ? 0 : 2),
                            StartPos = Int32.Parse(arr[3]) - 1,
                            EndPos = Int32.Parse(arr[4]) - 1,
                            Dir = Int32.Parse(arr[5]) - 1,
                            Group = Int32.Parse(arr[7]),
                            OriginalId = Int32.Parse(arr[0])
                        };

                        var time = Double.Parse(arr[1]);
                        var beats = TimeToBeats(time * 1000, bpm);
                        note.Beat = beats.Item1;
                        note.SubBeat = beats.Item2;
                        note.Time = time;

                        notes.Add(note);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                // first pass: set tails and nexts
                for (int i = 0; i < notes.Count; ++i)
                {
                    var note = notes[i];
                    if (note.Skipped)
                        continue;

                    if (note.Type == 1) // hold note
                    {
                        // find tail
                        for (int j = i + 1; j < notes.Count; ++j)
                        {
                            var other = notes[j];
                            if (other.EndPos == note.EndPos) // this has to be the tail
                            {
                                note.EndBeat = other.Beat;
                                note.EndSubBeat = other.SubBeat;

                                if (other.Type == 2)
                                {
                                    note.OriginalNextId = other.OriginalId;
                                    note.NextIndex = j;
                                    other.StartPos = note.StartPos;
                                }
                                else
                                {
                                    note.OriginalNextId = other.OriginalId;
                                    note.Group = other.Group;
                                    other.Skipped = true;
                                }
                                break;
                            }
                        }
                    }
                    else if (note.Type == 2 && note.Group != 0) // swipe note
                    {
                        for (int j = i + 1; j < notes.Count; ++j)
                        {
                            var other = notes[j];
                            if (other.Group == note.Group)
                            {
                                note.NextIndex = j;
                                break;
                            }
                        }
                    }
                }

                // second pass: assign Id
                int id = 1;
                for (int i = 0; i < notes.Count; ++i)
                {
                    var note = notes[i];
                    if (note.Skipped)
                        continue;

                    note.Id = id;
                    ++id;
                }

                // third pass: translate
                for (int i = 0; i < notes.Count; ++i)
                {
                    var note = notes[i];
                    if (note.Skipped)
                        continue;

                    // for debug
                    //if (note.OriginalNextId < 0)
                    //    sb.AppendLine($"#{note.OriginalId}");
                    //else
                    //    sb.AppendLine($"#{note.OriginalId}, {note.OriginalNextId}");

                    // find group
                    if (note.Group > 0)
                    {
                        for (int j = i + 1; j < notes.Count; ++j)
                        {
                            var other = notes[j];
                            if (other.Skipped)
                                continue;

                            if (other.Group == note.Group)
                            {
                                note.GroupNextId = other.Id;
                                break;
                            }
                        }
                    }

                    var common = $"{note.Id},{note.Type},{note.StartPos},{note.EndPos},0,{note.Beat},{note.SubBeat},{note.GroupNextId}";
                    switch (note.Type)
                    {
                        case 0:
                            sb.AppendLine($"{common}");
                            break;
                        case 1:
                            sb.Append($"{common},{note.EndBeat},{note.EndSubBeat},");
                            if (note.NextIndex < 0)
                                sb.AppendLine("0");
                            else
                                sb.AppendLine($"{notes[note.NextIndex].Id}");
                            break;
                        case 2:
                            sb.AppendLine($"{common},{note.Dir}");
                            break;
                    }
                }

                File.WriteAllText(Path.Combine(dir, $"{filename}.diff.txt"), sb.ToString());
                Console.WriteLine("----OK!----");
            }

            Prompt("Press ENTER to exit.");
        }
    }
}
