using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SusEquip.Data.Services.ErrorHandling;
using SusEquip.Data.Interfaces.Services;
using SusEquip.Data.Services;

namespace SusEquip.Tests.Infrastructure
{
    /// <summary>
    /// Base class for integration tests providing dependency injection and service configuration
    /// </summary>
    public abstract class IntegrationTestBase : IDisposable
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected IServiceScope ServiceScope { get; private set; }
        protected ILogger Logger { get; private set; }

        protected IntegrationTestBase()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            ServiceScope = ServiceProvider.CreateScope();
            Logger = ServiceScope.ServiceProvider.GetRequiredService<ILogger<IntegrationTestBase>>();
        }

        /// <summary>
        /// Configure services for the test environment
        /// </summary>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Add configuration
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Test.json", optional: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            // Add error handling services
            services.AddSingleton<ICircuitBreaker, CircuitBreakerService>();
            services.AddSingleton<IRetryPolicy, RetryPolicyService>();
            services.AddSingleton<ICompensationCoordinator, CompensationCoordinatorService>();
            services.AddSingleton<FaultTolerantService>();

            // Add equipment services with mock implementations for testing
            ConfigureEquipmentServices(services);

            // Add additional test-specific services
            ConfigureTestServices(services);
        }

        /// <summary>
        /// Configure equipment services - override in derived classes for specific test scenarios
        /// </summary>
        protected virtual void ConfigureEquipmentServices(IServiceCollection services)
        {
            // Default mock implementations - can be overridden in specific test classes
            var mockEquipmentService = new Mock<IEquipmentService>();
            services.AddSingleton(mockEquipmentService.Object);
            services.AddSingleton(mockEquipmentService);
        }

        /// <summary>
        /// Configure additional test-specific services - override in derived classes
        /// </summary>
        protected virtual void ConfigureTestServices(IServiceCollection services)
        {
            // Override in derived classes for specific test needs
        }

        /// <summary>
        /// Get a service from the test service provider
        /// </summary>
        protected T GetService<T>() where T : notnull
        {
            return ServiceScope.ServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Get a mock service from the test service provider
        /// </summary>
        protected Mock<T> GetMock<T>() where T : class
        {
            return ServiceScope.ServiceProvider.GetRequiredService<Mock<T>>();
        }

        /// <summary>
        /// Create a new service scope for isolated testing
        /// </summary>
        protected IServiceScope CreateScope()
        {
            return ServiceProvider.CreateScope();
        }

        public virtual void Dispose()
        {
            ServiceScope?.Dispose();
            if (ServiceProvider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }
        }
    }
}