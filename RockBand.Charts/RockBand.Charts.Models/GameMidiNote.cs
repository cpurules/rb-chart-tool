
namespace RockBand.Charts.Models {
    public class GameMidiNote {
        public int Count { get { return Notes.Count; } }
        public List<GameNoteType> Notes { get; set; }
        public bool IsHopo { get; set; } = false;
        public long NoteOn { get; set; }
        public long NoteOff { get; set; }
        public long Duration { get { return NoteOff - NoteOn;  } }

        public GameMidiMeasure Measure { get; set; }

        public GameMidiNote(List<GameNoteType> notes, long noteOn, long noteOff) {
            Notes = notes;
            NoteOn = noteOn;
            NoteOff = noteOff;
        }

        public void AttachTo(GameMidiMeasure m) {
            this.Measure = m;
        }
    }
}