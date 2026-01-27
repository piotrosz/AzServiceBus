using Microsoft.Extensions.Logging;

namespace WolverineDemo;

public static class CreateCustomerStaticHandler
{
    public static CustomerCreatedEvent Handle(
        CreateCustomerCommand command, 
        ILogger<CreateCustomerCommandHandler> logger)
    {
        logger.LogInformation("Processing message from static Handle method");
        return new CustomerCreatedEvent(command.Id, command.Name);
    }
}