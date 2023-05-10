using RockBand.Charts.Models;
using RockBand.Charts.Models.Chart;
using RockBand.Charts.Models.Chart.Events;
using RockBand.Charts.Models.Chart.Events.PhraseEvents;
using RockBand.Charts.Models.Chart.Measures;
using RockBand.Charts.Models.Chart.Notes;
using RockBand.Charts.Models.Chart.Phrases;
using RockBand.Charts.Models.Game;
using System.Collections.Immutable;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using static System.Formats.Asn1.AsnWriter;

namespace RockBand.Charts.ChartGen {
    public class ChartGenerator {
        public Chart Midi { get; set; }
        public int ChartHeight { get; }
        public static int ChartWidth { get; } = 1070;
        public static int PixelsPerBeat { get; } = 60;
        public static int StaffHeight { get; } = 12;
        public static int BpmPrecision { get; } = 1;

        public Bitmap Chart { get; set; }
        private string _instrument { get; set; } = "guitar";
        private DrawingConfig _drawingConfig;
        private int _drawX = 25;
        private int _drawY = 100;


        public ChartGenerator(Chart midi) {
            Midi = midi;
            ChartHeight = _calculateHeight();
        }

        public void Save(string path) {
            this.Chart.Save(path, ImageFormat.Png);
        }

        public Bitmap DrawChart(string instrument) {
            return DrawChart(instrument, new List<OverdriveActivationEvent>());
        }

        public Bitmap DrawChart(string instrument, List<OverdriveActivationEvent> activations) {
            _instrument = instrument;
            _initChart();
            _drawMeasures(activations);


            return Chart;
        }

        private void _initChart() {
            _drawX = 25;
            _drawY = 100;
            Chart = new Bitmap(ChartWidth, ChartHeight);
            using (var g = Graphics.FromImage(Chart)) {
                g.Clear(Color.White);
                using (var font = new Font("Consolas", 12, FontStyle.Bold)) {
                    g.DrawString("[song name]", font, Brushes.Black, 0, 0);
                    g.DrawString(_instrument.ToLower(), font, Brushes.Black, 0, 15);
                    g.DrawString("Generated at " + DateTime.Now, font, Brushes.DarkGray, ChartWidth - 320, ChartHeight - 27);
                }
            }
        }

        private void _drawMeasures(List<OverdriveActivationEvent> activations) {
            var measures = Midi.Tracks[_instrument].Measures;

            _drawingConfig = new DrawingConfig();

            foreach (var measure in measures) {
                var index = measure.Number - 1;
                if (_drawX + _pxPerMeasure(measure) > ChartWidth - 25) {
                    _drawX = 25;
                    _drawY += 25 + 45 + 5 * StaffHeight;
                }

                _drawMeasureBackground(measure, _drawX, _drawY, activations);
                _drawMeasureNotes(measure, _drawX, _drawY);
                _drawMeasureScores(measure, _drawX, _drawY);

                if (_drawX + _pxPerMeasure(measure) > ChartWidth - 50) {
                    _drawX = 25;
                    _drawY += 25 + 45 + 5 * StaffHeight;
                }
                else {
                    _drawX += _pxPerMeasure(measure);
                }
            }
        }

        private void _drawMeasureScores(ChartMeasure measure, int x, int y) {
            using (var g = Graphics.FromImage(Chart)) {
                // Measure Score
                string mScore = measure.GetMetadata<int>("MeasureScore").ToString();

                g.DrawString(mScore, new Font("Consolas", 10), Brushes.Black, x + _pxPerMeasure(measure) - mScore.Length * 9, y + 48);

                // cumulative score
                string cScore = measure.GetMetadata<int>("CumulativeScore").ToString();
                g.DrawString(cScore, new Font("Consolas", 10), Brushes.Firebrick, x + _pxPerMeasure(measure) - cScore.Length * 9, y + 58);

                // optional bonus score
                if (measure.HasMetadata("BonusScore")) {
                    string bScore = measure.GetMetadata<int>("BonusScore").ToString();
                    g.DrawString(bScore, new Font("Consolas", 10), Brushes.ForestGreen, x + _pxPerMeasure(measure) - (cScore.Length + bScore.Length) * 9 + 4, y + 58);
                }
            }
        }

