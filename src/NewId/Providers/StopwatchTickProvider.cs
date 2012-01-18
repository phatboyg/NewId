namespace NewId.Providers
{
    using System;
    using System.Diagnostics;

    public class StopwatchTickProvider :
        ITickProvider
    {
        DateTime _start;
        Stopwatch _stopwatch;

        public StopwatchTickProvider()
        {
            _start = DateTime.UtcNow;
            _stopwatch = Stopwatch.StartNew();
        }

        public long Ticks
        {
            get { return _start.AddTicks(_stopwatch.ElapsedTicks).Ticks; }
        }
    }
}