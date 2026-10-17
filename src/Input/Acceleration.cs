namespace Loupedeck.ArtRawEditorPlugin
{
    using System;
    using System.Diagnostics;

    // Fast consecutive ticks in the same direction on the same slider increase the step size.
    internal sealed class Acceleration
    {
        private const Int32 MaxMultiplier = 4;
        private static readonly TimeSpan FastTick = TimeSpan.FromMilliseconds(100);

        private readonly Object _lock = new();
        private String _lastSlider;
        private Int32 _lastDirection;
        private Int64 _lastTick;
        private Int32 _fastTicks;

        public Int32 Steps(String sliderId, Int32 diff)
        {
            lock (this._lock)
            {
                var direction = Math.Sign(diff);
                var fast = sliderId == this._lastSlider && direction == this._lastDirection
                    && Stopwatch.GetElapsedTime(this._lastTick) < FastTick;

                this._fastTicks = fast ? this._fastTicks + 1 : 0;
                this._lastSlider = sliderId;
                this._lastDirection = direction;
                this._lastTick = Stopwatch.GetTimestamp();

                return diff * Math.Min(1 + this._fastTicks / 3, MaxMultiplier);
            }
        }
    }
}
