using RockBand.Charts.Models.Chart.Events.TimeEvents;
using RockBand.Charts.Models.Chart.Notes;
using RockBand.Charts.Models.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models.Chart.Measures
{
    public class ChartMeasure {
        public int Number { get; set; }
        public long Time { get; set; }
        public long Duration { get; set; }
        public long End { get { return Time + Duration;  } }
        public GameTimeSignature TimeSignature { get; set; }
        public List<ChartNote> Notes { get; set; } = new List<ChartNote>();
        public List<ChartTempoEvent> Tempos { get; set; } = new List<ChartTempoEvent>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        public ChartMeasure(int number, long time, long duration, GameTimeSignature timeSignature) {
            Number = number;
            Time = time;
            Duration = duration;
            TimeSignature = timeSignature;
        }

        public T GetMetadata<T>(string key) {
            object data = Metadata.GetValueOrDefault(key);
            return (T)data;
        }

        public void SetMetadata<T>(string key, T data) {
            if (!this.Metadata.ContainsKey(key))
                this.Metadata.Add(key, data);
            Metadata[key] = data;
        }

        public bool HasMetadata(string key) { return this.Metadata.ContainsKey(key);  }
    }
}
