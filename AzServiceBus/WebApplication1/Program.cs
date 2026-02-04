using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

const string SBConnectionString = 
    "Endpoint=....;SharedAccessKeyName=....;SharedAccessKey=....;EntityPath=....";

builder.Services.AddAzureClients(async clientBuilder =>
{ 
    clientBuilder.AddServiceBusClient(SBConnectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", ([FromServices] ServiceBusClient serviceBusClient, CancellationToken cancellationToken) =>
    {
        // Assumption: connection string contains EntityPath
        var connString = ServiceBusConnectionStringProperties.Parse(SBConnectionString);

        // CreateSender and SendMessageAsync are virtual methods and can be mocked for unit testing
        var sender = serviceBusClient.CreateSender(connString.EntityPath);
        
        // await sender.SendMessageAsync(new ServiceBusMessage(...), cancellationToken);
        
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}