using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OmniChat.Api.Middlewares;
using OmniChat.Application.Services.BackgroundJobs;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Utils;
using OmniChat.Infrastructure.Extensions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Implements;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices();
ConfigureDatabase();
ConfigureAuthentication();
ConfigureSwagger();

var app = builder.Build();


//builder.Services.Configure<OmniChatDbContext>(
//    builder.Configuration.GetSection("ZaloWebHook")
//);

ConfigureMiddleware();

app.Run();

void ConfigureServices()
{
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Enum -> string
            options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter()
            );

            // DateTime -> UTC ISO format
            options.JsonSerializerOptions.Converters.Add(
                new UtcDateTimeJsonConverter()
            );

            //Nullable UTC ISO fomat
            options.JsonSerializerOptions.Converters.Add(
                new NullableUtcDateTimeJsonConverter()
            );

        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    builder.Services.AddHttpClient("ZaloOAuth", client =>
    {
        client.BaseAddress = new Uri("https://oauth.zaloapp.com/");
    });

    // Register Unit of Work pattern
    builder.Services.AddScoped<IUnitOfWork<OmniChatDbContext>, UnitOfWork<OmniChatDbContext>>();
    // Register utility services
    builder.Services.AddSingleton<JwtUtil>();
    // Register application services
    RegisterApplicationServices();
    RegisterBackgroundServices();


}

void RegisterApplicationServices()
{
    builder.Services.AddScoped<IProviderService, ProviderService>();
    builder.Services.AddScoped<ICustomerProfileService, CustomerProfileService>();
    builder.Services.AddScoped<IZaloOAuthService, ZaloOAuthService>();
    builder.Services.AddScoped<ICustomerMessageService, CustomerMessageService>();
    builder.Services.AddScoped<IWebhookService, WebhookService>();
    builder.Services.AddScoped<IZaloUserService, ZaloUserService>();
    builder.Services.AddScoped<IFacebookUserService, FacebookUserService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    builder.Services.AddScoped<IAccountService, AccountService>();
}

void RegisterBackgroundServices()
{
    // Register background services here
    builder.Services.AddHostedService<ZaloTokenRefreshWorker>();
    builder.Services.AddHostedService<RefreshTokenCleanUpWorker>();
}

void ConfigureDatabase()
{
    builder.Services.AddDbContext<OmniChatDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("PostgresConnection"),
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(
                    typeof(OmniChatDbContext).Assembly.FullName
                );
            }
        )
    );
}

void ConfigureAuthentication()
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)

        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>(),
                ValidAudience = builder.Configuration.GetSection("Jwt:Audience").Get<string>(),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Headers["Authorization"].FirstOrDefault();

                    if (!string.IsNullOrEmpty(accessToken) && !accessToken.StartsWith("Bearer "))
                    {
                        context.Request.Headers["Authorization"] = "Bearer " + accessToken;
                    }

                    return Task.CompletedTask;
                },

                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var response = new ApiResponse<object>
                    {
                        StatusCode = StatusCodes.Status401Unauthorized,
                        Message = "Unauthorized access",
                        Reason = "Authentication failed. Please provide a valid token.",
                        IsSuccess = false,
                        Data = new
                        {
                            Path = context.Request.Path,
                            Method = context.Request.Method,
                            Timestamp = DateTime.UtcNow
                        }
                    };

                    await context.Response.WriteAsJsonAsync(response);
                }


            };
        });


    // Add authorization policies
    builder.Services.AddAuthorization(options =>
    {
        //add custom policies here
    });
}

void ConfigureSwagger()
{
    builder.Services.AddSwaggerGen(options =>
    {
        options.EnableAnnotations();
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "OmniChat.API",
            Version = "v1",
            Description = "OmniChat API document"
        });

        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
        {
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Description = "JWT Authorization header using the Bearer scheme. Example: "
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                Array.Empty<string>()
            }
        });
    });
}

void ConfigureMiddleware()
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OmniChat API v1");
    });

    app.UseMiddleware<ExceptionHandlerMiddleware>();

    app.UseHttpsRedirection();

    app.UseCors(options =>
    {
        options.SetIsOriginAllowed(origin =>
           origin.StartsWith("http://localhost:") ||
           origin.StartsWith("https://localhost:") ||
           origin.EndsWith(".vercel.app"))
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();
}
