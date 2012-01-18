namespace NewId
{
    public class NewIdGenerator
    {
        readonly int _c;
        readonly int _d;
        readonly byte[] _networkId;

        readonly object _sync = new object();
        readonly ITickProvider _tickProvider;
        readonly int _workerIndex;
        int _a;
        int _b;
        long _lastTick;

        ushort _sequence;


        public NewIdGenerator(ITickProvider tickProvider, IWorkerIdProvider workerIdProvider, int workerIndex = 0)
        {
            _workerIndex = workerIndex;
            _networkId = workerIdProvider.GetWorkerId(_workerIndex);
            _tickProvider = tickProvider;

            _c = _networkId[0] << 24 | _networkId[1] << 16 | _networkId[2] << 8 | _networkId[3];
            _d = _networkId[4] << 24 | _networkId[5] << 16;
        }

        public NewId Next()
        {
            ushort sequence;

            long ticks = _tickProvider.Ticks;
            lock (_sync)
            {
                if (ticks > _lastTick)
                {
                    UpdateTimestamp(ticks);
                }

                if (_sequence == 65535) // we are about to rollover, so we need to increment ticks
                {
                    UpdateTimestamp(_lastTick + 1);
                }

                sequence = _sequence++;
            }

            return new NewId(_a, _b, _c, _d | sequence);
        }

        void UpdateTimestamp(long tick)
        {
            _lastTick = tick;
            _sequence = 0;

            _a = (int) (tick >> 32);
            _b = (int) (tick & 0xFFFFFFFF);
        }
    }
}