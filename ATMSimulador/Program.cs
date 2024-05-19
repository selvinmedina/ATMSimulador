using ATMSimulador.Domain.Security;
using ATMSimulador.Domain.Validations;
using ATMSimulador.Features.Sockets;
using ATMSimulador.Features.Usuarios;
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

builder.Services.AddDbContext<ATMDbContext>(options =>
            options.UseSqlServer("name=ATMSimulador"));

builder.Services.AddScoped<IUnitOfWork, ApplicationUnitOfWork>();

builder.Services.AddSingleton<UsuariosService>();
builder.Services.AddSingleton<XmlEncryptionService>();
builder.Services.AddSingleton<UsuarioDomain>();
builder.Services.AddHostedService<SignalRClient>(x=>
{
    return new SignalRClient();
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

app.MapHub<NotificacionHub>("/hub");

app.Run();
