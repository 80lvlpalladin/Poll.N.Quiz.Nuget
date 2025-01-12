namespace Poll.N.Quiz.Settings.Projection.ReadOnly.Internal;

internal interface IReadOnlyKeyValueStorage
{
    internal Task<bool> IsEmptyAsync();
    internal Task<T?> GetAsync<T>(string key) where T : class;
    internal Task<IReadOnlyCollection<string>> ListAllKeysAsync
        (CancellationToken cancellationToken);
}
