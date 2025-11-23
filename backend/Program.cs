using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RedYellowGreen.Api.Data.Models;
using RedYellowGreen.Api.Data.Models.Entities;
using RedYellowGreen.Api.Hubs;

// Configuring WebApplication
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source=redyellowgreen.db"));

builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Services.AddScoped<RedYellowGreen.Api.Interfaces.IEquipmentRepository, RedYellowGreen.Api.Repositories.EquipmentRepository>();
builder.Services.AddScoped<RedYellowGreen.Api.Interfaces.IEquipmentService, RedYellowGreen.Api.Services.EquipmentService>();

// Swagger setup
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RedYellowGreen API", Version = "v1" });
});

// Build and run the app
var app = builder.Build();

app.UseRouting();
app.UseCors("AllowFrontend");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    // Seed mock data if empty
    if (!db.Equipments.Any())
    {
        db.Equipments.AddRange(
            new Equipment { Name = "Packing Line A", CurrentState = ProductionState.Green },
            new Equipment { Name = "Packing Line B", CurrentState = ProductionState.Red },
            new Equipment { Name = "Packing Line C", CurrentState = ProductionState.Yellow }
        );
        db.SaveChanges();
    }
    if (!db.ProductionOrders.Any())
    {
        var now = DateTime.UtcNow;
        db.ProductionOrders.AddRange(
            new ProductionOrder { OrderNumber = "ORD-1001", EquipmentId = db.Equipments.First().Id, ScheduledStart = now.AddMinutes(-30), ScheduledEnd = now.AddHours(1), Status = "InProgress", Description = "Batch A" },
            new ProductionOrder { OrderNumber = "ORD-1002", EquipmentId = db.Equipments.Skip(0).First().Id, ScheduledStart = now.AddHours(2), Status = "Scheduled", Description = "Batch B" }
        );
        db.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHub<StateHub>("/hubs/state").RequireCors("AllowFrontend");

app.Run();
