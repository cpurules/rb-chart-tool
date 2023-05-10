using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

using System.Collections;
using System.Security.Cryptography.X509Certificates;
using RockBand.Charts.Models.Chart;
using RockBand.Charts.Models.Chart.Tracks;
using RockBand.Charts.Models;
using RockBand.Charts.Models.Game;
using RockBand.Charts.Models.Chart.Notes;
using RockBand.Charts.Models.Chart.Phrases;
using RockBand.Charts.Models.Chart.Events.PhraseEvents;

namespace RockBand.Charts.FileHandlers
{
    public class MidiHandler {
        public string FileName { get; }
        private MidiFile _handler;
        private long _duration { get { return _handler.GetDuration<MidiTimeSpan>().TimeSpan; } }
        private long _timebase { get { return (_handler.TimeDivision as TicksPerQuarterNoteTimeDivision).TicksPerQuarterNote; } }

        public MidiHandler(string fileName) {
            FileName = fileName;
            _handler = MidiFile.Read(fileName);
        }        

        public Chart Read() {
            Chart chart = new Chart(_duration, _timebase);
            chart.SetTimeTrack(_generateTimeTrack());
            chart.AddTrack("guitar", _generateInstrumentTrack("guitar"));
            chart.AddTrack("bass", _generateInstrumentTrack("bass"));
            chart.ApplyEventsFor("guitar");
            chart.ApplyEventsFor("bass");
            chart.PutNotesInMeasures("guitar");
            chart.PutNotesInMeasures("bass");
            chart.ScoreMeasuresFor("guitar");
            chart.ScoreMeasuresFor("bass");
            return chart;
        }

        private ChartInstrumentTrack _generateInstrumentTrack(string instrument) {
            // Reset type counters
            SoloPhraseEvent.ResetCounter();
            OverdrivePhraseEvent.ResetCounter();

            ChartNoteTrack noteTrack = _generateNoteTrack(instrument);
            ChartPhraseTrack phraseTrack = _generatePhraseTrack(instrument);

            return new ChartInstrumentTrack(noteTrack, phraseTrack);
        }

