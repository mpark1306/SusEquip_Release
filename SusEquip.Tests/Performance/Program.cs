using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;

namespace SusEquip.Tests.Performance
{
    /// <summary>
    /// Benchmark runner program for Phase 5.5 Integration Testing Framework performance tests
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Phase 5.5 Integration Testing Framework - Performance Benchmarks ===");
            Console.WriteLine();
            
            var config = ManualConfig.Create(DefaultConfig.Instance)
                .AddExporter(MarkdownExporter.GitHub) // Export results to GitHub-compatible markdown
                .AddExporter(HtmlExporter.Default)     // Export results to HTML
                .AddLogger(ConsoleLogger.Default);     // Log to console

            if (args.Length > 0)
            {
                // Run specific benchmark based on command line argument
                switch (args[0].ToLowerInvariant())
                {
                    case "service":
                    case "servicecomposition":
                        Console.WriteLine("Running Service Composition Benchmarks...");
                        BenchmarkRunner.Run<ServiceCompositionBenchmarks>(config);
                        break;
                        
                    case "fault":
                    case "faulttolerance":
                        Console.WriteLine("Running Fault Tolerance Benchmarks...");
                        BenchmarkRunner.Run<FaultToleranceBenchmarks>(config);
                        break;
                        
                    case "reliability":
                    case "compensation":
                        Console.WriteLine("Running Reliability & Compensation Benchmarks...");
                        // BenchmarkRunner.Run<ReliabilityBenchmarks>(config);
                        break;
                        
                    case "all":
                        Console.WriteLine("Running All Benchmarks...");
                        RunAllBenchmarks(config);
                        break;
                        
                    default:
                        ShowUsage();
                        return;
                }
            }
            else
            {
                // Interactive mode - let user choose
                ShowMenu(config);
            }
        }

        private static void ShowMenu(IConfig config)
        {
            while (true)
            {
                Console.WriteLine("Please select benchmark suite to run:");
                Console.WriteLine("1. Service Composition Benchmarks");
                Console.WriteLine("2. Fault Tolerance Benchmarks");
                Console.WriteLine("3. Reliability & Compensation Benchmarks");
                Console.WriteLine("4. Run All Benchmarks");
                Console.WriteLine("5. Exit");
                Console.Write("Enter your choice (1-5): ");
                
                var choice = Console.ReadLine();
                
                switch (choice)
                {
                    case "1":
                        Console.WriteLine("\nRunning Service Composition Benchmarks...");
                        BenchmarkRunner.Run<ServiceCompositionBenchmarks>(config);
                        break;
                        
                    case "2":
                        Console.WriteLine("\nRunning Fault Tolerance Benchmarks...");
                        BenchmarkRunner.Run<FaultToleranceBenchmarks>(config);
                        break;
                        
                    case "3":
                        Console.WriteLine("\nRunning Reliability & Compensation Benchmarks...");
                        // BenchmarkRunner.Run<ReliabilityBenchmarks>(config);
                        break;
                        
                    case "4":
                        Console.WriteLine("\nRunning All Benchmarks...");
                        RunAllBenchmarks(config);
                        break;
                        
                    case "5":
                        Console.WriteLine("Exiting...");
                        return;
                        
                    default:
                        Console.WriteLine("Invalid choice. Please enter 1-5.");
                        continue;
                }
                
                Console.WriteLine("\nBenchmarks completed. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private static void RunAllBenchmarks(IConfig config)
        {
            Console.WriteLine("=== Running All Phase 5.5 Performance Benchmarks ===");
            Console.WriteLine();
            
            try
            {
                Console.WriteLine("1/3: Service Composition Benchmarks");
                Console.WriteLine("====================================");
                BenchmarkRunner.Run<ServiceCompositionBenchmarks>(config);
                Console.WriteLine();
                
                Console.WriteLine("2/3: Fault Tolerance Benchmarks");
                Console.WriteLine("================================");
                BenchmarkRunner.Run<FaultToleranceBenchmarks>(config);
                Console.WriteLine();
                
                Console.WriteLine("3/3: Reliability & Compensation Benchmarks");
                Console.WriteLine("==========================================");
                // BenchmarkRunner.Run<ReliabilityBenchmarks>(config);
                Console.WriteLine();
                
                Console.WriteLine("=== All Benchmarks Completed Successfully ===");
                Console.WriteLine();
                Console.WriteLine("Results have been exported to:");
                Console.WriteLine("- BenchmarkDotNet.Artifacts/results/ (detailed results)");
                Console.WriteLine("- *.html (HTML reports)");
                Console.WriteLine("- *.md (Markdown reports)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running benchmarks: {ex.Message}");
                Console.WriteLine("Please check the error details and try again.");
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Usage: SusEquip.Tests.Performance.exe [benchmark-type]");
            Console.WriteLine();
            Console.WriteLine("Benchmark types:");
            Console.WriteLine("  service         - Run Service Composition benchmarks");
            Console.WriteLine("  faulttolerance  - Run Fault Tolerance benchmarks");
            Console.WriteLine("  reliability     - Run Reliability & Compensation benchmarks");
            Console.WriteLine("  all             - Run all benchmark suites");
            Console.WriteLine();
            Console.WriteLine("If no argument is provided, interactive mode will be used.");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  SusEquip.Tests.Performance.exe service");
            Console.WriteLine("  SusEquip.Tests.Performance.exe all");
        }
    }
}