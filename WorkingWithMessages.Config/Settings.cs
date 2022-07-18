using Microsoft.Extensions.Configuration;

namespace WorkingWithMessages.Config
{
    public static class Settings
    {
        public const string QueueName = "workingwithmessages";

        public static string GetConnectionString()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();
            return config.GetConnectionString("ServiceBus");
        }
    }
}