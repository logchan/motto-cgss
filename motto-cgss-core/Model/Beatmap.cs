using System;
using System.Collections.Generic;
using System.Linq;

namespace motto_cgss_core.Model
{
    public class Beatmap
    {
        public Beatmap(BeatmapInfo info)
        {
            Info = info;
        }

        // Information
        public BeatmapInfo Info { get; }
        public int NumberOfButtons { get; set; }
        public int Offset { get; set; }
        public string Author { get; set; }
        public string DifficultyName { get; set; }
        
        // Section and notes
        public List<BeatmapSection> Sections { get; } = new List<BeatmapSection>();
        public List<Note> Notes { get; } = new List<Note>();
        public Dictionary<int, Note> NotesMap { get; } = new Dictionary<int, Note>();

        public void Parse(string[] lines)
        {
            Sections.Clear();
            Notes.Clear();
            NotesMap.Clear();

            var isNotes = false;

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                    continue;

                if (!isNotes)
                {
                    if (line == "NOTES")
                    {
                        isNotes = true;
                        continue;
                    }

                    var arr = line.Split(':');
                    if (arr.Length < 2)
                        continue;

                    switch (arr[0].ToLower())
                    {
                        case "buttons":
                            int btn;
                            if (Int32.TryParse(arr[1], out btn))
                                NumberOfButtons = btn;
                            break;
                        case "offset":
                            int offset;
                            if (Int32.TryParse(arr[1], out offset))
                                Offset = offset;
                            break;
                        case "author":
                            Author = arr[1];
                            break;
                        case "difficulty":
                            DifficultyName = arr[1];
                            break;
                        case "sections":
                            var strs = arr[1].Split('|');
                            foreach (var sectionStr in strs)
                            {
                                var sectionArr = sectionStr.Split(',');
                                if (sectionArr.Length < 2)
                                    continue;

                                double bpm, start;
                                if (Double.TryParse(sectionArr[0], out start) &&
                                    Double.TryParse(sectionArr[1], out bpm))
                                {
                                    Sections.Add(new BeatmapSection
                                    {
                                        Bpm = bpm,
                                        StartTime = start,
                                    });
                                }
                            }
                            break;
                    }
                }
                else
                {
                    var note = Note.ParseLine(line, this);

                    if (note != null && !NotesMap.ContainsKey(note.Id))
                    {
                        Notes.Add(note);
                        NotesMap[note.Id] = note;
                    }
                }
            }

            Notes.Sort((a, b) =>
            {
                var diff = a.Time - b.Time;
                return diff != 0 ? diff : a.Id - b.Id;
            });

            for (int i = 0; i < Notes.Count; ++i)
            {
                Notes[i].Index = i;
                Notes[i].PostInitialize();
            }
        }
    }
}
