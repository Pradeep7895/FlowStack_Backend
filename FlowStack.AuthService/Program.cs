using System.Text;
using FlowStack.AuthService.Data;
using FlowStack.AuthService.Helpers;
using FlowStack.AuthService.Middleware;
using FlowStack.AuthService.Repositories;
using FlowStack.AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

//  Database (PostgreSQL) 
// UseSnakeCaseNamingConvention maps C# PascalCase → postgres snake_case automatically
// e.g. UserId → user_id, FullName → full_name, PasswordHash → password_hash
builder.Services.AddDbContext<AuthDbContext>(options =>
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention());

//  Dependency Injection 
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthServiceImpl>();
builder.Services.AddSingleton<JwtHelper>();

//  JWT Bearer Authentication 
var jwtSecret   = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
var jwtIssuer   = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
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
})
//  Google OAuth2 
.AddGoogle(options =>
{
    options.ClientId     = builder.Configuration["OAuth:Google:ClientId"]     ?? "";
    options.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"] ?? "";
    options.CallbackPath = "/api/oauth/google/callback";
    options.SaveTokens   = true; 
    options.Scope.Add("email");
    options.Scope.Add("profile");
})
//  GitHub OAuth2 
.AddOAuth("GitHub", options =>
{
    options.ClientId                = builder.Configuration["OAuth:GitHub:ClientId"]     ?? "";
    options.ClientSecret            = builder.Configuration["OAuth:GitHub:ClientSecret"] ?? "";
    options.CallbackPath            = "/api/oauth/github/callback";
    options.AuthorizationEndpoint   = "https://github.com/login/oauth/authorize";
    options.TokenEndpoint           = "https://github.com/login/oauth/access_token";
    options.UserInformationEndpoint = "https://api.github.com/user";
    options.Scope.Add("user:email");

    options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.NameIdentifier, "id");
    options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Name,           "name");
    options.ClaimActions.MapJsonKey(System.Security.Claims.ClaimTypes.Email,          "email");
    options.ClaimActions.MapJsonKey("avatar_url",                                      "avatar_url");

    options.Events.OnCreatingTicket = async context =>
    {
        var req = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
        req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
        var res  = await context.Backchannel.SendAsync(req);
        var user = System.Text.Json.JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        context.RunClaimActions(user.RootElement);
    };
});

//  Authorization Policies 
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MemberOrAbove", policy => policy.RequireRole("Member", "BoardAdmin", "PlatformAdmin"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("PlatformAdmin"));
});

//  CORS 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlowStackWeb", policy =>
    {
        policy.WithOrigins("http://localhost:3000","http://localhost:5009","https://flowStack.app")
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
        Title       = "FlowStack Auth Service",
        Version     = "v1",
        Description = "Authentication, OAuth2, and user management for FlowStack"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Health checks 
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AuthDbContext>("auth-db");

var app = builder.Build();

//  Middleware pipeline 
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowStack Auth v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFlowStackWeb");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

app.Run();