using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using wahaha.API.Data;
using wahaha.API.Mappings;
using wahaha.API.Repositories;
using wahaha.API.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<WahahaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WahahaByteConnectionString"),
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        );
    }));
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(typeof(Program).Assembly);
});
// Repository registrations
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAvatarItemRepository, AvatarItemRepository>();
builder.Services.AddScoped<IMinigameRepository, MinigameRepository>();
builder.Services.AddScoped<IMinigameSessionRepository, MinigameSessionRepository>();
builder.Services.AddScoped<IPointTransactionRepository, PointTransactionRepository>();
builder.Services.AddScoped<IStreakRepository, StreakRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IUserInventoryRepository, UserInventoryRepository>();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
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