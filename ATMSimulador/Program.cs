#region usings
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Security;
using ATMSimulador.Features.Auth;
using ATMSimulador.Features.Sockets;
using ATMSimulador.Features.Usuarios;
using ATMSimulador.Infrastructure;
using ATMSimulador.Infrastructure.Database;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;
#endregion

#region Variables
var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

var jwtSettings = new JwtSettings()
{
    Audience = config["Security:Jwt:Audience"]!,
    Issuer = config["Security:Jwt:Issuer"]!,
    SecretKey = config["Security:Jwt:SecretKey"]!
};
#endregion

#region ServiceProvider
// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ServiciosApp(builder);
#endregion

var app = builder.Build();

app.UseCors("AllowAngularApp");

#region Configure
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// TODO: Pendiente de agregar cors
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
#endregion

app.Run();

void ServiciosApp(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<ATMDbContext>(options =>
    options.UseSqlServer("name=ATMSimulador"));
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

    builder.Services.AddScoped<IUnitOfWork, ApplicationUnitOfWork>();

    builder.Services.AddTransient<IAuthService, AuthService>();

    builder.Services.AddTransient<IUsuariosService, UsuariosService>();
    builder.Services.AddSingleton<XmlEncryptionService>();
    builder.Services.AddSingleton(jwtSettings);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularApp",
            builder =>
            {
                builder.WithOrigins("http://localhost:4200")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
            });
    });

    // Configure SignalRClient as a hosted service
    builder.Services.AddHostedService(serviceProvider =>
    {
        var xmlEncryptionService = serviceProvider.GetRequiredService<XmlEncryptionService>();
        var mediator = serviceProvider.GetRequiredService<IMediator>();

        var signalRUrl = builder.Configuration["SignalR:Url"];
        if (string.IsNullOrEmpty(signalRUrl))
        {
            throw new Exception(ProgramMensajes.MSP_001);
        }
        return new SignalRClient(signalRUrl, xmlEncryptionService, mediator);
    });
}