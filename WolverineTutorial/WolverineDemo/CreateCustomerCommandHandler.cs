using Microsoft.Extensions.Logging;

namespace WolverineDemo;

public sealed class CreateCustomerCommandHandler(ILogger<CreateCustomerCommandHandler> logger)
{
    public void Handle(CreateCustomerCommand command)
    {
        logger.LogInformation("Processing message " + command);
    }
}