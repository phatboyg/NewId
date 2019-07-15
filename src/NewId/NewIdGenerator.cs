namespace MassTransit
{
    public class NewIdGenerator :
        INewIdGenerator
    {
        readonly int _c;
        readonly int _d;

        readonly object _sync = new object();
        readonly ITickProvider _tickProvider;
        int _a;
        int _b;
        long _lastTick;

        ushort _sequence;


        public NewIdGenerator(ITickProvider tickProvider, IWorkerIdProvider workerIdProvider, IProcessIdProvider processIdProvider = null, int workerIndex = 0)
        {
            _tickProvider = tickProvider;

            byte[] workerId = workerIdProvider.GetWorkerId(workerIndex);

            _c = workerId[0] << 24 | workerId[1] << 16 | workerId[2] << 8 | workerId[3];

            if (processIdProvider != null)
            {
                var processId = processIdProvider.GetProcessId();
                _d = processId[0] << 24 | processId[1] << 16;
            }
            else
            {
                _d = workerId[4] << 24 | workerId[5] << 16;
            }
        }

        public NewId Next()
        {
            long ticks = _tickProvider.Ticks;
            int a;
            int b;
            ushort sequence;
            lock (_sync)
            {
                if (ticks > _lastTick)
                    UpdateTimestamp(ticks);
                else if (_sequence == 65535) // we are about to rollover, so we need to increment ticks
                    UpdateTimestamp(_lastTick + 1);

                sequence = _sequence++;

                a = _a;
                b = _b;
            }

            return new NewId(a, b, _c, _d | sequence);
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
