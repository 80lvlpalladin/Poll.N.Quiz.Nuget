namespace Poll.N.Quiz.Settings.ProjectionStore.ReadOnly.Internal;

internal interface IReadOnlyKeyValueStorage
{
    Task<bool> IsEmptyAsync();
    Task<T?> GetAsync<T>(string key) where T : class;
    Task<IReadOnlyCollection<string>> ListKeysAsync
        (string keyPattern = "*", CancellationToken cancellationToken = default);
}
