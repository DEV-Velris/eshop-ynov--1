using Email.API.Consumers;
using Email.API.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Services de base
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Service d'email
builder.Services.AddScoped<IEmailService, EmailService>();

// Configuration MassTransit pour écouter les événements
builder.Services.AddMassTransit(config =>
{
    // Ajouter le consumer
    config.AddConsumer<OrderCreatedEventConsumer>();
    
    config.UsingRabbitMq((context, cfg) =>
    {
        // Configuration RabbitMQ
        var connectionString = builder.Configuration.GetConnectionString("MessageBroker") 
            ?? "amqp://guest:guest@messageBroker:5672";
            
        cfg.Host(new Uri(connectionString));
        
        // Configuration des retries en cas d'erreur
        cfg.UseMessageRetry(retry => retry.Exponential(
            retryLimit: 3,
            minInterval: TimeSpan.FromSeconds(2),
            maxInterval: TimeSpan.FromMinutes(5),
            intervalDelta: TimeSpan.FromSeconds(2)
        ));
        
        // Configuration automatique des endpoints
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