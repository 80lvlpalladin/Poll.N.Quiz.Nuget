namespace Poll.N.Quiz.API.Shared.Exceptions;

public class ConfigurationException(string sectionName)
    : Exception($"The configuration section '{sectionName}' is invalid or missing.");
