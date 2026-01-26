
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using WolverineDemo;

var builder = Host.CreateDefaultBuilder();

builder.UseWolverine();

builder.ConfigureServices(services =>
{
    services.AddHostedService<BgPublisher>();
});

var app = builder.Build();

app.Run();