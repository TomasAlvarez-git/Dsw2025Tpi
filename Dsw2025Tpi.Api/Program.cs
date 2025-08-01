using Dsw2025Tpi.Api.Helpers;
using Dsw2025Tpi.Application.Helpers;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Api.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog.Extensions.Logging;
using System.Text;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddLoggingService(builder.Configuration); // Configura el servicio de logging

        // Agrega servicios básicos para controladores
        builder.Services.AddControllers();

        // Agrega servicios para documentar la API (Swagger/OpenAPI)
        builder.Services.AddEndpointsApiExplorer();

        // Configura Swagger para documentar la API y definir el esquema de seguridad
        builder.Services.AddSwaggerDocumentation();

        // Agrega soporte para health checks
        builder.Services.AddHealthChecks();

        // Configura la autenticación y autorización con JWT
        builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);

        // Configuración de la base de datos y el contexto
        builder.Services.AddDbContexts(builder.Configuration);

        // Inyección de dependencias para servicios y repositorios
        builder.Services.AddAplicationServices();

        // Política CORS para permitir frontend en localhost:3000
        builder.Services.AddCustomCors();

        var app = builder.Build();

        // Middleware para entorno de desarrollo: habilita Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Middlewares
        app.UseCustomMiddlewares();

        // Mapeo de los controladores a rutas HTTP
        app.MapControllers();

        // Endpoint para health check
        app.MapHealthChecks("/healthcheck");

        // Inicia la aplicación
        app.Run();
    }
}

