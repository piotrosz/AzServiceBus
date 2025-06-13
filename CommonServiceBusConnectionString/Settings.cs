using Microsoft.Extensions.Configuration;

namespace CommonServiceBusConnectionString;

public static class Settings
{
    const string ConnectionStringKey = "ServiceBus";
    const string ConfigFileName = "appsettings.json";
    
    public static string GetConnectionString()
    {
        var builder = new ConfigurationBuilder()
            .AddUserSecrets(ConnectionStringKey)
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(ConfigFileName, optional: false);

        IConfiguration config = builder.Build();
        var connectionString = config.GetConnectionString(ConnectionStringKey);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ApplicationException($"ServiceBus connection string was not found in user secrets nor in {ConfigFileName} config file.");
        }
        
        return connectionString;
    }
}