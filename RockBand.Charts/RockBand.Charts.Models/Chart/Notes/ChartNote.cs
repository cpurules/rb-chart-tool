using RockBand.Charts.Models.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models.Chart.Notes {
    public class ChartNote {
        public int Index { get; set; }
        public long Time { get; set; }
        public long Duration { get; set; }
        public bool Hopo { get; set; } = false;
        public List<ChartNoteButton> Buttons { get; set; } = new List<ChartNoteButton>();
        protected Dictionary<string, object> Metadata = new Dictionary<string, object>();
        public long EffectiveDuration { get { return Duration >= (1.5 * GameConstants.SUSTAIN_THRESHOLD) ? Duration : 0; }  }



        public ChartNote(int index, long time, long duration, List<ChartNoteButton> buttons) {
            Index = index;
            Time = time;
            Duration = duration;
            Buttons = buttons;
        }

        public ChartNote(int index, long time, long duration, ChartNoteButton button)
            : this(index, time, duration, new List<ChartNoteButton> { button }) { }


        public T GetMetadata<T>(string key) {
            object data = Metadata.GetValueOrDefault(key);
            return (T)data;
        }

        public void SetMetadata<T>(string key, T data) {
            if (!HasMetadata(key))
                this.Metadata.Add(key, data);
            this.Metadata[key] = data;
        }

        public bool HasMetadata(string key) {
            return this.Metadata.ContainsKey(key);
        }

        public int GetIndex() { return this.Index;  }

        public void AddButton(ChartNoteButton button) {
            Buttons.Add(button);
        }

        public List<ChartNoteButton> GetButtons() {
            return Buttons;
        }

        public int GetButtonCount() { return Buttons.Count; }

        public long GetDuration() {
            return Duration;
        }

        public long GetEnd() {
            return GetTime() + GetDuration();
        }

        public long GetTime() {
            return Time;
        }

        public bool IsHopo() {
            return Hopo;
        }

        public void SetHopo(bool hopo) { this.Hopo = hopo;  }

        public string ToString() {
            return String.Join("", Buttons.Select(x => x.ToString()));
        }
    }
}
