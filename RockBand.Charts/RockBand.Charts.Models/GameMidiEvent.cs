using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models {
    public abstract class GameMidiEvent {
        public long DeltaTime { get; set; }

        protected GameMidiEvent(long deltaTime) {
            DeltaTime = deltaTime;
        }
    }

    public class GameTimeSignatureEvent : GameMidiEvent {
        public int Numerator { get; set; }
        public int Denominator { get; set; }
        public int ClocksPerClick { get; set; }
        public int ThirtySecondNotesPerBeat { get; set; }

        public GameTimeSignatureEvent(long deltaTime, int numerator, int denominator) : base(deltaTime) {
            Numerator = numerator;
            Denominator = denominator;
        }
    }

    public class GameTempoEvent : GameMidiEvent {
        public long MicrosecondsPerQuarterNote { get; set; }
        public double BPM { get { return 60000000.0 / MicrosecondsPerQuarterNote; } }

        public GameTempoEvent(long deltaTime, long microsecondsPerQuarterNote) : base(deltaTime) {
            MicrosecondsPerQuarterNote = microsecondsPerQuarterNote;
        }
    }
}
