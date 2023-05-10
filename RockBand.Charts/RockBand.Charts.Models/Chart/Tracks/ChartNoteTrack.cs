using RockBand.Charts.Models.Chart.Events.PhraseEvents;
using RockBand.Charts.Models.Chart.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models.Chart.Tracks {
    public class ChartNoteTrack {
        public long Duration { get; set; }
        public List<ChartNote> Notes { get; set; } = new List<ChartNote>();

        public ChartNoteTrack(long duration) {
            Duration = duration;
        }

        public void AddNote(ChartNote note) {
            this.Notes.Add(note);
        }
    }
}
