using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models {
    public class GameMidiMeasure {
        public int Number { get; set; }
        public long Time { get; set; }
        public long Duration { get; set; }
        public int TsNum { get; set; }
        public int TsDenom { get; set; }
        public List<GameMidiNote> Notes { get; set; } = new List<GameMidiNote>();
        public List<GameTempoEvent> Tempos { get; set; } = new List<GameTempoEvent>();

        public GameMidiMeasure(int number, long time, long duration, int tsNum, int tsDenom) {
            Number = number;
            Time = time;
            Duration = duration;
            TsNum = tsNum;
            TsDenom = tsDenom;
        }

        public void AddNote(GameMidiNote note) {
            this.Notes.Add(note);
            note.AttachTo(this);
        }

        public void AddTempo(GameTempoEvent tempo) {
            this.Tempos.Add(tempo);
        }
    }

    public static class GameMidiMeasureListExtensions {
        public static List<GameMidiMeasure> BareClone(this List<GameMidiMeasure> list) {
            List<GameMidiMeasure> copy = new List<GameMidiMeasure>();
            foreach (var measure in list) {
                GameMidiMeasure measureClone = new GameMidiMeasure(measure.Number, measure.Time, measure.Duration, measure.TsNum, measure.TsDenom);
                measureClone.Tempos = measure.Tempos;
                copy.Add(measureClone);
            }
            return copy;
        }
    }
}
