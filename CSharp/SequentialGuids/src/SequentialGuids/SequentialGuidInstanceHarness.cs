using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace SequentialGuids;

[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class SequentialGuidInstanceHarness
{
    private SequentialGuid_Original _instanceOriginal = new SequentialGuid_Original();
    private SequentialGuid_Optimized _instanceOptimized = new SequentialGuid_Optimized();

    [Benchmark(Baseline = true)]
    public void GetSequentialGuidInstance_Original()
    {
        _instanceOriginal++;
    }

    [Benchmark]
    public void GetSequentialGuidInstance_Optimized()
    {
        _instanceOptimized++;
    }
}
