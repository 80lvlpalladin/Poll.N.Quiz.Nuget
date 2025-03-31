namespace Poll.N.Quiz.Settings.ProjectionStore.WriteOnly.Internal;

internal class SettingsProjectionStoreOptions
{
    internal const string SectionName = "SettingsProjectionStore";

    internal ushort ExpirationTimeHours { get; init; }
}
