using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RockBand.Charts.Models.Game;

namespace RockBand.Charts.Models.Chart.Events.TimeEvents
{
    public class ChartTempoEvent {
        public long Time { get; set; }
        public GameTempo Tempo { get; set; }

        public ChartTempoEvent(long time, GameTempo tempo) {
            Time = time;
            Tempo = tempo;
        }
    }
}
