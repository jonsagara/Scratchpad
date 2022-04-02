//using BenchmarkDotNet.Attributes;
//using BenchmarkDotNet.Jobs;

//namespace StringHelpers.Harnesses;

//[SimpleJob(RuntimeMoniker.Net60)]
//[MemoryDiagnoser]
//public class StringEncodingHelperHarness
//{
//    private readonly Guid Id = Guid.Parse("b8ebb77b-9e77-4b62-a744-c05f308f55d7");


//    [Benchmark(Baseline = true)]
//    public void ToUrlEncodedString()
//    {
//        var encoded = StringEncodingHelper.ToUrlEncodedString(Id);
//    }

//    [Benchmark]
//    public void ToUrlEncodedString_Marshal()
//    {
//        var encoded = StringEncodingHelper.ToUrlEncodedString_Marshal(Id);
//    }
//}
