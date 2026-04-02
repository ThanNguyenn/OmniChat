using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OmniChat.Api.Middlewares;
using OmniChat.Application.Services.BackgroundJobs;
using OmniChat.Application.Services.Channels;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Services.Resolver;
using OmniChat.Application.SignalRHub;
using OmniChat.Application.Utils;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Extensions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Implements;
using OmniChat.Infrastructure.Repositories.Interfaces;
using StackExchange.Redis;
using SwaggerThemes;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices();
ConfigureDatabase();
ConfigureAuthentication();
ConfigureSwagger();
ConfigureSignalRServices();
RegisterResolver();
BackgoudTaskQueue();
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

            // zalo Phone field can be string or number, we need to handle both cases
            options.JsonSerializerOptions.Converters.Add(
                new StringOrNumberConverter()
            );
        });

    // define redis
    var redisHost = builder.Environment.IsDevelopment()
      ? "localhost:6379"
      : Environment.GetEnvironmentVariable("REDIS") ?? "redis:6379";

    var options = ConfigurationOptions.Parse(redisHost);

    options.AbortOnConnectFail = false;
    options.ConnectRetry = 5;
    options.ConnectTimeout = 10000;
    options.SyncTimeout = 10000;
    options.ReconnectRetryPolicy = new ExponentialRetry(5000);

    var redis = ConnectionMultiplexer.Connect(options);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    ConfigureR2Storage();

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
    void ConfigureR2Storage()
    {
        // Configure typed settings
        builder.Services.Configure<R2Settings>(builder.Configuration.GetSection("R2"));
        builder.Services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<R2Settings>>().Value
        );

        // Register AmazonS3 client
        builder.Services.AddSingleton<IAmazonS3>(sp =>
        {
            var settings = sp.GetRequiredService<R2Settings>();

            if (string.IsNullOrWhiteSpace(settings.AccessKeyId) ||
                string.IsNullOrWhiteSpace(settings.SecretAccessKey) ||
                string.IsNullOrWhiteSpace(settings.Endpoint))
                throw new BusinessException("Missing R2 configuration");

            var credentials = new BasicAWSCredentials(settings.AccessKeyId, settings.SecretAccessKey);
            var config = new AmazonS3Config { ServiceURL = settings.Endpoint };

            return new AmazonS3Client(credentials, config);
        });
    }

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddHttpContextAccessor();
    var licenseKey = builder.Configuration["AutoMapper:LicenseKey"];
    builder.Services.AddAutoMapper(cfg => { cfg.LicenseKey = licenseKey; }, AppDomain.CurrentDomain.GetAssemblies());

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
    builder.Services.AddScoped<ITaskAssignmentService, TaskAssignmentService>();
    builder.Services.AddScoped<IClaimTypeService, ClaimTypeService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    builder.Services.AddScoped<IAccountService, AccountService>();
    builder.Services.AddScoped<IStaffService, StaffService>();
    builder.Services.AddScoped<ICustomerMergeService, CustomerMergeService>();
    builder.Services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IClaimService, ClaimService>();
    builder.Services.AddScoped<IFacebookOAuthService, FacebookOAuthService>();
    builder.Services.AddScoped<IChatAggregationService, ChatAggregationService>();
    builder.Services.AddScoped<IInstagramOAuthService, InstagramOAuthService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IKeywordService, KeywordService>();
    builder.Services.AddScoped<IKeywordTypeService, KeywordTypeService>();
    builder.Services.AddScoped<IMessageKeywordFilterService, MessageKeywordFilterService>();
    builder.Services.AddScoped<IR2StorageService, R2StorageService>();
    builder.Services.AddScoped<IProductBrandService, ProductBrandService>();
    //builder.Services.AddScoped<IWalletService, WalletService>();
    //builder.Services.AddScoped<IInvoiceService, InvoiceService>();
    //builder.Services.AddScoped<ICreditNoteService, CreditNoteService>();
}

void RegisterBackgroundServices()
{
    // Register background services here
    builder.Services.AddHostedService<ZaloTokenRefreshWorker>();
    builder.Services.AddHostedService<RefreshTokenCleanUpWorker>();
    builder.Services.AddHostedService<WebhookBackgroundWorker>();
    builder.Services.AddHostedService<ChatAggregationWorker>();
}

// add signalR
void ConfigureSignalRServices()
{
    builder.Services.AddSignalR();
}

// add Resolver
void RegisterResolver()
{
    builder.Services.AddScoped<FacebookResolver>();
    builder.Services.AddScoped<ZaloResolver>();
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

void BackgoudTaskQueue()
{
    builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
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
        var css = SwaggerTheme.GetSwaggerThemeCss(Theme.UniversalDark);
        c.HeadContent = $"<style id='custom-dark-mode'>{css}</style>";
    });
    var logger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("RawBodyLogger");

    app.Use(async (context, next) =>
    {
        if (context.Request.ContentType?.Contains("application/json") == true)
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            logger.LogInformation("RAW BODY: {Body}", body);
        }

        await next();
    });
    app.UseMiddleware<ExceptionHandlerMiddleware>();

    app.UseHttpsRedirection();

    app.UseCors(options =>
    {
        options.SetIsOriginAllowed(origin =>
           origin.StartsWith("http://localhost:") ||
           origin.StartsWith("https://localhost:") ||
           origin.Contains("omnichat.click") ||
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
