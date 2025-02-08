using Microsoft.Extensions.Configuration;

namespace CommonServiceBusConnectionString;

public static class Settings
{
    public static string GetConnectionString()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        IConfiguration config = builder.Build();
        return config.GetConnectionString("ServiceBus");
    }
}