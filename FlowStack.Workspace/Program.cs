using System.Text;
using FlowStack.Workspace.Data;
using FlowStack.Workspace.Middleware;
using FlowStack.Workspace.Repositories;
using FlowStack.Workspace.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Npgsql 6.0+ legacy timestamp behavior
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

//  Database (PostgreSQL) 
// Each microservice owns its own database — workspace-service never touches auth DB
builder.Services.AddDbContext<WorkspaceDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

// Dependency Injection  
builder.Services.AddScoped<IWorkspaceRepository, WorkspaceRepository>();
builder.Services.AddScoped<IWorkspaceService, WorkspaceServiceImpl>();

// HTTP client for calling auth-service (user profile lookups) 
// IHttpClientFactory creates typed clients — replaces RestTemplate from Spring
builder.Services.AddHttpClient("auth-service", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:AuthServiceUrl"]
            ?? throw new InvalidOperationException("Services:AuthServiceUrl not configured."));
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

//  JWT Bearer Authentication 
// workspace-service shares the SAME Jwt:Secret as auth-service.
// Tokens are validated locally — no round-trip to auth-service per request.
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
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
        policy.WithOrigins("http://localhost:3000", "https://flowStack.app")
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
        Title = "FlowBoard Workspace Service",
        Version = "v1",
        Description = "Workspace management and membership for FlowBoard"
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
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

//  Health checks 
builder.Services.AddHealthChecks()
    .AddDbContextCheck<WorkspaceDbContext>("workspace-db");

var app = builder.Build();

// Middleware pipeline 
app.UseGlobalExceptionHandler();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowStack Workspace v1");
    c.RoutePrefix = "swagger";
});


app.UseHttpsRedirection();
app.UseCors("AllowFlowStackWeb");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// Auto-migrate on startup (dev) 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WorkspaceDbContext>();
    db.Database.Migrate();
}

app.Run();