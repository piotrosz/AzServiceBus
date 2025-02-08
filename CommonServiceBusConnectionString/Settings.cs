using Microsoft.Extensions.Configuration;

namespace CommonServiceBusConnectionString;

public static class Settings
{
    const string ConnectionStringKey = "ServiceBus";
    
    public static string GetConnectionString()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        IConfiguration config = builder.Build();
        var connString = config.GetConnectionString(ConnectionStringKey);

        if (string.IsNullOrWhiteSpace(connString))
        {
            throw new ApplicationException("No ServiceBus connection string found in appsettings.json");
        }
        
        return connString;
    }
}