namespace MassTransit.NewIdProviders
{
    using System;
    using System.Diagnostics;


    public class StopwatchTickProvider :
        ITickProvider
    {

        readonly Stopwatch _stopwatch;
        DateTime _start;

        public StopwatchTickProvider()
        {
            _start = DateTime.UtcNow;
            _stopwatch = Stopwatch.StartNew();
        }

        private static readonly double StopwatchTickFrequency;

        static StopwatchTickProvider()
        {
            if (Stopwatch.IsHighResolution)
            {
                StopwatchTickFrequency = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;
            }
            else
            {
                StopwatchTickFrequency = 1.0;
            }
        }

        public long Ticks
        {
            get { return _start.AddTicks(GetStopwatchTicksAsTimespanTicks()).Ticks; }
        }

        // Stopwatch and Timespan ticks are not the same
        long GetStopwatchTicksAsTimespanTicks()
        {
            long rawTicks = _stopwatch.ElapsedTicks;
            if (Stopwatch.IsHighResolution)
                return (long)(rawTicks * StopwatchTickFrequency);
            else
                return rawTicks;
        }
    }
}