using RockBand.Charts.Models.Chart.Events;
using RockBand.Charts.Models.Chart.Events.PhraseEvents;
using RockBand.Charts.Models.Chart.Events.TimeEvents;
using RockBand.Charts.Models.Chart.Measures;
using RockBand.Charts.Models.Chart.Tracks;
using RockBand.Charts.Models.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models.Chart {
    public class Chart {
        public long Duration { get; set; }
        public long Timebase { get; set; }
        public ChartTimeTrack TimeTrack { get; private set; }
        public Dictionary<string, ChartInstrumentTrack> Tracks { get; set; } = new Dictionary<string, ChartInstrumentTrack>();

        public Chart(long duration, long timebase) {
            Duration = duration;
            Timebase = timebase;
        }

        public List<ChartMeasure> GetChartMeasures() {
            List<ChartMeasure> measures = new List<ChartMeasure>();

            long curTime = 0;
            int measureNum = 0;
            int timeSigIdx = 0;
            int tempoIdx = 0;

            // Base measures and time signatures
            while (curTime < Duration) {
                var curTimeSig = TimeTrack.TimeSignatures[timeSigIdx].TimeSignature;
                var nextTimeSig = TimeTrack.TimeSignatures.ElementAtOrDefault(timeSigIdx + 1);


                long measureLength = Timebase * curTimeSig.Top * 4 / curTimeSig.Bottom;
                long end = (nextTimeSig != null ? nextTimeSig.Time : Duration);
                while (curTime < end) {
                    ChartMeasure measure = new ChartMeasure(++measureNum, curTime, measureLength, curTimeSig);
                    measures.Add(measure);
                    curTime += measureLength;         
                }

                timeSigIdx++;
            }

            var curTempo = TimeTrack.Tempos[tempoIdx];
            foreach (var measure in measures) {
                var nextTempo = TimeTrack.Tempos.ElementAtOrDefault(tempoIdx + 1);
                // if not (another tempo exists AND the next tempo is at the start of this measure)
                if ((nextTempo == null) || (nextTempo.Time != measure.Time))
                    measure.Tempos.Add(curTempo);

                while (nextTempo != null && nextTempo.Time < measure.Time + measure.Duration) {
                    measure.Tempos.Add(nextTempo);
                    curTempo = nextTempo;
                    nextTempo = TimeTrack.Tempos.ElementAtOrDefault(++tempoIdx + 1);
                }
            }

            return measures;
        }

        public void SetTimeTrack(ChartTimeTrack track) {
            this.TimeTrack = track;
        }

        public void AddTrack(string instrument, ChartInstrumentTrack track) {
            this.Tracks.Add(instrument, track);
        }

        public void ApplyEventsFor(string instrument) {
            this.Tracks[instrument].ApplyEvents();
        }

        public void PutNotesInMeasures(string instrument) {
            this.Tracks[instrument].PutNotesIn(GetChartMeasures());
        }

        public bool IsPhraseUnison(OverdrivePhraseEvent odPhrase) {
            return (Tracks.Select(x => x.Value.PhraseTrack.PhraseEvents).Where(x => x.Any(y => y is OverdrivePhraseEvent && y.Time == odPhrase.Time)).Count() > 1);
        }

        public void ScoreMeasuresFor(string instrument) {
            this.Tracks[instrument].ScoreMeasures(this.Timebase, instrument);
        }

        public void ScoreMeasuresFor(string instrument, List<OverdriveActivationEvent> activations) {
            this.Tracks[instrument].ScoreMeasures(this.Timebase, instrument, activations);
        }
    }
}