        private void _drawMeasureBackground(ChartMeasure measure, int x, int y, List<OverdriveActivationEvent> activations) {
            var ts = measure.TimeSignature;

            // check for overdrive activation in this measure
            foreach (var activation in activations) {
                Color color = Color.FromArgb(175, 175, 255);
                int by = y - 15; int bey = y + 15 + StaffHeight * 4;

                int bx, bex;

                using (var g = Graphics.FromImage(Chart)) {
                    if (measure.Number == 27) {
                        var qq = 1;
                    }
                    if (activation.StartTick >= measure.Time && activation.End <= measure.End) {
                        // wholly contained
                        bx = (int)((activation.StartTick - measure.Time) * PixelsPerBeat / Midi.Timebase) + x;
                        bex = (int)((activation.End - measure.Time) * PixelsPerBeat / Midi.Timebase) + x;

                        if (by != bey) {
                            g.FillRectangle(new SolidBrush(color), bx, by, (bex - bx), (bey - by));
                        }
                    }
                    else if (activation.StartTick < measure.Time && activation.End > measure.End) {
                        // goes through entire measure
                        bx = x;
                        bex = x + _pxPerMeasure(measure);

                        g.FillRectangle(new SolidBrush(color), bx, by, _pxPerMeasure(measure), (bey - by));
                    }
                    else if (activation.StartTick < measure.Time && activation.End >= measure.Time && activation.End <= measure.End) {
                        // starts before, ends during
                        bx = x;
                        bex = (int)((activation.End - measure.Time) * PixelsPerBeat / Midi.Timebase + x);

                        g.FillRectangle(new SolidBrush(color), bx, by, (bex - bx), (bey - by));
                    }
                    else if (activation.StartTick >= measure.Time && activation.StartTick < measure.End && activation.End >= measure.End) {
                        // starts during, ends after
                        bx = (int)((activation.StartTick - measure.Time) * PixelsPerBeat / Midi.Timebase + x);
                        bex = x + _pxPerMeasure(measure);

                        g.FillRectangle(new SolidBrush(color), bx, by, bex - bx, bey - by);
                    }
                }
            }

            // check for event lines in this measure
            foreach (var phraseEvent in Midi.Tracks[_instrument].PhraseTrack.PhraseEvents) {
                // Cases to check:
                // -> wholly contained
                // -> goes through entire measure
                // -> start before, ends in
                // -> start in, ends after

                Color color;
                int by, bey;
                var drawUnison = false;

                if (phraseEvent.PhraseNote == ChartPhraseNote.SOLO) {
                    color = Color.FromArgb(224, 224, 255);
                    by = y;
                    bey = y + StaffHeight * 4;
                }
                else if (phraseEvent.PhraseNote == ChartPhraseNote.OVERDRIVE) {
                    color = Color.FromArgb(192, 255, 192);
                    by = y - 5;
                    bey = y + 5 + StaffHeight * 4;
                    if (Midi.IsPhraseUnison(phraseEvent as OverdrivePhraseEvent)) {
                        drawUnison = true;
                    }
                }
                else {
                    continue;
                }

                int bx, bex;

                using (var g = Graphics.FromImage(Chart)) {
                    if (phraseEvent.Time >= measure.Time && phraseEvent.End <= measure.End) {
                        // wholly contained
                        bx = (int)((phraseEvent.Time - measure.Time) * PixelsPerBeat / Midi.Timebase) + x;
                        bex = (int)((phraseEvent.End - measure.Time) * PixelsPerBeat / Midi.Timebase) + x;

                        // draw unison if applicable
                        if (drawUnison) {
                            g.FillRectangle(Brushes.Gold, bx, by - 4, bex - bx, (bey - by) + 8);
                        }


                        if (by != bey) {
                            g.FillRectangle(new SolidBrush(color), bx, by, (bex - bx), (bey - by));
                        }

                        // draw number of notes in solo
                        if (phraseEvent.PhraseNote == ChartPhraseNote.SOLO) {
                            int tx = (int)((phraseEvent.End - measure.Time) * PixelsPerBeat / Midi.Timebase + x + 2);
                            int ty = y - 35;
                            g.DrawString((phraseEvent as SoloPhraseEvent).NoteCount + " notes", new Font("Consolas", 10), Brushes.Black, tx, ty);
                        }
                    }
                    else if (phraseEvent.Time < measure.Time && phraseEvent.End > measure.End) {
                        // goes through entire measure
                        bx = x;
                        bex = x + _pxPerMeasure(measure);

                        // draw unison if applicable
                        if (drawUnison) {
                            g.FillRectangle(Brushes.Gold, bx, by - 4, (bex - bx), (bey - by) + 8);
                        }

                        g.FillRectangle(new SolidBrush(color), bx, by, _pxPerMeasure(measure), (bey - by));
                    }
                    else if (phraseEvent.Time < measure.Time && phraseEvent.End >= measure.Time && phraseEvent.End <= measure.End) {
                        // starts before, ends during
                        bx = x;
                        bex = (int)((phraseEvent.End - measure.Time) * PixelsPerBeat / Midi.Timebase + x);


                        // draw unison if applicable
                        if (drawUnison) {
                            g.FillRectangle(Brushes.Gold, bx, by - 4, (bex - bx), (bey - by) + 8);
                        }

                        g.FillRectangle(new SolidBrush(color), bx, by, (bex - bx), (bey - by));

                        // draw number of notes in solo
                        if (phraseEvent.PhraseNote == ChartPhraseNote.SOLO) {
                            int tx = (int)((phraseEvent.End - measure.Time) * PixelsPerBeat / Midi.Timebase + x + 2);
                            int ty = y - 35;
                            g.DrawString((phraseEvent as SoloPhraseEvent).NoteCount + " notes", new Font("Consolas", 10), Brushes.Black, tx, ty);
                        }
                    }
                    else if (phraseEvent.Time >= measure.Time && phraseEvent.Time < measure.End && phraseEvent.End >= measure.End) {
                        // starts during, ends after
                        bx = (int)((phraseEvent.Time - measure.Time) * PixelsPerBeat / Midi.Timebase + x);
                        bex = x + _pxPerMeasure(measure);


                        // draw unison if applicable
                        if (drawUnison) {
                            g.FillRectangle(Brushes.Gold, bx, by - 4, (bex - bx), (bey - by) + 8);
                        }

                        g.FillRectangle(new SolidBrush(color), bx, by, bex - bx, bey - by);
                    }
                }
            }


            // measure outline
            using (var g = Graphics.FromImage(Chart)) {
                g.DrawLine(Pens.DarkSlateGray, x, y, x + _pxPerMeasure(measure), y);
                g.DrawLine(Pens.DarkSlateGray, x, y + (StaffHeight * 4), x + _pxPerMeasure(measure), y + (StaffHeight * 4));
                g.DrawLine(Pens.DarkSlateGray, x, y, x, y + (StaffHeight * 4));
                // draw right side later

                // beat lines
                for (int i = 0; i < ts.Top * 4 / ts.Bottom; i++) {
                    var _x = (i * PixelsPerBeat + PixelsPerBeat / 2);
                    // up beat line
                    g.DrawLine(Pens.LightGray, x + _x, y + 1, x + _x, y - 1 + StaffHeight * 4);
                    // skip last down beat line
                    if (i + 1 < (ts.Top * 4 / ts.Bottom)) {
                        g.DrawLine(Pens.DarkGray, x + (i + 1) * PixelsPerBeat, y + 1, x + (i + 1) * PixelsPerBeat, y - 1 + StaffHeight * 4);
                    }
                }

                // right side
                g.DrawLine(Pens.DarkSlateGray, x + _pxPerMeasure(measure), y, x + _pxPerMeasure(measure), y + (StaffHeight * 4));

                // staff lines
                for (int i = 1; i < 4; i++) {
                    g.DrawLine(Pens.DarkGray, x + 1, y + StaffHeight * i, x - 1 + _pxPerMeasure(measure), y + StaffHeight * i);
                }

                // time signature
                if (ts.Top != _drawingConfig.LastTsNum || ts.Bottom != _drawingConfig.LastTsDenom) {
                    _drawingConfig.LastTsNum = ts.Top;
                    _drawingConfig.LastTsDenom = ts.Bottom;

                    g.DrawString(ts.Top.ToString(), new Font("Consolas", 14, FontStyle.Bold), Brushes.LightSlateGray, x + 2, y);
                    g.DrawString(ts.Bottom.ToString(), new Font("Consolas", 14, FontStyle.Bold), Brushes.LightSlateGray, x + 2, y + StaffHeight * 2);
                }

                // measure number
                g.DrawString(measure.Number.ToString(), new Font("Consolas", 10), Brushes.Firebrick, x, y - 16);

                // section name... skipping tbh

                // tempo
                foreach (var tempo in measure.Tempos) {
                    var thisBPM = Math.Round(tempo.Tempo.BPM, BpmPrecision);
                    if (thisBPM != _drawingConfig.LastBPM) {
                        var bx = (tempo.Time - measure.Time) / Midi.Timebase;
                        bx *= 4 / measure.TimeSignature.Bottom;
                        bx *= PixelsPerBeat;
                        bx += x;
                        g.FillEllipse(Brushes.Gray, bx + 4, y - 19, 4, 4);
                        g.DrawLine(Pens.Gray, bx + 7, y - 23, bx + 7, y - 19);
                        g.DrawString("=" + thisBPM, new Font("Consolas", 10), Brushes.Gray, bx + 10, y - 27);
                        _drawingConfig.LastBPM = thisBPM;
                    }
                }
            }
        }

