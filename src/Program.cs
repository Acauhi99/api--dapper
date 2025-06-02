using api__dapper.domain.services;
using api__dapper.http.middlewares;
using api__dapper.http.routes;
using api__dapper.infra;
using api__dapper.infra.repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI Registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IServiceManagementService, ServiceManagementService>();

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


// Application Start
app.Run();
