using DotNet.Testcontainers.Containers;
using Poll.N.Quiz.Settings.ProjectionStore.WriteOnly.Internal;
using Testcontainers.Redis;

namespace Poll.N.Quiz.NuGet.IntegrationTests.Projection.WriteOnly;

public class RedisWriteOnlyKeyValueStorageTests
{
    private static readonly RedisContainer RedisContainer = new RedisBuilder().Build();

    [Before(Class)]
    public static Task StartRedisContainerAsync() => RedisContainer.StartAsync();

    [After(Class)]
    public static Task StopRedisContainerAsync() => RedisContainer.StopAsync();

    [After(Test)]
    public Task FlushRedisAsync() => RedisContainer.ExecAsync(["redis-cli", "FLUSHALL"]);

    private static string ExtractValueFrom(ExecResult containerExecResult) =>
        containerExecResult.Stdout
            .TrimStart('\"')
            .TrimEnd(Environment.NewLine.ToCharArray())
            .TrimEnd('\"');

    [Test]
    [NotInParallel]
    public async Task SetAsync_WithoutExpiration_ShouldSetValueInRedis()
    {
        // Arrange
        IWriteOnlyKeyValueStorage storage =
            new RedisWriteOnlyKeyValueStorage(RedisContainer.GetConnectionString());
        var expectedKey = "test-key";
        var expectedValue = "test-value";

        // Act
        await storage.SetAsync(expectedKey, expectedValue, null);

        // Assert
        var containerOutput =
            await RedisContainer.ExecAsync(["redis-cli", "GET", expectedKey]);

        await Assert.That(expectedValue).IsEqualTo(ExtractValueFrom(containerOutput));
    }

    [Test]
    [NotInParallel]
    public async Task SetAsync_WithExpiration_ShouldSetTemporalValueInRedis()
    {
        // Arrange
        IWriteOnlyKeyValueStorage storage =
            new RedisWriteOnlyKeyValueStorage(RedisContainer.GetConnectionString());
        var expectedKey = "test-key";
        var expectedValue = "test-value";
        var expiration = TimeSpan.FromMilliseconds(500);

        // Act
        await storage.SetAsync(expectedKey, expectedValue, expiration);
        await Task.Delay(expiration);

        // Assert
        var containerOutput =
            await RedisContainer.ExecAsync(["redis-cli", "GET", expectedKey]);
        await Assert.That(string.IsNullOrWhiteSpace(ExtractValueFrom(containerOutput))).IsTrue();
    }

    [Test]
    [NotInParallel]
    public async Task ClearAsync_ShouldDeleteAllKeysInRedis()
    {
        // Arrange
        IWriteOnlyKeyValueStorage storage =
            new RedisWriteOnlyKeyValueStorage(RedisContainer.GetConnectionString());
        var expectedKey = "test-key";
        var expectedValue = "test-value";
        await storage.SetAsync(expectedKey, expectedValue, null);

        // Act
        await storage.ClearAsync();

        // Assert
        var containerOutput =
            await RedisContainer.ExecAsync(["redis-cli", "GET", expectedKey]);
        await Assert.That(string.IsNullOrWhiteSpace(ExtractValueFrom(containerOutput))).IsTrue();
    }

    [Test]
    [NotInParallel]
    public async Task RemoveBatchAsync_ShouldDeleteAllKeysWithPrefixInRedis()
    {
        // Arrange
        IWriteOnlyKeyValueStorage storage =
            new RedisWriteOnlyKeyValueStorage(RedisContainer.GetConnectionString());
        var keyPrefix = "test-key";
        var expectedKeys = new[] { "test-key1", "test-key2", "test-key3", "another-key" };
        foreach (var key in expectedKeys)
        {
            await storage.SetAsync(key, "test-value", null);
        }

        // Act
        await storage.RemoveBatchAsync(keyPrefix, CancellationToken.None);

        // Assert
        foreach (var key in expectedKeys)
        {
            var containerOutput =
                await RedisContainer.ExecAsync(["redis-cli", "GET", key]);

            if (key.StartsWith(keyPrefix, StringComparison.InvariantCulture))
                await Assert.That(string.IsNullOrWhiteSpace(ExtractValueFrom(containerOutput))).IsTrue();
            else
                await Assert.That(ExtractValueFrom(containerOutput)).IsEqualTo("test-value");
        }
    }

    [Test]
    [NotInParallel]
    public async Task RemoveBatchAsync_ShouldStopDeletingKeys_WhenCancellationRequested()
    {
        // Arrange
        IWriteOnlyKeyValueStorage storage =
            new RedisWriteOnlyKeyValueStorage(RedisContainer.GetConnectionString());
        var keyPrefix = "test-key";
        var expectedKeys = new[] { "test-key1", "test-key2", "test-key3", "another-key" };
        var expectedValue = "test-value";
        foreach (var key in expectedKeys)
        {
            await storage.SetAsync(key, expectedValue, null);
        }

        // Act
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await storage.RemoveBatchAsync(keyPrefix, cts.Token);

        // Assert
        foreach (var key in expectedKeys)
        {
            var containerOutput =
                await RedisContainer.ExecAsync(["redis-cli", "GET", key], CancellationToken.None);
            await Assert.That(ExtractValueFrom(containerOutput)).IsEqualTo(expectedValue);
        }
    }
}
