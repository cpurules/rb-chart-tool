using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models.Chart.Events {
    public class OverdriveActivationEvent {
        public long StartTick { get; set; }
        public long Duration { get; set; }
        public long End { get { return StartTick + Duration;  } }

        public OverdriveActivationEvent(long startTick, long duration) {
            StartTick = startTick;
            Duration = duration;
        }
    }
}
