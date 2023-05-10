using RockBand.Charts.Models.Chart.Events.PhraseEvents;
using RockBand.Charts.Models.Chart.Phrases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models.Chart.Tracks {
    public class ChartPhraseTrack {
        public List<ChartPhraseEvent> PhraseEvents { get; set; } = new List<ChartPhraseEvent>();

        public void AddPhraseEvent(ChartPhraseEvent pe) {
            this.PhraseEvents.Add(pe);
        }
    }
}