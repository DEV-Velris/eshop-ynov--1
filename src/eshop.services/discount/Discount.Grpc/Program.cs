using Discount.Grpc.Data;
using Discount.Grpc.Data.Extensions;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DiscountContext>(options => options.UseSqlite(configuration.GetConnectionString("DiscountConnection")));
builder.Services.AddScoped<DiscountEvaluationService>();

var app = builder.Build();

app.UseCustomMigration();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGrpcService<DiscountServiceServer>();
app.MapControllers();

app.MapGet("/",
    () =>
        "Discount service is running. Use gRPC or the documented REST API to interact with promotions.");

app.Run();