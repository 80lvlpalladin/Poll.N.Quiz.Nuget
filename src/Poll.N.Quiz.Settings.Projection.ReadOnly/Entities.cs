// ReSharper disable once CheckNamespace
namespace Poll.N.Quiz.Settings.Projection.ReadOnly.Entities;

public record SettingsMetadata(string ServiceName, string[] EnvironmentNames) :
    IEqualityComparer<SettingsMetadata>
{
    public bool Equals(SettingsMetadata? x, SettingsMetadata? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.ServiceName == y.ServiceName &&
               x.EnvironmentNames.OrderBy(name => name).SequenceEqual(y.EnvironmentNames.OrderBy(name => name));
    }

    public int GetHashCode(SettingsMetadata obj) => HashCode.Combine(obj.ServiceName, HashCode.Combine(EnvironmentNames));
}
