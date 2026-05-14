using System.Text;
using FlowStack.BoardService.Data;
using FlowStack.BoardService.Helpers;
using FlowStack.BoardService.Middleware;
using FlowStack.BoardService.Repositories;
using FlowStack.BoardService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// Serilog bootstrap logger 
// Used only during startup — before full config is loaded.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    // Npgsql 6.0+ legacy timestamp behavior
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    Log.Information("Starting FlowStack.BoardService...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog full configuration
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    // Database
    builder.Services.AddDbContext<BoardDbContext>(options =>
        options
            .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention());

    // Dependency Injection
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IBoardRepository, BoardRepository>();
    builder.Services.AddScoped<IBoardService, BoardServiceImpl>();

    // HTTP Client — workspace-service 
    // Typed client used to verify workspace membership before board operations.
    builder.Services.AddHttpClient<WorkspaceClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Services:WorkspaceServiceUrl"]
                ?? throw new InvalidOperationException(
                        "Services:WorkspaceServiceUrl not configured."));
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    // JWT Bearer Authentication 
    // board-service validates tokens LOCALLY using the shared secret.
    // No round-trip to auth-service per request — stateless and fast.
    // All three values (Secret, Issuer, Audience) MUST match auth-service exactly.
    var jwtSecret   = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
    var jwtIssuer   = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer           = true,
            ValidIssuer              = jwtIssuer,
            ValidateAudience         = true,
            ValidAudience            = jwtAudience,
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero 
        };
    });

    // Authorization Policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("MemberOrAbove", policy =>
            policy.RequireRole("Member", "WorkspaceAdmin", "PlatformAdmin"));

        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("PlatformAdmin"));
    });

    // CORS 
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFlowStackWeb", policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://flowstack.app")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
        });
    });

    // Controllers + Swagger
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "FlowStack Board Service",
            Version     = "v1",
            Description = "Kanban board management and membership for FlowStack"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name         = "Authorization",
            Type         = SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT",
            In           = ParameterLocation.Header,
            Description  = "Paste your JWT token here. Example: eyJhbGci..."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // Health Checks 
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<BoardDbContext>("board-db");

    var app = builder.Build();

    // Middleware Pipeline 
    // ORDER IS CRITICAL — each middleware wraps everything below it.

    app.UseCorrelationId();

    app.UseGlobalExceptionHandler();

    app.UseRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowStack Board Service v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowFlowStackWeb");

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // Auto-migrate on startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BoardDbContext>();
        db.Database.Migrate();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "FlowStack.BoardService failed to start.");
}
finally
{
    Log.CloseAndFlush();
}