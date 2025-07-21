
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Data.Helpers;
using Dsw2025Tpi.Data.Repositories;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Dsw2025Tpi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Agrega servicios básicos para controladores
        builder.Services.AddControllers();

        // Agrega servicios para documentar la API (Swagger/OpenAPI)
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(o =>
        {
            // Soluciona conflictos de nombres de clases anidadas en Swagger
            o.CustomSchemaIds(type => type.FullName.Replace("+", "."));
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Dsw2025Tpi",
                Version = "v1",
            });

            // Configura esquema de autenticación JWT en Swagger
            o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Ingrese el token JWT",
                Type = SecuritySchemeType.ApiKey
            });
            o.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Agrega soporte para health checks
        builder.Services.AddHealthChecks();

        // Configuración de Identity con política de contraseña mínima
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
        {
            options.Password = new PasswordOptions
            {
                RequiredLength = 8
            };
        })
           .AddEntityFrameworkStores<AuthenticateContext>() // Usa AuthenticateContext como fuente de usuarios
           .AddDefaultTokenProviders();

        // Configura la autenticación JWT
        var jwtConfig = builder.Configuration.GetSection("Jwt");
        var keyText = jwtConfig["Key"] ?? throw new ArgumentNullException("JWT Key");
        var key = Encoding.UTF8.GetBytes(keyText);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig["Issuer"],
                ValidAudience = jwtConfig["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        // Servicio que genera los tokens JWT
        builder.Services.AddSingleton<JwtTokenService>();

        // Configura el contexto de autenticación
        builder.Services.AddDbContext<AuthenticateContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("Dsw2025TpiEntities"));
        });

        builder.Services.AddAuthorization();

        // Configura el contexto principal con seed (carga inicial de datos desde JSON)
        builder.Services.AddDbContext<Dsw2025TpiContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("Dsw2025TpiEntities"));
            options.UseSeeding((c, t) =>
            {
                ((Dsw2025TpiContext)c).Seedwork<Customer>("Sources\\customers.json");
                ((Dsw2025TpiContext)c).Seedwork<Product>("Sources\\products.json");
                ((Dsw2025TpiContext)c).Seedwork<Order>("Sources\\orders.json");
                ((Dsw2025TpiContext)c).Seedwork<OrderItem>("Sources\\orderitems.json");
            });
        });

        // Inyección de dependencias para servicios y repositorios
        builder.Services.AddScoped<IRepository, EfRepository>(); // Patrón repositorio
        builder.Services.AddTransient<ProductsManagementService>();
        builder.Services.AddTransient<OrdersManagementService>();

        // Política CORS para permitir frontend en localhost:3000
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("PermitirFrontend", policy =>
                policy.WithOrigins("http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod());
        });

        var app = builder.Build();

        // Middleware para entorno de desarrollo: habilita Swagger
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Redirección HTTPS obligatoria
        app.UseHttpsRedirection();

        // Aplicar la política de CORS configurada anteriormente
        app.UseCors("PermitirFrontend");

        // Habilita autenticación (validación de tokens)
        app.UseAuthentication();

        // Habilita autorización (validación de roles y claims)
        app.UseAuthorization();

        // Mapeo de los controladores a rutas HTTP
        app.MapControllers();

        // Endpoint para health check
        app.MapHealthChecks("/healthcheck");

        // Inicia la aplicación
        app.Run();
    }
}

