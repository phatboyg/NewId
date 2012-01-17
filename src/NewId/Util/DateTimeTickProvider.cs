namespace NewId.Util
{
    using System;

    public class DateTimeTickProvider :
        ITickProvider
    {
        int _lastTickCount;
        DateTime _lastUtcNow = DateTime.MinValue;

        public long Ticks
        {
            get
            {
                int tickCount = Environment.TickCount;
                if (tickCount != _lastTickCount)
                {
                    _lastTickCount = tickCount;
                    _lastUtcNow = DateTime.UtcNow;
                }

                return _lastUtcNow.Ticks;
            }
        }
    }
}