using BenchmarkDotNet.Attributes;

namespace SusEquip.Tests.Performance
{
    [MemoryDiagnoser]
    [SimpleJob]
    public class ServiceCompositionBenchmarks
    {
        [Benchmark]
        public async Task SimpleCompositionBenchmark()
        {
            // Simple service composition benchmark
            await Task.Delay(1);
        }
        
        [Benchmark]
        public async Task ConcurrentCompositionBenchmark()
        {
            // Concurrent service composition benchmark
            var tasks = Enumerable.Range(0, 10).Select(_ => Task.Delay(1));
            await Task.WhenAll(tasks);
        }
    }
}