using System.Text;
using FlowStack.CommentService.Data;
using FlowStack.CommentService.Helpers;
using FlowStack.CommentService.Middleware;
using FlowStack.CommentService.Repositories;
using FlowStack.CommentService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

try
{
    Log.Information("Starting FlowStack.CommentService...");

    var builder = WebApplication.CreateBuilder(args);


    // Database
    builder.Services.AddDbContext<CommentDbContext>(options =>
        options
            .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention());

    //  Dependency Injection 
    builder.Services.AddScoped<ICommentRepository, CommentRepository>();
    builder.Services.AddScoped<ICommentService, CommentServiceImpl>();

    // HTTP Clients 
    builder.Services.AddHttpClient<TaskClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Services:TaskServiceUrl"]
                ?? throw new InvalidOperationException("Services:TaskServiceUrl not configured."));
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(10);
    });

    builder.Services.AddHttpClient<BoardClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["Services:BoardServiceUrl"]
                ?? throw new InvalidOperationException("Services:BoardServiceUrl not configured."));
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(10);
    });

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
            Title = "FlowStack Comment & Attachment Service",
            Version = "v1",
            Description = "Collaboration service for FlowStack task cards"
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
        .AddDbContextCheck<CommentDbContext>("comment-db");

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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowStack Comment Service v1");
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
        var db = scope.ServiceProvider.GetRequiredService<CommentDbContext>();
        db.Database.Migrate();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "FlowStack.CommentService failed to start.");
}
finally
{
    Log.CloseAndFlush();
}
