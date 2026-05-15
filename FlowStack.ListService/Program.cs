using System.Text;
using FlowStack.ListService.Data;
using FlowStack.ListService.Helpers;
using FlowStack.ListService.Middleware;
using FlowStack.ListService.Repositories;
using FlowStack.ListService.Services;
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
    Log.Information("Starting FlowStack.ListService...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, svc, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration).ReadFrom.Services(svc).Enrich.FromLogContext());

    // Database
    builder.Services.AddDbContext<ListDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention());

    // Dependency Injection
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IListRepository, ListRepository>();
    builder.Services.AddScoped<IListService, ListServiceImpl>();

    // HTTP Client board-service 
    builder.Services.AddHttpClient<BoardClient>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["Services:BoardServiceUrl"]
                ?? throw new InvalidOperationException("Services:BoardServiceUrl not configured."));
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    // JWT Bearer Authentication 
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

    // Authorization 
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
            Title = "FlowStack List Service",
            Version = "v1",
            Description = "List/column management for FlowStack Kanban boards"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Health Checks 
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ListDbContext>("list-db");

    var app = builder.Build();

    // Middleware Pipeline
    // ORDER IS CRITICAL
    app.UseCorrelationId();        
    app.UseGlobalExceptionHandler(); 
    app.UseRequestLogging();   

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowStack List Service v1");
        c.RoutePrefix = "swagger";
    });


    app.UseHttpsRedirection();
    app.UseCors("AllowFlowStackWeb");
    app.UseAuthentication();  
    app.UseAuthorization(); 
    app.MapControllers();
    app.MapHealthChecks("/health");

    // Auto-migrate on startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ListDbContext>();
        db.Database.Migrate();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "FlowStack.ListService failed to start.");
}
finally
{
    Log.CloseAndFlush();
}