        private void _drawMeasureNotes(ChartMeasure measure, int x, int y) {
            if (measure.Number == 1) {
                _drawingConfig.Leftover = null;
                _drawingConfig.Overwhammy = 0;
            }

            SustainLeftover newLeftover = null;
            if (_drawingConfig.Leftover != null) {
                var leftover = _drawingConfig.Leftover;
                foreach (ChartNoteButton button in leftover.Note.Buttons) {
                    var ny = StaffHeight * ((int)button - 96) + y;

                    var remDur = leftover.RemainingDuration;
                    if (remDur > (_beatsPerMeasure(measure) * Midi.Timebase)) {
                        // extends into next measure
                        newLeftover = new SustainLeftover();
                        newLeftover.Note = leftover.Note;
                        newLeftover.RemainingDuration = leftover.RemainingDuration - (_beatsPerMeasure(measure) * Midi.Timebase);
                    }

                    var eX = (int)((remDur * 4 / measure.TimeSignature.Bottom) * PixelsPerBeat / Midi.Timebase + x);

                    ChartPhraseEvent? odPhrase = leftover.Note.GetMetadata<ChartPhraseEvent>("OverdrivePhrase");
                    var partOfPhrase = (odPhrase != null);

                    using (Graphics g = Graphics.FromImage(Chart)) {
                        SolidBrush color;
                        if (partOfPhrase) color = new SolidBrush(Color.FromArgb(168, 168, 168));
                        else color = new SolidBrush(button.GetColor());
                        g.FillRectangle(color, x + 1, ny - 1, (eX - x - 1), 3);
                    }
                }
            }
            _drawingConfig.Leftover = newLeftover;

            double whammy = _drawingConfig.Overwhammy;
            if (_drawingConfig.Overwhammy > measure.TimeSignature.Top) {
                _drawingConfig.Overwhammy -= measure.TimeSignature.Top;
                whammy = measure.TimeSignature.Top;
            }
            else {
                _drawingConfig.Overwhammy = 0;
            }


            foreach (ChartNote note in measure.Notes) {
                _drawNote(note, measure, x, y);

                // see if whammy beats are present
                if (note.HasMetadata("OverdrivePhrase") && note.EffectiveDuration > 0) {
                    if (note.GetEnd() > measure.End) {
                        _drawingConfig.Overwhammy += ((note.GetEnd() - measure.End) / (double)Midi.Timebase);
                        whammy += (measure.End - note.Time) / (double)Midi.Timebase;
                    }
                    else {
                        whammy += (note.EffectiveDuration / (double)Midi.Timebase);
                    }
                }
            }

            if (whammy > 0) {
                measure.SetMetadata("Whammy", whammy);
                whammy = Math.Round(whammy, 3);
                var odStr = (whammy.ToString() + " OD");
                using (Graphics g = Graphics.FromImage(Chart)) {
                    g.DrawString(whammy.ToString() + " OD", new Font("Consolas", 10), Brushes.DarkGray, x + _pxPerMeasure(measure) - odStr.Length * 9, y + 68);
                }
            }
        }

