using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace motto_conv
{
    public static class Cgss2Motto
    {
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

        public static string Convert(List<string> lines, double bpm)
        {
            var sb = new StringBuilder();
            var notes = new List<Note>();

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
                    var beats = Helpers.TimeToBeats(time * 1000, bpm);
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

            return sb.ToString();
        }
    }
}
