using Microsoft.Extensions.Logging;

namespace WolverineDemo;

public sealed class CustomerCreatedEventHandler(ILogger<CustomerCreatedEventHandler> logger)
{
    public void Handle(CustomerCreatedEvent @event)
    {
        logger.LogInformation("Customer created");
    }
}