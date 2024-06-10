using ATMSimulador.Domain.Dominios;
using ATMSimulador.Domain.Security;
using ATMSimulador.Features.Auth;
using ATMSimulador.Features.Cuentas;
using ATMSimulador.Features.Pagos;
using ATMSimulador.Features.Servicios;
using ATMSimulador.Features.Transacciones;
using ATMSimulador.Features.Usuarios;
using ATMSimulador.Infrastructure;
using ATMSimulador.Infrastructure.Database;
using ATMSimulador.Middlewares;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SoapCore;
using System.Text;

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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var settings = config.GetSection("FileWSDL").Get<WsdlFileOptions>();

if (settings is { })
    settings.AppPath = app.Environment.ContentRootPath;

app.UseMiddleware<TokenExtractionMiddleware>();
app.UseMiddleware<SoapDecryptionMiddleware>();
app.UseMiddleware<SoapBodyReplacementMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.UseSoapEndpoint<IUsuariosService>("/UsuariosService.svc", new SoapEncoderOptions(), SoapSerializer.XmlSerializer, false, null, settings);
    endpoints.UseSoapEndpoint<IServiciosService>("/ServiciosService.svc", new SoapEncoderOptions(), SoapSerializer.XmlSerializer, false, null, settings);
    endpoints.UseSoapEndpoint<ICuentasService>("/CuentasService.svc", new SoapEncoderOptions(), SoapSerializer.XmlSerializer, false, null, settings);
    endpoints.UseSoapEndpoint<IPagosService>("/PagosService.svc", new SoapEncoderOptions(), SoapSerializer.XmlSerializer, false, null, settings);
    endpoints.UseSoapEndpoint<ITransaccionesService>("/TransaccionesService.svc", new SoapEncoderOptions(), SoapSerializer.XmlSerializer, false, null, settings);
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
    builder.Services.AddTransient<ICuentasService, CuentasService>();
    builder.Services.AddTransient<IPagosService, PagosService>();
    builder.Services.AddTransient<ITransaccionesService, TransaccionesService>();

    builder.Services.AddSingleton<EncryptionService>(x =>
    {
        var secretKey = builder.Configuration["Security:SecretKeyEncryptionService"];

        return secretKey is null
            ? throw new Exception("La key Security:SecretKeyEncryptionService no existe en appsettings.")
            : new(secretKey);
    });
    builder.Services.AddSingleton<UsuarioDomain>();
    builder.Services.AddSingleton<CuentaDomain>();
    builder.Services.AddTransient<PagoDomain>();

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
