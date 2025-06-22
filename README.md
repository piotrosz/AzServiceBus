## Azure Service Bus examples

### Configure Azure ServiceBus connection string

using user secrets:
```bash
dotnet user-secrets init
dotnet user-secrets set "ServiceBusConnectionString" "[connection string value]" 
```

or `appconfig.json` in project folder.

### Administration

Operations on queues, topics and subscriptions (create, list, delete).

### Working with messages

