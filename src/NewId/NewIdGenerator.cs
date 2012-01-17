namespace NewId
{
    using System;
    using Util;


    public class NewIdGenerator
    {
        readonly byte[] _networkId;

        readonly object _sync = new object();
        readonly ITickProvider _tickProvider;
        long _lastTick;

        ushort _sequence;
        byte[] _timestamp;


        public NewIdGenerator(byte[] networkId, ITickProvider tickProvider)
        {
            _networkId = networkId;
            _tickProvider = tickProvider;
        }

        public NewId Next()
        {
            byte[] timestampBytes;
            ushort sequence;

            long ticks = _tickProvider.Ticks;
            lock (_sync)
            {
                if (ticks > _lastTick)
                {
                    _lastTick = ticks;
                    _timestamp = GetTimestamp();
                    _sequence = 0;
                }

                if (_sequence == 65535) // we are about to rollover, so we need to increment ticks
                {
                    _lastTick++;
                    _timestamp = GetTimestamp();
                    _sequence = 0;
                }

                timestampBytes = _timestamp;
                sequence = _sequence++;
            }

            var bytes = new byte[16];
            byte[] sequenceBytes = BitConverter.GetBytes(sequence);

            Buffer.BlockCopy(timestampBytes, 0, bytes, 0, 14);

            bytes[14] = sequenceBytes[1];
            bytes[15] = sequenceBytes[0];

            var result = new NewId(bytes);

            return result;
        }

        byte[] GetTimestamp()
        {
            byte[] timestamp = BitConverter.GetBytes(_lastTick);

            var result = new byte[14];
            result[0] = timestamp[7];
            result[1] = timestamp[6];
            result[2] = timestamp[5];
            result[3] = timestamp[4];
            result[4] = timestamp[3];
            result[5] = timestamp[2];
            result[6] = timestamp[1];
            result[7] = timestamp[0];

            Buffer.BlockCopy(_networkId, 0, result, 8, 6);

            return result;
        }
    }
}