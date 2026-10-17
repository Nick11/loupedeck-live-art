namespace Loupedeck.ArtRawEditorPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    // Works on any ART slider: point the mouse at it, turn = Shift+wheel, press = double-click (reset).
    public class ArtHoverWheelAdjustment : PluginDynamicAdjustment
    {
        private const Int32 TicksPerStep = 2;

        // ART's slider steps differ a lot relative to their ranges (WB temperature: 5 K of 60000 K) and the
        // slider under the pointer is unknown. Speed curve: precise (×1), fast (cubic up to ×30),
        // ultra fast (quadratic up to ×150). Measured on a Loupedeck Live: normal turning spans 0–300,
        // fast 300–1100 ticks/s, delivered in bursts, so the speed is averaged over a short window.
        // The multiplier only builds up gradually (sustained fast turns) and drops immediately, so a short
        // jerk never makes the slider jump while the user is looking at the image.
        private static readonly TimeSpan SpeedWindow = TimeSpan.FromMilliseconds(200);
        private const Double PreciseTicksPerSecond = 250, FastTicksPerSecond = 550, UltraTicksPerSecond = 1000;
        private const Double FastMultiplier = 30, UltraMultiplier = 150, MaxGrowthPerEvent = 1.25;

        private Double _multiplier = 1;

        private readonly Queue<(Int64 Timestamp, Int32 Ticks)> _recent = new();

        public ArtHoverWheelAdjustment()
            : base(displayName: "ART Hover Wheel", description: "Adjusts the ART slider under the mouse pointer", groupName: "Hover", hasReset: true)
        {
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff) =>
            ArtRawEditorPlugin.KeySender?.EnqueueWheel((Int32)Math.Round(diff * TicksPerStep * this.Multiplier(diff)));

        protected override void RunCommand(String actionParameter) =>
            ArtRawEditorPlugin.KeySender?.EnqueueDoubleClick();

        protected override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize) => "Hover";

        protected override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize) =>
            ArtIcons.Load("hover.wheel");

        private Double Multiplier(Int32 diff)
        {
            lock (this._recent)
            {
                this._recent.Enqueue((Stopwatch.GetTimestamp(), Math.Abs(diff)));
                while (Stopwatch.GetElapsedTime(this._recent.Peek().Timestamp) > SpeedWindow)
                {
                    this._recent.Dequeue();
                }

                var ticksPerSecond = this._recent.Sum(t => t.Ticks) / SpeedWindow.TotalSeconds;
                var target = ticksPerSecond <= FastTicksPerSecond
                    ? 1 + (FastMultiplier - 1) * Math.Pow(Ramp(ticksPerSecond, PreciseTicksPerSecond, FastTicksPerSecond), 3)
                    : FastMultiplier + (UltraMultiplier - FastMultiplier) * Math.Pow(Ramp(ticksPerSecond, FastTicksPerSecond, UltraTicksPerSecond), 2);
                this._multiplier = Math.Min(target, this._multiplier * MaxGrowthPerEvent);
                return this._multiplier;
            }
        }

        private static Double Ramp(Double value, Double from, Double to) => Math.Clamp((value - from) / (to - from), 0, 1);
    }
}
