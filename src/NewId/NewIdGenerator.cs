using System;

namespace MassTransit
{
    using System.Threading;

    public class NewIdGenerator :
        INewIdGenerator
    {
        readonly int _c;
        readonly int _d;
        readonly short _gb;
        readonly short _gc;
        readonly ITickProvider _tickProvider;
        int _a;
        int _b;
        long _lastTick;
        int _sequence;

        SpinLock _spinLock;

        public NewIdGenerator(ITickProvider tickProvider, IWorkerIdProvider workerIdProvider, IProcessIdProvider processIdProvider = null, int workerIndex = 0)
        {
            _tickProvider = tickProvider;

            _spinLock = new SpinLock(false);

            var workerId = workerIdProvider.GetWorkerId(workerIndex);

            _c = (workerId[0] << 24) | (workerId[1] << 16) | (workerId[2] << 8) | workerId[3];

            if (processIdProvider != null)
            {
                var processId = processIdProvider.GetProcessId();
                _d = (processId[0] << 24) | (processId[1] << 16);
            }
            else
            {
                _d = (workerId[4] << 24) | (workerId[5] << 16);
            }

            _gb = (short) _c;
            _gc = (short) (_c >> 16);
        }

        public NewId Next()
        {
            var ticks = _tickProvider.Ticks;

            int a;
            int b;
            int sequence;

            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                if (ticks > _lastTick)
                    UpdateTimestamp(ticks);
                else if (_sequence == 65535) // we are about to rollover, so we need to increment ticks
                    UpdateTimestamp(_lastTick + 1);

                sequence = _sequence++;

                a = _a;
                b = _b;
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }

            return new NewId(a, b, _c, _d | sequence);
        }

        public Guid NextGuid()
        {
            var ticks = _tickProvider.Ticks;

            int a;
            int b;
            int sequence;

            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                if (ticks > _lastTick)
                    UpdateTimestamp(ticks);
                else if (_sequence == 65535) // we are about to rollover, so we need to increment ticks
                    UpdateTimestamp(_lastTick + 1);

                sequence = _sequence++;

                a = _a;
                b = _b;
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }

            var d = (byte) (b >> 8);
            var e = (byte) b;
            var f = (byte) (a >> 24);
            var g = (byte) (a >> 16);
            var h = (byte) (a >> 8);
            var i = (byte) a;
            var j = (byte) (b >> 24);
            var k = (byte) (b >> 16);

            return new Guid(_d | sequence, _gb, _gc, d, e, f, g, h, i, j, k);
        }

        public ArraySegment<NewId> Next(NewId[] ids, int index, int count)
        {
            long ticks = _tickProvider.Ticks;
            var lockTaken = false;
            try
            {
                _spinLock.Enter(ref lockTaken);

                if (ticks > _lastTick)
                    UpdateTimestamp(ticks);

                int limit = index + count;
                for (int i = index; i < limit; i++)
                {
                    if (_sequence == 65535) // we are about to rollover, so we need to increment ticks
                        UpdateTimestamp(_lastTick + 1);

                    ids[i] = new NewId(_a, _b, _c, _d | _sequence++);
                }
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }

            return new ArraySegment<NewId>(ids, index, count);
        }


        void UpdateTimestamp(long tick)
        {
            _b = (int) (tick & 0xFFFFFFFF);
            _a = (int) (tick >> 32);

            _sequence = 0;
            _lastTick = tick;
        }
    }
}
