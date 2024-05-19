using ATMSimulador.Domain.Mensajes;
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

// Configure SignalRClient as a hosted service
builder.Services.AddHostedService<SignalRClient>(serviceProvider =>
{
    var xmlEncryptionService = serviceProvider.GetRequiredService<XmlEncryptionService>();
    var signalRUrl = builder.Configuration["SignalR:Url"];
    if (string.IsNullOrEmpty(signalRUrl))
    {
        throw new ArgumentNullException(nameof(signalRUrl), ProgramMensajes.MSP_001);
    }
    return new SignalRClient(signalRUrl, xmlEncryptionService);
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
