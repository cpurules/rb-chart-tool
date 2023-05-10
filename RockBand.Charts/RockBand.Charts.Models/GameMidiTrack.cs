using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models {
    public class GameMidiTrack {
        public List<GameMidiNote> Notes { get; set; } = new List<GameMidiNote>();
        public List<GameMidiEventNote> Events { get; set; } = new List<GameMidiEventNote>();
    }
}
