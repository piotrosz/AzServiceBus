using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace CommonServiceBusConnectionString;

public static class Settings
{
    const string ConnectionStringKey = "ServiceBusConnectionString";
    const string ConfigFileName = "appsettings.json";
    
    public static string GetConnectionString(Assembly executingAssembly)
    {
        // Console.WriteLine($"Getting connection string from: {executingAssembly.FullName}");
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(ConfigFileName, optional: true, reloadOnChange: true)
            .AddUserSecrets(executingAssembly);

        IConfiguration config = builder.Build();
        var connectionString = config.GetSection(ConnectionStringKey);

        if (string.IsNullOrWhiteSpace(connectionString.Value))
        {
            throw new ApplicationException($"ServiceBus connection string was not found in user secrets nor in {ConfigFileName} config file.");
        }
        
        return connectionString.Value;
    }
}