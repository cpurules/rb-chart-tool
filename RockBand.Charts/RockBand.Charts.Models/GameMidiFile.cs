using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models {
    public class GameMidiFile {
        public static long CHORD_THRESHOLD = 15;
        public static long SUSTAIN_THRESHOLD = 160;
        public static long HOPO_THRESHOLD = 170;
        public static long TICKS_PER_BEAT = 12;
        public static int GEM_SCORE = 25;
        
        public List<GameTimeSignatureEvent> TimeSignatures { get; set; } = new List<GameTimeSignatureEvent>();
        public List<GameTempoEvent> Tempos { get; set; } = new List<GameTempoEvent>();
        public long Duration { get; set; }
        public long TimeBase { get; set; }
        public Dictionary<GameMidiTrackType, GameMidiTrack> Tracks { get; set; } = new Dictionary<GameMidiTrackType, GameMidiTrack>();
        public List<GameMidiMeasure> MeasureMap { get; set; }

        public GameMidiFile(long duration, long timeBase) {
            this.Duration = duration;
            this.TimeBase = timeBase;
        }

        public void GenerateMeasureMap() {
            List<GameMidiMeasure> measures = new List<GameMidiMeasure>();

            long curTime = 0;
            int curTimeSigIdx = 0;
            while (curTime < this.Duration) {
                GameTimeSignatureEvent ts = this.TimeSignatures[curTimeSigIdx];
                long measureLength = this.TimeBase * ts.Numerator * 4 / ts.Denominator;

                long nextTsChangeAt;
                try {
                    nextTsChangeAt = this.TimeSignatures[curTimeSigIdx + 1].DeltaTime;
                }
                catch {
                    nextTsChangeAt = this.Duration;
                }

                while (curTime < nextTsChangeAt) {
                    // Create a new measure
                    // number; time; duration; time sig num; time sig denom; notes; tempos
                    measures.Add(new GameMidiMeasure(measures.Count + 1, curTime, measureLength, ts.Numerator, ts.Denominator));

                    // Move along
                    curTime += measureLength;
                }

                // Hit next ts change
                curTimeSigIdx++;
            }

            // Loop through measures and add tempos
            int tempoIndex = 0;
            foreach (GameMidiMeasure measure in measures) {
                GameTempoEvent tempo = this.Tempos[tempoIndex];

                // if not (not last tempo track & next time track's time is start of this measure)
                // if last tempo track, or next tempo track's time is after this measure's start
                // Then add the last tempo event to this measure first
                if (tempoIndex + 1 == this.Tempos.Count || this.Tempos[tempoIndex+1].DeltaTime > measure.Time) {
                    measure.AddTempo(tempo);
                }

                // while (another tempo track and the next tempo track's time occurs during this measure)
                while (tempoIndex + 1 < this.Tempos.Count && this.Tempos[tempoIndex+1].DeltaTime < measure.Time + measure.Duration) {
                    tempo = this.Tempos[++tempoIndex];
                    measure.AddTempo(tempo);
                }
            }

            this.MeasureMap = measures;
        }
    }
}
