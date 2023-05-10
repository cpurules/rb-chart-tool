using RockBand.Charts.Models.Chart.Notes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.ChartGen {
    public class DrawingConfig {
        public int LastTsNum { get; set; } = 0;
        public int LastTsDenom { get; set; } = 0;
        public double LastBPM { get; set; } = 0;

        public SustainLeftover? Leftover { get; set; } = null;
        public double Overwhammy { get; set; } = 0;
    }

    public class SustainLeftover {
        public ChartNote Note { get; set; }
        public long RemainingDuration { get; set; }
    }

    public class Overwhammy {

    }
}
