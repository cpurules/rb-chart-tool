using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models.Game
{
    public class GameTempo
    {
        public long usPerQuarterNote { get; set; }
        public double BPM { get { return 60000000.0 / usPerQuarterNote; } }

        public GameTempo(long usPerQuarterNote)
        {
            this.usPerQuarterNote = usPerQuarterNote;
        }
    }
}