        private int _getTrackNumber(string instrument) {
            switch (instrument.ToLower()) {
                case "guitar": return 1;
                case "bass": return 3;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private ChartNoteTrack _generateNoteTrack(string instrument) {
            ChartNoteTrack track = new ChartNoteTrack(_duration);

            var rawNotes = _handler.GetTrackChunks().Skip(_getTrackNumber(instrument)).First()
                                .GetNotes().Where(x => Enum.IsDefined(typeof(ChartNoteButton), (int)x.NoteNumber));

            int lastNoteIdx = -1;
            int noteCount = 0;
            int noteIdx = -1;

            foreach (var rawNote in rawNotes) {
                // Check to see if this note is part of a chord; if not, move on
                if (!track.Notes.Any(x => x.GetTime() >= (rawNote.Time - ((x.GetButtonCount() + 1) * GameConstants.CHORD_THRESHOLD))))
                    noteIdx++;


                // Validate regular note
                if (rawNote.Velocity > 0) {
                    ChartNoteButton button = (ChartNoteButton)(int)rawNote.NoteNumber;
                    ChartNote currentNote;

                    if (instrument.Equals("bass")) {
                        int a = 1;
                    }
                    if (track.Notes.Count == noteIdx) { // We've moved forward
                        currentNote = new ChartNote(noteIdx, rawNote.Time, rawNote.EndTime - rawNote.Time, button);
                        track.AddNote(currentNote);
                    }
                    else { // This is an extra button on the chord
                        currentNote = track.Notes[noteIdx];
                        currentNote.AddButton(button);
                    }

                    ChartNote? lastNote = track.Notes.ElementAtOrDefault(lastNoteIdx);

                    // Handle HOPOs
                    if (currentNote.GetButtonCount() > 1)
                        currentNote.SetHopo(false);
                    else if (lastNote != null && currentNote.GetTime() - lastNote.GetTime() < GameConstants.HOPO_THRESHOLD) {
                        // No auto HOPO for buttons that appear in the previous note
                        currentNote.SetHopo(!lastNote.GetButtons().Contains(button));
                    }

                    if (currentNote.GetButtonCount() == 1) lastNoteIdx = noteIdx;
                }
            }

            return track;
        }

        private ChartPhraseTrack _generatePhraseTrack(string instrument) {
            ChartPhraseTrack track = new ChartPhraseTrack();

            var rawNotes = _handler.GetTrackChunks().Skip(_getTrackNumber(instrument)).First()
                                .GetNotes().Where(x => Enum.IsDefined(typeof(ChartPhraseNote), (int)x.NoteNumber));
            
            foreach (var rawNote in rawNotes) {
                ChartPhraseNote phraseNote = (ChartPhraseNote)(int)rawNote.NoteNumber;
                ChartPhraseEvent phraseEvent;
                if (phraseNote == ChartPhraseNote.OVERDRIVE)
                    phraseEvent = new OverdrivePhraseEvent(rawNote.Time, rawNote.EndTime - rawNote.Time);
                else if (phraseNote == ChartPhraseNote.SOLO)
                    phraseEvent = new SoloPhraseEvent(rawNote.Time, rawNote.EndTime - rawNote.Time, 0);
                else
                    throw new NotImplementedException();
                track.AddPhraseEvent(phraseEvent);
            }

            return track;
        }

        private ChartTimeTrack _generateTimeTrack() {
            ChartTimeTrack track = new ChartTimeTrack();

            using (var tempoMapMgr = new TempoMapManager(_handler.TimeDivision, _handler.GetTrackChunks().Select(c => c.Events))) {
                TempoMap map = tempoMapMgr.TempoMap;
                MidiTimeSpan start = new MidiTimeSpan(0);

                TimeSignature ts = map.GetTimeSignatureAtTime(start);
                track.AddTimeSignature(0, new GameTimeSignature(ts.Numerator, ts.Denominator));
                foreach (var tse in map.GetTimeSignatureChanges())
                    track.AddTimeSignature(tse.Time, new GameTimeSignature(tse.Value.Numerator, tse.Value.Denominator));

                Tempo tempo = map.GetTempoAtTime(start);
                //track.AddTempo(0, new GameTempo(tempo.MicrosecondsPerQuarterNote));
                foreach (var te in map.GetTempoChanges())
                    track.AddTempo(te.Time, new GameTempo(te.Value.MicrosecondsPerQuarterNote));
            }

            return track;
        }


        /*
        private void AddEventsTrack(GameMidiFile gmf) {
            var track = _handler.GetTrackChunks().Skip(GameMidiTrackType.EVENTS.TrackNumber()).First();

            foreach (var trackEvent in track.Events) {
                if (trackEvent.EventType == MidiEventType.TimeSignature) {
                    var tsEvent = trackEvent as TimeSignatureEvent;
                    gmf.EventsTrack.AddTimeSignature(
                        new GameTimeSignatureEvent(
                            tsEvent.DeltaTime,
                            tsEvent.Numerator,
                            tsEvent.Denominator,
                            tsEvent.ClocksPerClick,
                            tsEvent.ThirtySecondNotesPerBeat
                    ));
                }
                else if (trackEvent.EventType == MidiEventType.SetTempo) {
                    var tempoEvent = trackEvent as SetTempoEvent;
                    gmf.EventsTrack.AddTempo(
                        new GameTempoEvent(
                            tempoEvent.DeltaTime,
                            tempoEvent.MicrosecondsPerQuarterNote
                    ));
                }
            }
        }

        private void AddNotesTrack(GameMidiTrackType type, GameMidiFile gmf) {
            if (!gmf.NotesTracks.ContainsKey(type))
                gmf.NotesTracks.Add(type, new GameNotesTrack());

            var noteTrack = _handler.GetTrackChunks().Skip(type.TrackNumber()).First();
            var notes = noteTrack.GetNotes();
            foreach (Note note in notes) {
                var noteNumber = Convert.ToInt32(note.NoteNumber);
                if (Enum.IsDefined(typeof(GameNoteType), noteNumber)) {
                    GameNoteType noteType = (GameNoteType)noteNumber;
                    long noteOn = note.Time;
                    long noteOff = note.EndTime;
                    GameMidiNote gmn = new GameMidiNote(noteType, noteOn, noteOff);

                    gmf.NotesTracks[type].Notes.Add(gmn);
                }
            }
        }*/
    }
}