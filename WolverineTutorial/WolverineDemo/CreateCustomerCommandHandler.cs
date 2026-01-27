using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace WolverineDemo;

[UsedImplicitly]
public sealed class CreateCustomerCommandHandler(ILogger<CreateCustomerCommandHandler> logger)
{
    public void Handle(CreateCustomerCommand command)
    {
        logger.LogInformation("Processing message from Handle method " + command);
    }
}