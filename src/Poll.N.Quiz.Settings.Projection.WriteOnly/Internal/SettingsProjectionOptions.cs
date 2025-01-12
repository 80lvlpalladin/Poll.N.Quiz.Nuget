namespace Poll.N.Quiz.Settings.Projection.WriteOnly.Internal;

internal class SettingsProjectionOptions
{
    internal const string SectionName = "SettingsProjection";

    internal ushort ExpirationTimeHours { get; init; }
}
