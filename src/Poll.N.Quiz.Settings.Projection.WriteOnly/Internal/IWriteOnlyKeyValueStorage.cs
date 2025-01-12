namespace Poll.N.Quiz.Settings.Projection.WriteOnly.Internal;

internal interface IWriteOnlyKeyValueStorage
{
    internal Task SetAsync<T>(string key, T value, TimeSpan? expiry);
    internal Task ClearAsync();
    internal Task RemoveBatchAsync
        (string keyPrefix, CancellationToken cancellationToken);
}
