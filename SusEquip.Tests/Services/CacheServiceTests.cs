using Xunit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SusEquip.Data.Services;
using System;
using System.Threading.Tasks;

namespace SusEquip.Tests.Services
{
    public class CacheServiceTests
    {
        private readonly CacheService _cacheService;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ILogger<CacheService>> _mockLogger;

        public CacheServiceTests()
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _mockLogger = new Mock<ILogger<CacheService>>();
            _cacheService = new CacheService(_memoryCache, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAsync_WhenKeyNotExists_ShouldReturnNull()
        {
            // Act
            var result = await _cacheService.GetAsync<string>("non-existent-key");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_ThenGet_ShouldReturnValue()
        {
            // Arrange
            const string key = "test-key";
            const string value = "test-value";
            var expiration = TimeSpan.FromMinutes(5);

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task SetAsync_WithComplexObject_ShouldReturnObject()
        {
            // Arrange
            const string key = "complex-key";
            var value = new { Name = "Test", Count = 42, Items = new[] { "A", "B", "C" } };
            var expiration = TimeSpan.FromMinutes(10);

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<object>(key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task RemoveAsync_ExistingKey_ShouldRemoveValue()
        {
            // Arrange
            const string key = "remove-test-key";
            const string value = "remove-test-value";
            var expiration = TimeSpan.FromMinutes(5);

            await _cacheService.SetAsync(key, value, expiration);

            // Act
            await _cacheService.RemoveAsync(key);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RemoveAsync_NonExistentKey_ShouldNotThrow()
        {
            // Act & Assert - Should not throw exception
            await _cacheService.RemoveAsync("non-existent-key");
            
            // If we reach here, the test passes
            Assert.True(true);
        }

        [Fact]
        public async Task SetAsync_WithZeroExpiration_ShouldStillCache()
        {
            // Arrange
            const string key = "zero-expiration-key";
            const string value = "zero-expiration-value";
            var expiration = TimeSpan.Zero;

            // Act
            await _cacheService.SetAsync(key, value, expiration);
            var result = await _cacheService.GetAsync<string>(key);

            // Assert
            // Even with zero expiration, the value should be cached temporarily
            Assert.Equal(value, result);
        }

        [Fact]
        public async Task GetAsync_WithWrongType_ShouldReturnDefault()
        {
            // Arrange
            const string key = "type-mismatch-key";
            const string stringValue = "string-value";
            var expiration = TimeSpan.FromMinutes(5);

            await _cacheService.SetAsync(key, stringValue, expiration);

            // Act - Try to get as int instead of string
            var result = await _cacheService.GetAsync<int>(key);

            // Assert - Should return default value for int (0) due to type mismatch
            Assert.Equal(default(int), result);
        }

        [Fact]
        public async Task MultipleOperations_ShouldWorkIndependently()
        {
            // Arrange
            var expiration = TimeSpan.FromMinutes(10);
            
            // Act
            await _cacheService.SetAsync("key1", "value1", expiration);
            await _cacheService.SetAsync("key2", 42, expiration);
            await _cacheService.SetAsync("key3", new[] { "A", "B", "C" }, expiration);

            var result1 = await _cacheService.GetAsync<string>("key1");
            var result2 = await _cacheService.GetAsync<int>("key2");
            var result3 = await _cacheService.GetAsync<string[]>("key3");

            // Remove one key
            await _cacheService.RemoveAsync("key2");
            var removedResult = await _cacheService.GetAsync<int>("key2");

            // Assert
            Assert.Equal("value1", result1);
            Assert.Equal(42, result2);
            Assert.Equal(new[] { "A", "B", "C" }, result3);
            Assert.Equal(default(int), removedResult); // Should be 0 after removal
        }
    }
}