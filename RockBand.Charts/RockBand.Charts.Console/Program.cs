using Melanchall.DryWetMidi.Core;
using RockBand.Charts.ChartGen;
using RockBand.Charts.FileHandlers;
using RockBand.Charts.Models;
using RockBand.Charts.Models.Chart;
using RockBand.Charts.Models.Chart.Events;
using RockBand.Charts.Models.Chart.Events.PhraseEvents;

namespace RockBand.Charts.ConsoleApp {
    public class Program {
        private static long _beatsToMidiTick(double beats, long timebase) {
            return (long)Math.Round(beats * timebase);
        }
        public static void Main(string[] args) {
            MidiHandler handler;

            handler = new MidiHandler("C:\\Users\\CPURules\\Desktop\\RockBand\\Charting\\FeedBack0.97b\\Songs\\circles.mid");
            //handler = new MidiHandler("C:\\Users\\CPURules\\Desktop\\RockBand\\Charting\\FeedBack0.97b\\Songs\\classless-act.mid");
            //handler = new MidiHandler("C:\\Users\\CPURules\\Downloads\\notes.mid");
            //handler = new MidiHandler("C:\\Users\\CPURules\\Desktop\\RockBand\\Charting\\FeedBack0.97b\\Songs\\dontfencemein.mid");
            Console.WriteLine(handler.FileName);

            Chart chart = handler.Read();

            var circle_actsg = new List<OverdriveActivationEvent> {
                new OverdriveActivationEvent(_beatsToMidiTick(4 * (33 - 1), chart.Timebase), (long)((4 * 9 + 2 + 0.1) * chart.Timebase)),
                new OverdriveActivationEvent(_beatsToMidiTick(4 * (70 - 1), chart.Timebase), (long)((4 * 8 + 0.1) * chart.Timebase)),
                new OverdriveActivationEvent(_beatsToMidiTick(4 * (94 - 1), chart.Timebase), (long)((4 * 8 + 2.5 + 0.1) * chart.Timebase)),
                new OverdriveActivationEvent(_beatsToMidiTick(4 * (123 - 1), chart.Timebase), (long)((4 * 6 + 0.5 + 0.1) * chart.Timebase))
            };

            chart.ScoreMeasuresFor("guitar", circle_actsg);
            
            ChartGenerator gen = new ChartGenerator(chart);
            gen.DrawChart("guitar", circle_actsg);
            gen.Save("C:\\Users\\CPURules\\Desktop\\WOWIE.png");

            System.Diagnostics.Process.Start("mspaint.exe", "C:\\Users\\CPURules\\Desktop\\WOWIE.png");
            
            Console.WriteLine();
        }
    }
}