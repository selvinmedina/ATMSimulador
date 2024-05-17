using ATMSimulador.Infrastructure;
using ATMSimulador.Infrastructure.Database;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

builder.Services.AddDbContext<ATMDbContext>(options =>
            options.UseSqlServer("name=ATMSimulador"));

builder.Services.AddScoped<IUnitOfWork, ApplicationUnitOfWork>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
