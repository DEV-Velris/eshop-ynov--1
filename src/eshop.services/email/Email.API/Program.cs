using Email.API.Consumers;
using Email.API.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddMassTransit(config =>
{
    // Add Consumers
    config.AddConsumer<OrderCreatedEventConsumer>();
    config.AddConsumer<OrderUpdatedEventConsumer>();
    config.AddConsumer<OrderDeletedEventConsumer>();
    
    config.UsingRabbitMq((context, cfg) =>
    {
        // Configure RabbitMQ
        var connectionString = builder.Configuration.GetConnectionString("MessageBroker") 
            ?? "amqp://guest:guest@messageBroker:5672";
            
        cfg.Host(new Uri(connectionString));
        
        // Configure message retries
        cfg.UseMessageRetry(retry => retry.Exponential(
            retryLimit: 3,
            minInterval: TimeSpan.FromSeconds(2),
            maxInterval: TimeSpan.FromMinutes(5),
            intervalDelta: TimeSpan.FromSeconds(2)
        ));
        
        // Auto-configure endpoints for all consumers
        cfg.ConfigureEndpoints(context);
    });
});

// Logging
builder.Services.AddLogging();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();

app.Run();