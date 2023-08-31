using Microsoft.EntityFrameworkCore;
using Sensors.Data;
using Sensors.Data.Repos;
using Sensors.Data.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContextFactory<SensorsDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("Sensors"), sql =>
    {
        sql.MigrationsAssembly(typeof(SensorsDbContext).Assembly.FullName);
        sql.EnableRetryOnFailure();
    }), ServiceLifetime.Singleton);

builder.Services.AddSingleton<SensorRepository>();
builder.Services.AddSingleton<PredictionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
