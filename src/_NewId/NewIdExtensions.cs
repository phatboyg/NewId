namespace MassTransit
{
    using System;


    public static class NewIdExtensions
    {
        public static NewId ToNewId(this Guid guid)
        {
            return new NewId(guid.ToByteArray());
        }
    }
}