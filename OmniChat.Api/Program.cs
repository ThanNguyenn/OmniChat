using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OmniChat.Api.Middlewares;
using OmniChat.Application.Services.BackgroundJobs;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
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
ConfigureSignalRServices();
var app = builder.Build();


//builder.Services.Configure<OmniChatDbContext>(
//    builder.Configuration.GetSection("ZaloWebHook")
//);
ConfigureSignalREndpoints();
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

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => new
                {
                    Field = e.Key,
                    Errors = e.Value.Errors.Select(x => x.ErrorMessage)
                });

            return new BadRequestObjectResult(new
            {
                Message = "Model binding failed",
                Errors = errors
            });
        };
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
    builder.Services.AddScoped<IInstagramUserService, InstagramUserService>();
    builder.Services.AddScoped<ISupportStaffMessageService, SupportStaffMessageService>();
    builder.Services.AddScoped<ISupportConversationService, SupportConversationService>();
    builder.Services.AddScoped<IClaimTypeService, ClaimTypeService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    builder.Services.AddScoped<IAccountService, AccountService>();
    builder.Services.AddScoped<IStaffService, StaffService>();
    builder.Services.AddScoped<ICustomerMergeService,CustomerMergeService>();
    builder.Services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IClaimService, ClaimService>();
    builder.Services.AddScoped<IFacebookOAuthService, FacebookOAuthService>();
    builder.Services.AddScoped<IInstagramOAuthService, InstagramOAuthService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
}

void RegisterBackgroundServices()
{
    // Register background services here
    builder.Services.AddHostedService<ZaloTokenRefreshWorker>();
    builder.Services.AddHostedService<RefreshTokenCleanUpWorker>();
}

// add signalR
void ConfigureSignalRServices()
{
    builder.Services.AddSignalR();
}

// SignalR Endpoint
void ConfigureSignalREndpoints()
{
    app.MapHub<SupportConversationHub>("/api/v1/supportConversationHub");
}

void ConfigureDatabase()
{
    builder.Services.AddDbContext<OmniChatDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("SupabaseConnection"),
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
                    var path = context.HttpContext.Request.Path;

                    // signalR token from query string
                    var signalRToken = context.Request.Query["access_token"];

                    Console.WriteLine($"[OnMessageReceived] Path: {path}");
                    Console.WriteLine($"[OnMessageReceived] Token from query: {signalRToken}");


                    if (!string.IsNullOrEmpty(signalRToken) && path.StartsWithSegments("/api/v1/supportConversationHub"))
                    {
                        context.Token = signalRToken;
                        Console.WriteLine("[OnMessageReceived] Token set for SignalR");
                        return Task.CompletedTask;
                    }

                    var accessToken = context.Request.Headers["Authorization"].FirstOrDefault();
              
                    if (!string.IsNullOrEmpty(accessToken) && !accessToken.StartsWith("Bearer "))
                    {
                        context.Request.Headers["Authorization"] = "Bearer " + accessToken;
                    }
                        return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
                    Console.WriteLine("[OnTokenValidated] Claims:");
                    foreach (var claim in claims ?? Enumerable.Empty<string>())
                    {
                        Console.WriteLine($"  {claim}");
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
    var logger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("RawBodyLogger");

    app.Use(async (context, next) =>
    {
        context.Request.EnableBuffering();

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true);

        var rawBody = await reader.ReadToEndAsync();

        context.Request.Body.Position = 0;

        logger.LogError("RAW REQUEST BODY:\n{Body}", rawBody);

        await next();
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
    app.UseStaticFiles();

    app.MapControllers();
}
