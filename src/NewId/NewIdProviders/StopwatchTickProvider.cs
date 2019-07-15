namespace MassTransit.NewIdProviders
{
    using System;
    using System.Diagnostics;


    public class StopwatchTickProvider :
        ITickProvider
    {
        readonly Stopwatch _stopwatch;
        readonly DateTime _start;

        public StopwatchTickProvider()
        {
            _start = DateTime.UtcNow;
            _stopwatch = Stopwatch.StartNew();
        }

        public long Ticks => _start.AddTicks(_stopwatch.Elapsed.Ticks).Ticks;
    }
}