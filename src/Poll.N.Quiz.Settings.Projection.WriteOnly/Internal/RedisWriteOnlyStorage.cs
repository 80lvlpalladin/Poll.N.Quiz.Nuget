using System.Text.Json;
using StackExchange.Redis;

namespace Poll.N.Quiz.Settings.Projection.WriteOnly.Internal;

internal class RedisWriteOnlyStorage(string connectionString) : IWriteOnlyKeyValueStorage
{
    private readonly ConnectionMultiplexer _connectionMultiplexer =
        ConnectionMultiplexer.Connect(connectionString, options =>{ options.AllowAdmin = true; });

    async Task IWriteOnlyKeyValueStorage.SetAsync<T>(string key, T value, TimeSpan? expiry)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var jsonValueString = JsonSerializer.Serialize(value);
        await database.StringSetAsync(key, jsonValueString, expiry);
    }

    async Task IWriteOnlyKeyValueStorage.ClearAsync()
    {
        foreach (var endpoint in _connectionMultiplexer.GetEndPoints())
        {
            var server = _connectionMultiplexer.GetServer(endpoint);
            await server.FlushAllDatabasesAsync();
        }
    }

    async Task IWriteOnlyKeyValueStorage.RemoveBatchAsync
        (string keyPrefix, CancellationToken cancellationToken)
    {
        var database = _connectionMultiplexer.GetDatabase();
        foreach (var endpoint in _connectionMultiplexer.GetEndPoints())
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var server = _connectionMultiplexer.GetServer(endpoint);

            await foreach (var key in server
                               .KeysAsync(pattern: $"{keyPrefix}*")
                               .WithCancellation(cancellationToken))
            {
                await database.KeyDeleteAsync(key);
            }
        }
    }
}
