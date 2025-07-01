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

### Message correlation

Sends duplicate messages randomly.

### Subscription rules

Subscriptions with different SQL filters.

### Error handling

3 console apps:
- Sender
- Receiver can handle message or send to dead letter queue on error.
Creates queue and forwarding queue.
- Dead letter receiver 
