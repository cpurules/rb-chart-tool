using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models.Chart {
    public enum ChartNoteButton {
        G = 96,
        R = 97,
        Y = 98,
        B = 99,
        O = 100
    }

    public static class ChartNoteButtonExtensions {
        public static Color GetColor(this ChartNoteButton button) {
            switch (button) {
                case ChartNoteButton.G: return Color.LimeGreen;
                case ChartNoteButton.R: return Color.Red;
                case ChartNoteButton.Y: return Color.Yellow;
                case ChartNoteButton.B: return Color.Blue;
                case ChartNoteButton.O: return Color.Orange;
                default: throw new NotImplementedException();
            }
        }
    }
}
