using ATMSimulador.Domain.Dtos;
using ATMSimulador.Features.Auth;
using ATMSimulador.Features.Usuarios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using SoapCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.OpenApi.Models;
using ATMSimulador.Domain.Security;
using ATMSimulador.Domain.Validations;
using ATMSimulador.Infrastructure.Database;
using ATMSimulador.Infrastructure;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using ATMSimulador.Features.Servicios;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

var jwtSettings = new JwtSettings()
{
    Audience = config["Security:Jwt:Audience"]!,
    Issuer = config["Security:Jwt:Issuer"]!,
    SecretKey = config["Security:Jwt:SecretKey"]!
};

// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers()
    .AddXmlSerializerFormatters();
builder.Services.AddSoapCore();
builder.Services.AddMvc();

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
});

ServiciosApp(builder);

var app = builder.Build();

app.UseCors("AllowAngularApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
}

app.UseHttpsRedirection();
app.UseRouting(); // Place this before UseAuthorization

app.UseAuthentication();
app.UseAuthorization();

var settings = config.GetSection("FileWSDL").Get<WsdlFileOptions>();

if (settings is { })
    settings.AppPath = app.Environment.ContentRootPath;

app.UseEndpoints(endpoints =>
{
    endpoints.UseSoapEndpoint<IUsuariosService>("/UsuariosService.svc", new SoapEncoderOptions(), SoapSerializer.XmlSerializer, false, null, settings);
    endpoints.UseSoapEndpoint<IServiciosService>("/ServiciosService.svc", new SoapEncoderOptions(), SoapSerializer.XmlSerializer, false, null, settings);
    endpoints.MapControllers();
});

app.Run();

void ServiciosApp(WebApplicationBuilder builder)
{
    builder.Services.AddDbContext<ATMDbContext>(options =>
        options.UseSqlServer("name=ATMSimulador"));
    builder.Services.AddScoped<IUnitOfWork, ApplicationUnitOfWork>();
    builder.Services.AddTransient<IAuthService, AuthService>();
    builder.Services.AddTransient<IUsuariosService, UsuariosService>();
    builder.Services.AddTransient<IServiciosService, ServiciosService>();

    builder.Services.AddSingleton<EncryptionService>(x =>
    {
        var secretKey = builder.Configuration["Security:SecretKeyEncryptionService"];

        if (secretKey is null) throw new Exception("La key Security:SecretKeyEncryptionService no existe en appsettings.");

        return new(secretKey);
    });
    builder.Services.AddSingleton<UsuarioDomain>();
    builder.Services.AddSingleton(jwtSettings);
    builder.Services.AddHttpContextAccessor();

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
