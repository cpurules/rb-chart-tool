using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models {
    public enum GameNoteType {
        G = 96,
        R = 97,
        Y = 98,
        B = 99,
        O = 100,
        OVERDRIVE = 116,
        SOLO = 103
    }

    public static class GameNoteTypeExtensions {
        public static Color GetColor(this GameNoteType type) {
            switch (type) {
                case GameNoteType.G: return Color.Green;
                case GameNoteType.R: return Color.Red;
                case GameNoteType.Y: return Color.Yellow;
                case GameNoteType.B: return Color.Blue;
                case GameNoteType.O: return Color.Orange;
                case GameNoteType.OVERDRIVE: return Color.FromArgb(192, 255, 192);
                case GameNoteType.SOLO: return Color.FromArgb(224, 224, 255);
                default: throw new ArgumentException("Unknown note: " + type.ToString());
            }
        }

        public static bool IsPlayable(this GameNoteType type) {
            return (type >= GameNoteType.G && type <= GameNoteType.O);
        }
    }
}
