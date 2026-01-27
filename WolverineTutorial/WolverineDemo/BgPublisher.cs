using Microsoft.Extensions.Hosting;
using Wolverine;

namespace WolverineDemo;

public class BgPublisher(IMessageBus messageBus) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await messageBus.SendAsync(
                new CreateCustomerCommand(Guid.NewGuid(), Guid.NewGuid().ToString()));
            await Task.Delay(3000, stoppingToken);
        }
    }
}