using api__dapper.domain.services;
using api__dapper.http.routes;
using api__dapper.infra;
using api__dapper.infra.repositories;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DI Registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Initialize SQLite database
DatabaseConfig.InitializeDatabase(app.Configuration);

// Middleware
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Routes
app.MapUserEndpoints();

app.Run();
