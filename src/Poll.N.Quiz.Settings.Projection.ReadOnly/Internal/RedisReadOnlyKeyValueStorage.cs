using System.Text.Json;
using StackExchange.Redis;

namespace Poll.N.Quiz.Settings.Projection.ReadOnly.Internal;

internal class RedisReadOnlyKeyValueStorage(string connectionString) : IReadOnlyKeyValueStorage
{
    private readonly ConnectionMultiplexer _connectionMultiplexer =
        ConnectionMultiplexer.Connect(connectionString, options =>{ options.AllowAdmin = true; });

    async Task<bool> IReadOnlyKeyValueStorage.IsEmptyAsync()
    {
        var database = _connectionMultiplexer.GetDatabase();
        var dbsize = await database.ExecuteAsync("DBSIZE");
        return dbsize.ToString() == "0";
    }

    async Task<T?> IReadOnlyKeyValueStorage.GetAsync<T>(string key) where T : class
    {
        var database = _connectionMultiplexer.GetDatabase();
        var redisValue = await database.StringGetAsync(key);

        if (string.IsNullOrWhiteSpace(redisValue))
            return null;

        if (typeof(T) == typeof(string))
        {
            return (T)(object) redisValue.ToString();
        }

        return JsonSerializer.Deserialize<T>(redisValue.ToString());
    }


    async Task<IReadOnlyCollection<string>> IReadOnlyKeyValueStorage.ListAllKeysAsync(CancellationToken cancellationToken)
    {
        List<string> result = [];

        foreach (var endpoint in _connectionMultiplexer.GetEndPoints())
        {
            if(cancellationToken.IsCancellationRequested)
                break;

            var server = _connectionMultiplexer.GetServer(endpoint);

            await foreach (var key in
                           server.KeysAsync(pattern: "*").WithCancellation(cancellationToken))
            {
                result.Add(key.ToString());
            }
        }

        return result;
    }
}
