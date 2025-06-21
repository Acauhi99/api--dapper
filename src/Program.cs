using api__dapper.domain.services;
using api__dapper.http.middlewares;
using api__dapper.http.routes;
using api__dapper.infra;
using api__dapper.infra.repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "Services API",
    Version = "v1",
    Description = "API for managing services, users, and sales"
  });

  // Add JWT authentication to Swagger UI
  options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JWT Authorization header using the Bearer scheme."
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

// Configure CORS
builder.Services.AddCors(options =>
{
  options.AddDefaultPolicy(policy =>
  {
    policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
  });
});

// DI Registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IServiceManagementService, ServiceManagementService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<ISellRepository, SellRepository>();
builder.Services.AddScoped<ISellService, SellService>();

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
  options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

  // Global rate limit: 30 requests per 10 seconds per client IP
  options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
  {
    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "unknown";
    return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
    {
      PermitLimit = 30,
      Window = TimeSpan.FromSeconds(10)
    });
  });

  // More restrictive limits for auth endpoints
  options.AddFixedWindowLimiter("auth", options =>
  {
    options.PermitLimit = 10;
    options.Window = TimeSpan.FromMinutes(1);
    options.QueueLimit = 0;
  });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
              Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty))
      };
    });

// Add authorization policies
builder.Services.AddAuthorizationPolicies();

var app = builder.Build();

// Initialize SQLite database
DatabaseConfig.InitializeDatabase(app.Configuration);

// Middleware
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// Apply CORS - deve ser antes dos middlewares que lidam com requisições
app.UseCors();

// Apply rate limiting before handling the request
app.UseRateLimiter();

// Request sanitization
app.UseRequestSanitization();
app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapUserEndpoints();
app.MapAuthEndpoints();
app.MapAdminEndpoints();
app.MapServiceEndpoints();
app.MapSellEndpoints();
app.MapDashboardEndpoints();


// Application Start
app.Run();
