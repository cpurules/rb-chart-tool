using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models {
    public enum GameMidiTrackType {
        EVENTS = 0,
        GUITAR = 1,
        BASS = 3
    }

    public static class GameMidiTrackTypeExtensions {
        public static int TrackNumber(this GameMidiTrackType track) {
            return (int) track;
        }
    }
}
