using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RockBand.Charts.Models.Game;

namespace RockBand.Charts.Models.Chart.Events.TimeEvents
{
    public class ChartTimeSignatureEvent {
        public long Time { get; set; }
        public GameTimeSignature TimeSignature { get; set; }

        public ChartTimeSignatureEvent(long time, GameTimeSignature timeSignature) {
            Time = time;
            TimeSignature = timeSignature;
        }

        public ChartTimeSignatureEvent(long time, int top, int bottom) {
            Time = time;
            TimeSignature = new GameTimeSignature(top, bottom);
        }
    }
}
