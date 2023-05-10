using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models.Game
{
    public class GameTimeSignature
    {
        public int Top { get; set; }
        public int Bottom { get; set; }

        public GameTimeSignature(int top, int bottom)
        {
            Top = top;
            Bottom = bottom;
        }
    }
}
