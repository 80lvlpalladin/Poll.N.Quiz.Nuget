namespace Poll.N.Quiz.Settings.ProjectionStore.WriteOnly.Internal;

public class SettingsProjectionStoreOptions
{
    public const string SectionName = "SettingsProjectionStore";

    public ushort ExpirationTimeHours { get; init; }
}
