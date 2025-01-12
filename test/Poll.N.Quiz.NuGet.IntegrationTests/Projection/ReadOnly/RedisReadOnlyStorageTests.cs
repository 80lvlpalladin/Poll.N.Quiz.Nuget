using Bogus;
using Poll.N.Quiz.Settings.Projection.ReadOnly.Internal;
using Testcontainers.Redis;

namespace Poll.N.Quiz.NuGet.IntegrationTests.Projection.ReadOnly;

public class RedisReadOnlyStorageTests
{
    private static readonly RedisContainer RedisContainer = new RedisBuilder().Build();

    [Before(Class)]
    public static Task StartRedisContainerAsync() => RedisContainer.StartAsync();

    [After(Class)]
    public static Task StopRedisContainerAsync() => RedisContainer.StopAsync();


    [After(Test)]
    public Task FlushRedisAsync() =>
        RedisContainer.ExecAsync(["redis-cli", "FLUSHALL"]);


    [Test]
    [NotInParallel]
    public async Task IsEmptyAsync_ReturnsTrue_WhenNoKeysExistInRedis()
    {
        // Arrange
        IReadOnlyKeyValueStorage storage =
            new RedisReadOnlyStorage(RedisContainer.GetConnectionString());

        // Act
        var isEmpty = await storage.IsEmptyAsync();

        // Assert
        await Assert.That(isEmpty).IsTrue();
    }

    [Test]
    [NotInParallel]
    public async Task IsEmptyAsync_ReturnsFalse_WhenKeysExistInRedis()
    {
        // Arrange
        IReadOnlyKeyValueStorage storage =
            new RedisReadOnlyStorage(RedisContainer.GetConnectionString());
        var command = new List<string>{ "redis-cli", "SET", "key1", "value1" };
        await RedisContainer.ExecAsync(command);

        // Act
        var isEmpty = await storage.IsEmptyAsync();

        // Assert
        await Assert.That(isEmpty).IsFalse();
    }

    [Test]
    [NotInParallel]
    public async Task GetAsync_ReturnsNull_WhenKeyDoesNotExistInRedis()
    {
        // Arrange
        IReadOnlyKeyValueStorage storage =
            new RedisReadOnlyStorage(RedisContainer.GetConnectionString());

        // Act
        var result = await storage.GetAsync<string>("service1__environment1");

        // Assert
        await Assert.That(result).IsNull();
    }

    [Test]
    [NotInParallel]
    public async Task GetAsync_ReturnsValue_WhenKeyExistsInRedis()
    {
        // Arrange
        IReadOnlyKeyValueStorage storage =
            new RedisReadOnlyStorage(RedisContainer.GetConnectionString());
        var expectedValue = "{ 'field1' : 'value1' }";
        var command = new List<string>{ "redis-cli", "SET", "service1__environment1", expectedValue };
        await RedisContainer.ExecAsync(command);

        // Act
        var actualValue = await storage.GetAsync<string>("service1__environment1");

        // Assert
        await Assert.That(expectedValue).IsEqualTo(actualValue);
    }

    [Test]
    [NotInParallel]
    public async Task ListAllKeysAsync_ReturnsEmptyCollection_WhenNoKeysExistInRedis()
    {
        // Arrange
        IReadOnlyKeyValueStorage storage =
            new RedisReadOnlyStorage(RedisContainer.GetConnectionString());

        // Act
        var keys = await storage.ListAllKeysAsync(CancellationToken.None);

        // Assert
        await Assert.That(keys).IsEmpty();
    }

    [Test]
    [NotInParallel]
    public async Task ListAllKeysAsync_ReturnsAllKeys_WhenKeysExistInRedis()
    {
        // Arrange
        IReadOnlyKeyValueStorage storage =
            new RedisReadOnlyStorage(RedisContainer.GetConnectionString());
        var keyValuePairs = FakeData.GenerateKeyValuePairs().ToArray();

        foreach (var kv in keyValuePairs)
        {
            var command = new List<string>{ "redis-cli", "SET", kv.Key, kv.Value };
            await RedisContainer.ExecAsync(command);
        }

        // Act
        var actualKeys = (await storage.ListAllKeysAsync(CancellationToken.None))
            .OrderBy(k => k).ToArray();

        //Assert
        var expectedKeys = keyValuePairs.Select(kv => kv.Key)
            .OrderBy(k => k).ToArray();

        await Assert.That(actualKeys.Length).IsEqualTo(expectedKeys.Length);

        for (var i = 0; i < actualKeys.Length; i++)
        {
            await Assert.That(expectedKeys[i]).IsEqualTo(actualKeys[i]);
        }

        await Assert.That(expectedKeys).IsEquivalentTo(actualKeys.OrderBy(k => k));
    }
}

static file class FakeData
{
    private static readonly Faker Faker = new();

    internal static IEnumerable<KeyValuePair<string, string>> GenerateKeyValuePairs()
    {
        var length = Faker.Random.Int(1, 10);

        for (var i = 0; i < length; i++)
        {
            yield return new KeyValuePair<string, string>(
                "key" + i,
                $$"""{ 'field{{i}}' : 'value{{i}}' }""");
        }
    }
}
