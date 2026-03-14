using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Infrastructure;
using Application;

// 1. Load the .env file variables into the application
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// 2. Fetch the secure connection string
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? throw new InvalidOperationException("DATABASE_URL environment variable is missing.");

// 3. Register Application and Infrastructure Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(connectionString);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();