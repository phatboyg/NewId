using System;
using BenchmarkDotNet.Attributes;

namespace MassTransit.Benchmarks
{
    [ClrJob(true), CoreJob]
    [LegacyJitX64Job, RyuJitX64Job]
    [MemoryDiagnoser, GcServer(true), GcForce]
    public class Benchmarker
    {
        [Benchmark(Baseline = true, Description = "Next")]
        public NewId GetNext() => NewId.Next();

        [Benchmark(Description = "NextGuid")]
        public Guid GetNextGuid() => NewId.NextGuid();
    }
}
