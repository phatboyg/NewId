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

        public long Ticks
        {
            // Stopwatch and Timespan ticks are not the same, use Stompwatch.ElapsedMilliseconds which does a proper conversion 
            get { return _start.AddMilliseconds(_stopwatch.ElapsedMilliseconds).Ticks; }
        }
    }
}