using System;
using System.Collections.Generic;
using System.Linq;

namespace motto_cgss_core.Model
{
    public class Beatmap
    {
        public Beatmap(BeatmapInfo info, string[] lines)
        {
            var isNotes = false;
            Info = info;
        
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
                    }
                }
                else
                {
                    var note = Note.ParseLine(line, this);
                
                    if (note != null && !_notesMap.ContainsKey(note.Id))
                    {
                        _notes.Add(note);
                        _notesMap[note.Id] = note;
                    }
                }
            }

            _notes = _notes.OrderBy(n => n.Time).ThenBy(n => n.Id).ToList();

            for (int i = 0; i < _notes.Count; ++i)
            {
                _notes[i].Index = i;
                _notes[i].PostInitialize();
            }

            NotesLoaded = true;
        }

        public BeatmapInfo Info { get; private set; }
        public int NumberOfButtons { get; private set; }
        public int Offset { get; private set; }
        public string Author { get; private set; }
        public string DifficultyName { get; private set; }

        public bool NotesLoaded { get; private set; }

        private List<Note> _notes = new List<Note>();

        public List<Note> Notes => _notes;

        private Dictionary<int, Note> _notesMap = new Dictionary<int, Note>();

        public Dictionary<int, Note> NotesMap => _notesMap;

        public void ResetNotes()
        {

        }
    }
}
