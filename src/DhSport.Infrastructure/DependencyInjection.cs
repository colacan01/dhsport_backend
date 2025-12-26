using System.Text;
using DhSport.Application.Interfaces;
using DhSport.Domain.Interfaces.Repositories;
using DhSport.Domain.Interfaces.Services;
using DhSport.Infrastructure.Data;
using DhSport.Infrastructure.Repositories;
using DhSport.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace DhSport.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                })
                .UseSnakeCaseNamingConvention();
        });

        // Redis
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configurationOptions = ConfigurationOptions.Parse(redisConnection);
                configurationOptions.AbortOnConnectFail = false;
                configurationOptions.ConnectTimeout = 5000;
                configurationOptions.SyncTimeout = 5000;
                return ConnectionMultiplexer.Connect(configurationOptions);
            });

            services.AddScoped<ICacheService, RedisCacheService>();
        }

        // Repositories
        services.AddScoped(typeof(DhSport.Application.Interfaces.IRepository<>), typeof(Repository<>));
        services.AddScoped<DhSport.Application.Interfaces.IUnitOfWork, UnitOfWork>();

        // JWT Service
        services.AddScoped<IJwtService, JwtService>();

        // File Service
        services.AddScoped<Infrastructure.Services.IFileService, FileService>();

        // JWT Authentication
        var jwtSecret = configuration["Jwt:Secret"];
        if (!string.IsNullOrEmpty(jwtSecret))
        {
            services.AddAuthentication(options =>
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
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        return services;
    }
}
