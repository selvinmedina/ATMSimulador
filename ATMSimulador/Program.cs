#region usings
using ATMSimulador.Domain.Security;
using ATMSimulador.Domain.Validations;
using ATMSimulador.Features.Auth;
using ATMSimulador.Features.Usuarios;
using ATMSimulador.Infrastructure;
using ATMSimulador.Infrastructure.Database;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

builder.Services.AddControllers()
    .AddXmlSerializerFormatters();

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

    builder.Services.AddScoped<IUnitOfWork, ApplicationUnitOfWork>();

    builder.Services.AddTransient<IAuthService, AuthService>();

    builder.Services.AddTransient<IUsuariosService, UsuariosService>();
    builder.Services.AddSingleton<EncryptionService>(x =>
    {
        var secretKey = builder.Configuration["Security:SecretKeyEncryptionService"];

        if (secretKey is null) throw new Exception("La key Security:SecretKeyEncryptionService no existe en appsettings.");

        return new(secretKey);
    });

    builder.Services.AddSingleton<UsuarioDomain>();
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
}