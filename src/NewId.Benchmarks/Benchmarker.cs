using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace MassTransit.Benchmarks
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser, GcServer(true), GcForce]
    public class Benchmarker
    {
        [Benchmark(Baseline = true, Description = "Next")]
        public NewId GetNext()
        {
            return NewId.Next();
        }

        [Benchmark(Description = "NextGuid")]
        public Guid GetNextGuid()
        {
            return NewId.NextGuid();
        }
    }
}
