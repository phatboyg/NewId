namespace MassTransit.NewIdProviders
{
    using System;


    public class DateTimeTickProvider :
        ITickProvider
    {
        public long Ticks
        {
            get { return DateTime.UtcNow.Ticks; }
        }
    }
}