        private void _drawNote(ChartNote note, ChartMeasure measure, int x, int y) {
            note.GetButtons().Sort();
            if (measure.Number == 20) {
                var a = 1;
            }

            int nx = (int)((note.GetTime() - measure.Time) * PixelsPerBeat / Midi.Timebase + x);

            // check for OD phrase
            ChartPhraseEvent? odPhrase = note.GetMetadata<ChartPhraseEvent>("OverdrivePhrase");
            var partOfPhrase = (odPhrase != null);

            using (Graphics g = Graphics.FromImage(Chart)) {
                foreach (ChartNoteButton button in note.GetButtons()) {
                    SolidBrush color;
                    if (partOfPhrase) color = new SolidBrush(Color.FromArgb(168, 168, 168));
                    else color = new SolidBrush(button.GetColor());

                    var ny = (StaffHeight * ((int)button - 96)) + y;
                    if (note.IsHopo()) {
                        g.FillRectangle(color, nx - 1, ny - 3, 3, 6);
                        if (partOfPhrase) g.DrawLine(Pens.LightGray, nx, ny + 1, nx, ny - 1);
                        g.DrawLine(Pens.Black, nx - 1, ny - 4, nx + 1, ny - 4);
                        g.DrawLine(Pens.Black, nx - 1, ny + 3, nx + 1, ny + 3);
                    }
                    else {
                        g.FillRectangle(color, nx - 1, ny - 4, 3, 9);
                        if (partOfPhrase) g.DrawLine(Pens.LightGray, nx, ny - 2, nx, ny + 2);
                        g.DrawLine(Pens.Black, nx - 1, ny - 5, nx + 1, ny - 5);
                        g.DrawLine(Pens.Black, nx - 1, ny + 5, nx + 1, ny + 5);
                    }

                    if (note.GetDuration() > (GameConstants.SUSTAIN_THRESHOLD * 1.5)) {
                        if (note.GetEnd() > measure.Time + (_beatsPerMeasure(measure) * Midi.Timebase)) {
                            // this sustain extends to the next measure
                            var leftover = new SustainLeftover();
                            leftover.Note = note;
                            leftover.RemainingDuration = note.GetEnd() - (_beatsPerMeasure(measure) * Midi.Timebase + measure.Time);
                            _drawingConfig.Leftover = leftover;
                        }

                        var eP = Math.Min(note.GetEnd(), measure.Time + (_beatsPerMeasure(measure) * Midi.Timebase));
                        var eX = (int)((eP - measure.Time) * PixelsPerBeat / Midi.Timebase) + x;
                        g.FillRectangle(color, nx + 1, ny - 1, (eX - nx - 1), 3);
                    }

                }
            }
        }

