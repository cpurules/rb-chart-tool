using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models.Game
{
    public static class GameConstants
    {
        public static readonly long CHORD_THRESHOLD = 15; // How close two MIDI notes must be to be considered a chord
        public static readonly long SUSTAIN_THRESHOLD = 160; // How long a MIDI note must be to be considered a sustain
        public static readonly long HOPO_THRESHOLD = 170; // How close two MIDI notes must be to potentially be auto-HOPOed
        public static readonly int GEM_SCORE = 25; // Base score for each individual MIDI note
        public static readonly int TICKS_PER_BEAT = 12; // Number of ticks scored per beat
    }
}
