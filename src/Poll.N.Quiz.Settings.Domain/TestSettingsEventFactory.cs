using Poll.N.Quiz.Settings.Domain.ValueObjects;

namespace Poll.N.Quiz.Settings.Domain;

public static class TestSettingsEventFactory
{
    public static IEnumerable<SettingsEvent> CreateSettingsEvents()
    {
        var now = Convert.ToUInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        var service1 = "service1";
        var environment1 = "environment1";
        var service2 = "service2";
        var environment2 = "environment2";

        yield return new SettingsEvent
            (SettingsEventType.CreateEvent, new SettingsMetadata(service1, environment1), now, 0, TrimJson(InitialSettings1));

        yield return new SettingsEvent
            (SettingsEventType.CreateEvent, new SettingsMetadata(service2, environment2),now + 100, 0, TrimJson(InitialSettings2));

        yield return new SettingsEvent
            (SettingsEventType.UpdateEvent, new SettingsMetadata(service1, environment1),now + 200, 1, TrimJson(Settings1Patch1));

        yield return new SettingsEvent
            (SettingsEventType.UpdateEvent,new SettingsMetadata(service1, environment1),now + 300, 2, TrimJson(Settings1Patch2));

        yield return new SettingsEvent
            (SettingsEventType.UpdateEvent, new SettingsMetadata(service1, environment1),now + 400, 3, TrimJson(Settings1Patch3));

        yield return new SettingsEvent
            (SettingsEventType.UpdateEvent,new SettingsMetadata(service2, environment2),now + 500, 1, TrimJson(Settings2Patch1));
    }

    public static SettingsEvent CreateSettingsUpdateEvent() =>
        CreateSettingsEvents().First(se => se.EventType is SettingsEventType.UpdateEvent);

    public static SettingsEvent CreateSettingsCreateEvent() =>
        CreateSettingsEvents().First(se => se.EventType is SettingsEventType.CreateEvent);


    public static string GetExpectedResultSettings(string serviceName, string environmentName)
    {
        if(serviceName == "service1" && environmentName == "environment1")
            return ExpectedResultSettings1;

        if(serviceName == "service2" && environmentName == "environment2")
            return ExpectedResultSettings2;

        throw new InvalidOperationException("Invalid service name or environment name");
    }

    private static string TrimJson(string json) => json.Replace("\n", "").Replace(" ", "");


    const string InitialSettings1 = """
                                    {
                                      "ConnectionStrings": {
                                        "Redis": "localhost:6379",
                                        "MongoDB": "mongodb://localhost:27017"
                                      },
                                      "SettingsProjection" : {
                                        "ExpirationTimeHours" : 1
                                      }
                                    }
                                    """;

    const string Settings1Patch1 = """
                                   [{
                                     "op": "add",
                                     "path": "/ConnectionStrings/Postgres",
                                     "value": "localhost:6310"
                                   }]
                                   """;

    const string Settings1Patch2 = """
                                   [{
                                     "op": "replace",
                                     "path": "/SettingsProjection/ExpirationTimeHours",
                                     "value": 2
                                   }]
                                   """;

    const string Settings1Patch3 = """
                                   [{
                                     "op": "remove",
                                     "path": "/ConnectionStrings/Redis"
                                   }]
                                   """;

    static readonly string ExpectedResultSettings1 = TrimJson("""
                                                           {
                                                             "ConnectionStrings": {
                                                               "MongoDB": "mongodb://localhost:27017",
                                                               "Postgres": "localhost:6310"
                                                             },
                                                             "SettingsProjection" : {
                                                               "ExpirationTimeHours" : 2
                                                             }
                                                           }
                                                           """);

    const string InitialSettings2 = """
                                    {
                                      "Debug" : {
                                        "DisableAuthorization" : false,
                                        "DisableDbConnection" : false
                                      }
                                    }
                                    """;
    const string Settings2Patch1 = """
                                   [{
                                     "op": "add",
                                     "path": "/Logging",
                                     "value":
                                     {
                                        "LogLevel":
                                        {
                                            "Default": "Warning"
                                        }
                                     }
                                   }]
                                   """;

    static readonly string ExpectedResultSettings2 = TrimJson("""
                                                           {
                                                             "Debug" : {
                                                               "DisableAuthorization" : false,
                                                               "DisableDbConnection" : false
                                                             },
                                                             "Logging" : {
                                                               "LogLevel" : {
                                                                 "Default" : "Warning"
                                                               }
                                                             }
                                                           }
                                                           """);
}
