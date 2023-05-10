using RockBand.Charts.Models.Chart.Events;
using RockBand.Charts.Models.Chart.Events.PhraseEvents;
using RockBand.Charts.Models.Chart.Measures;
using RockBand.Charts.Models.Chart.Notes;
using RockBand.Charts.Models.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RockBand.Charts.Models.Chart.Tracks {
    public class ChartInstrumentTrack {
        public ChartNoteTrack NoteTrack { get; private set; }
        public ChartPhraseTrack PhraseTrack { get; private set; }
        public List<ChartMeasure> Measures { get; set; } = new List<ChartMeasure>();
        public List<OverdriveActivationEvent> Activations { get; set; } = new List<OverdriveActivationEvent>();

        public ChartInstrumentTrack(ChartNoteTrack noteTrack, ChartPhraseTrack phraseTrack) {
            NoteTrack = noteTrack;
            PhraseTrack = phraseTrack;
        }

        public List<ChartNote> GetNotesBetween(long start, long end) {
            return NoteTrack.Notes.Where(x => x.GetTime() >= start && x.GetTime() < end).ToList();
        }

        public void ApplyEvents() {
            foreach (var phraseEvent in PhraseTrack.PhraseEvents) {
                var coveredNotes = GetNotesBetween(phraseEvent.Time, phraseEvent.End);
                if (coveredNotes.Count == 0) continue;

                if (phraseEvent.PhraseNote == Phrases.ChartPhraseNote.OVERDRIVE) {
                    for (int i = 0; i < coveredNotes.Count; i++) {
                        ChartNote note = coveredNotes[i];
                        note.SetMetadata("OverdrivePhrase", phraseEvent);
                    }
                }
                else if (phraseEvent.PhraseNote == Phrases.ChartPhraseNote.SOLO) {
                    (phraseEvent as SoloPhraseEvent).NoteCount = coveredNotes.Count();
                    for (int i = 0; i < coveredNotes.Count; i++) {
                        ChartNote note = coveredNotes[i];
                        note.SetMetadata("IsInSolo", true);
                    }
                    phraseEvent.SetMetadata("LastNoteIndex", coveredNotes.Last().Index);
                }
            }
        }

        public void PutNotesIn(List<ChartMeasure> measures) {
            foreach (var note in NoteTrack.Notes) {
                var measureIndex = 0;
                while (measureIndex < measures.Count && note.Time >= measures[measureIndex].Time) {
                    measureIndex++;
                }
                measureIndex--;

                var thisMeasure = measures[measureIndex];
                note.SetMetadata("Measure", thisMeasure);

                // find the tempo region we're in
                for (var i = 0; i < thisMeasure.Tempos.Count; i++) {
                    var tempo = thisMeasure.Tempos[i];
                    // if this is the last tempo for the measure, we have to be in that one
                    if (i + 1 == thisMeasure.Tempos.Count)
                        note.SetMetadata("Tempo", tempo);
                    // There is at least one more tempo in this measure, so check
                    else if (note.Time >= tempo.Time && note.Time < thisMeasure.Tempos[i + 1].Time)
                        note.SetMetadata("Tempo", tempo);
                }

                thisMeasure.Notes.Add(note);
            }

            this.Measures = measures;
        }

        public void ScoreMeasures(long midiTimebase, string instrument) {
            ScoreMeasures(midiTimebase, instrument, new List<OverdriveActivationEvent>());
        }

        private static double _midiTicksToBeats(long ticks, long timebase) {
            return (double)ticks / (double)timebase;
        }

        public void ScoreMeasures(long midiTimebase, string instrument, List<OverdriveActivationEvent> activations) {
            if (activations.Count > 0) {
                var xzzz = 1;
            }

            this.Activations = activations;

            double timebase = midiTimebase;
            int maxMulti = (instrument.Equals("bass")) ? 6 : 4;

            double overageCount = 0; ;
            double multiplier = 1;
            double total = 0;
            double bonusTotal = 0;
            double streak = 0;
            double overage = 0;
            double scoreOverage = 0;
            double totalTicks = 0;
            double totalScoreOverage = 0;
            OverdriveActivationEvent? lastActivation = null;

            foreach (var measure in this.Measures) {
                if (measure.Number == 58) {
                    var q = 1;
                }
                double measureScore = 0;

                // Are we in an activation to start this measure?
                var thisActivations = Activations.Where(x => (measure.Time >= x.StartTick && measure.Time < x.End) // in activation to start the measure
                                                            || (x.StartTick > measure.Time && x.StartTick < measure.End)); // start activation during
                lastActivation = thisActivations.FirstOrDefault();

                // take care of leftovers
                if (overage > 0) {
                    double newOverage = 0, newScoreOverage = 0, newTotalScoreOverage = 0;
                    if (overage > measure.TimeSignature.Top) {
                        // this sutain goes through entire measure into next
                        newOverage = overage - measure.TimeSignature.Top;
                        newScoreOverage = scoreOverage - (GameConstants.TICKS_PER_BEAT * measure.TimeSignature.Top * overageCount);
                        scoreOverage = (GameConstants.TICKS_PER_BEAT * measure.TimeSignature.Top * overageCount);
                        totalScoreOverage = scoreOverage;
                        overage = measure.TimeSignature.Top;
                    }

                    measureScore += scoreOverage;
                    total += (multiplier * scoreOverage);
                    bonusTotal += (multiplier * scoreOverage);

                    // Get percentage of overage covered by this activation (that we started the measure with)
                    if (lastActivation != null) {
                        // we either cover all of the overage, or we only cover part of it
                        var coveredOverage = Math.Min(overage, _midiTicksToBeats(lastActivation.End - measure.Time, midiTimebase));
                        total += (multiplier * coveredOverage);
                        bonusTotal += (multiplier * coveredOverage);
                    }

                    overage = newOverage;
                    scoreOverage = newScoreOverage;
                    totalScoreOverage = newTotalScoreOverage;
                }

                foreach (var note in measure.Notes) {
                    streak++;
                    bool incMulti = (streak % 10 == 0 && multiplier < maxMulti);
                    double oldMultiplier = multiplier;
                    if (incMulti)
                        ++multiplier;
                    oldMultiplier = multiplier;

                    // Check if this note is covered at all by an activation
                    var noteActivation = thisActivations.Where(x => x.StartTick <= note.GetEnd() && x.End > note.Time).FirstOrDefault();
                    var inActivation = (noteActivation != null);

                    if (note.GetEnd() > measure.End)
                        overage = (double)((note.GetEnd() - measure.End) / timebase);

                    // measure score
                    int gemScore = GameConstants.GEM_SCORE * note.GetButtonCount();

                    double ticks = Math.Floor((GameConstants.TICKS_PER_BEAT * _midiTicksToBeats(note.EffectiveDuration, midiTimebase)) + 1.0e-10);
                    if (overage > 0) {
                        double measureTicks = Math.Floor(ticks * (measure.End - note.Time) / note.EffectiveDuration);
                        scoreOverage = (ticks - measureTicks) * note.GetButtonCount();
                        ticks = measureTicks;
                    }

                    ticks *= note.GetButtonCount();
                    measureScore += (gemScore + ticks);

                    // total score
                    totalTicks = Math.Floor((GameConstants.TICKS_PER_BEAT * _midiTicksToBeats(note.EffectiveDuration, midiTimebase)) + 0.5 + 1e-10);
                    if (overage > 0) {
                        double totalMeasureTicks = Math.Floor((totalTicks * (measure.End - note.Time) / note.EffectiveDuration));
                        totalScoreOverage = (totalTicks - totalMeasureTicks) * (note.GetButtonCount());
                        totalTicks = totalMeasureTicks;
                    }
                    totalTicks *= note.GetButtonCount();

                    // tick multiplier

                    total += (gemScore * multiplier) + (totalTicks * oldMultiplier);
                    bonusTotal += (gemScore * multiplier) + (totalTicks * oldMultiplier);

                    // Apply activation bonuses
                    if (inActivation) {
                        double coveredGems = 0;
                        double coveredTicksBase = 0;
                        foreach (var act in thisActivations) {
                            // gems
                            if (act.StartTick <= note.Time) {
                                coveredGems += note.GetButtonCount();
                                // since we cover the start of the gem, let's count ticks
                                // this is fine, since any ticks that bleed over to the next measure get dealt with above in overage
                                if (note.EffectiveDuration > 0) {
                                    var actScoreEnd = Math.Min(note.GetEnd(), act.End);
                                    coveredTicksBase += (actScoreEnd - note.Time);
                                }
                            }

                            // ticks, for activations that start during the sustain
                            // really, the min() should also check activation, but
                            // in reality you can't start and end an activation in the same measure
                            if (act.StartTick > note.Time && note.EffectiveDuration > 0) {
                                double maxEnd = Math.Min(note.GetEnd(), measure.End);
                                coveredTicksBase += maxEnd - act.StartTick;
                            }
                        }

                        total += (coveredGems * GameConstants.GEM_SCORE * multiplier);
                        total += (Math.Floor((GameConstants.TICKS_PER_BEAT * _midiTicksToBeats((long)coveredTicksBase, midiTimebase)) + 0.5 + 1e-10) * oldMultiplier);
                        bonusTotal += (coveredGems * GameConstants.GEM_SCORE * multiplier);
                        bonusTotal += (Math.Floor((GameConstants.TICKS_PER_BEAT * _midiTicksToBeats((long)coveredTicksBase, midiTimebase)) + 0.5 + 1e-10) * oldMultiplier);
                    }
                    overageCount = note.GetButtonCount();
                }

                // SOLO check
                foreach (var phraseEvent in PhraseTrack.PhraseEvents.Where(x => x is SoloPhraseEvent)) {
                    var soloEvent = phraseEvent as SoloPhraseEvent;
                    if (soloEvent.End >= measure.Time && soloEvent.End < measure.End) {
                        bonusTotal += soloEvent.NoteCount * 100;
                    }
                }
                if (total != bonusTotal) measure.SetMetadata("BonusScore", (int)bonusTotal);
                measure.SetMetadata("MeasureScore", (int)measureScore);
                measure.SetMetadata("CumulativeScore", (int)total);
                measure.SetMetadata("Streak", (int)streak);
            }
        }
    }
}
