# Azure Service Bus examples

## Configure Azure ServiceBus connection string

using user secrets:
```bash
dotnet user-secrets init
dotnet user-secrets set "ServiceBusConnectionString" "[connection string value]" 
```

or `appconfig.json` in project folder.

## Administration

Operations on queues, topics and subscriptions (create, list, delete).

## Working with messages

Send in batches, threads, 

## Message correlation

Sends duplicate messages randomly.

## Subscription rules

Subscriptions with different SQL filters.

## Error handling

3 console apps:
- Sender
- Receiver can handle message or send to dead letter queue on error.
Creates queue and forwarding queue.
- Dead letter receiver 

## Topic chat

Multiple users can send and receive messages.

## QueueSendReceive

Simple example of sending and receiving messages in a queue.

## RFID checkout 

This code is part of a simulated RFID checkout system using Azure Service Bus for messaging. It represents the checkout counter that receives RFID tag messages from a tag reader and processes them to calculate the customer's bill.

The solution consists of two main components:
1.	RfidCheckout.TagReader: Simulates an RFID tag scanner that reads product tags and sends them as messages to Azure Service Bus.
2.	RfidCheckout.Checkout: Receives these messages and processes them to build a customer's bill.
Ke

You can experiment with duplicates detection and message sessions.	

https://learn.microsoft.com/en-us/azure/service-bus-messaging/message-sessions

https://learn.microsoft.com/en-us/azure/service-bus-messaging/duplicate-detection
