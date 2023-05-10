
namespace RockBand.Charts.Models {
    public class GameMidiEventNote {
        public GameNoteType Note { get; set; }
        public long NoteOn { get; set; }
        public long NoteOff { get; set; }
        public long Duration { get { return NoteOff - NoteOn;  } }

        public GameMidiEventNote(GameNoteType note, long noteOn, long noteOff) {
            Note = note;
            NoteOn = noteOn;
            NoteOff = noteOff;
        }
    }
}