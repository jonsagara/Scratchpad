using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace SequentialGuids;

[SimpleJob(RuntimeMoniker.Net60)]
[MemoryDiagnoser]
public class SequentialGuidHarness
{
    [Benchmark(Baseline = true)]
    public void GetSequentialGuid_Original()
    {
        var seqGuid = SequentialGuid_Original.GenerateComb();
    }

    [Benchmark]
    public void GetSequentialGuid_Optimized()
    {
        var seqGuid = SequentialGuid_Optimized.GenerateComb();
    }
}
