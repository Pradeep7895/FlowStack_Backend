using System.Text;
using FlowStack.NotificationService.Data;
using FlowStack.NotificationService.Middleware;
using FlowStack.NotificationService.Repositories;
using FlowStack.NotificationService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// Serilog bootstrap logger 
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting FlowStack.NotificationService...");

    var builder = WebApplication.CreateBuilder(args);

    //  Serilog 
    builder.Host.UseSerilog((ctx, svc, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(svc)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName());

    //  Database (PostgreSQL) 
    builder.Services.AddDbContext<NotificationDbContext>(options =>
        options
            .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention());

    //  Dependency Injection 
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
    builder.Services.AddScoped<INotificationService, NotificationServiceImpl>();

    //  JWT Bearer Authentication 
    var jwtSecret = builder.Configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("Jwt:Secret not configured.");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    //  CORS 
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

    //  Controllers + Swagger 
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "FlowStack Notification Service",
            Version = "v1",
            Description = "Notification dispatch and storage for FlowStack"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header
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

    //  Health Checks 
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<NotificationDbContext>("notification-db");

    var app = builder.Build();

    //  Middleware Pipeline 
    app.UseCorrelationId();
    app.UseGlobalExceptionHandler();
    app.UseRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowStack Notification Service v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowFlowStackWeb");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

    //  Auto-migrate on startup 
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        db.Database.Migrate();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "FlowStack.NotificationService failed to start.");
}
finally
{
    Log.CloseAndFlush();
}
