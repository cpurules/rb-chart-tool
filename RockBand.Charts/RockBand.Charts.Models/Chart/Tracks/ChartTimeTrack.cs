using RockBand.Charts.Models.Chart.Events.TimeEvents;
using RockBand.Charts.Models.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models.Chart.Tracks
{
    public class ChartTimeTrack {
        public List<ChartTimeSignatureEvent> TimeSignatures { get; set; } = new List<ChartTimeSignatureEvent>();
        public List<ChartTempoEvent> Tempos { get; set; } = new List<ChartTempoEvent>();

        public void AddTimeSignature(long time, GameTimeSignature ts) {
            TimeSignatures.Add(new ChartTimeSignatureEvent(time, ts));
        }

        public void AddTempo(long time, GameTempo tempo) {
            Tempos.Add(new ChartTempoEvent(time, tempo));
        }
    }
}