        private static int _beatsPerMeasure(ChartMeasure measure) {
            return measure.TimeSignature.Top * 4 / measure.TimeSignature.Bottom;
        }

        private static int _pxPerMeasure(ChartMeasure measure) {
            return PixelsPerBeat * _beatsPerMeasure(measure);
        }

        private int _calculateHeight() {
            int height;

            int x = 25;
            int y = 100;

            var measures = this.Midi.GetChartMeasures();

            for (int i = 0; i < measures.Count; i++) {
                ChartMeasure measure = measures[i];
                // if we would exceed the right margin, typewriter it up
                if (x + PixelsPerBeat * measure.TimeSignature.Top * 4 / measure.TimeSignature.Bottom > ChartWidth - 25) {
                    x = 25;
                    y += 25 + 45 + 5 * StaffHeight;
                }
                // if we would exceed double the right margin on the last measure, typewriter it up
                if (x + PixelsPerBeat * measure.TimeSignature.Top * 4 / measure.TimeSignature.Bottom > ChartWidth - 50 && i + 1 < measures.Count) {
                    x = 25;
                    y += 25 + 45 + 5 * StaffHeight;
                }
                // otherwise, move along x
                else {
                    x += PixelsPerBeat * measure.TimeSignature.Top * 4 / measure.TimeSignature.Bottom;
                }
            }

            return y + 40 + 45 + 5 * StaffHeight;
        }
    }
}