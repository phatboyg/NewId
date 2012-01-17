namespace NewId.Providers
{
    using System;

    public class DateTimeTickProvider :
        TickProvider
    {
        public long Ticks
        {
            get { return DateTime.UtcNow.Ticks; }
        }
    }
}