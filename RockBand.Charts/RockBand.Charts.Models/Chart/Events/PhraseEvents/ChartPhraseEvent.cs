using RockBand.Charts.Models.Chart.Phrases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace RockBand.Charts.Models.Chart.Events.PhraseEvents {
    public abstract class ChartPhraseEvent {
        protected static int typeCounter = 0;
        public ChartPhraseNote PhraseNote { get; set; }
        public long Time { get; set; }
        public long Duration { get; set; }
        public long End { get { return Time + Duration;  } }
        public int TypeInstance { get; protected set; } = 0;
        protected Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        protected ChartPhraseEvent(long time, long duration) {
            Time = time;
            Duration = duration;
        }

        public T GetMetadata<T>(string key) {
            object metadata = Metadata.GetValueOrDefault(key);
            return (T)metadata;
        }

        public void SetMetadata<T>(string key, T data) {
            this.Metadata.Add(key, data);
        }
    }

    public class OverdrivePhraseEvent : ChartPhraseEvent {
        public OverdrivePhraseEvent(long time, long duration) : base(time, duration) {

            PhraseNote = ChartPhraseNote.OVERDRIVE;
            TypeInstance = ++typeCounter;
        }

        public static void ResetCounter() {
            typeCounter = 0;
        }
    }

    public class SoloPhraseEvent : ChartPhraseEvent {
        public int NoteCount { get; set; } = 0;

        public SoloPhraseEvent(long time, long duration, int noteCount) : base(time, duration) {
            PhraseNote = ChartPhraseNote.SOLO;
            TypeInstance = ++typeCounter;
        }

        public void SetNoteCount(int noteCount) { this.NoteCount = noteCount;  }

        public static void ResetCounter() {
            typeCounter = 0;
        }
    }
}
