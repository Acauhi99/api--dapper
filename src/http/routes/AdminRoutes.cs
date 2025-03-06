using api__dapper.domain.services;
using api__dapper.dtos;
using api__dapper.utils.exceptions;

namespace api__dapper.http.routes;

public record CreateAdminDto(string Name, string Email, string Password, string AdminSecret);

public static class AdminRoutes
{
  public static void MapAdminEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("/api/admin");

    group.MapPost("/setup", async (CreateAdminDto adminDto, IUserService userService, IConfiguration configuration) =>
    {
      var configuredSecret = configuration["AdminSetup:Secret"];

      if (string.IsNullOrEmpty(configuredSecret) || adminDto.AdminSecret != configuredSecret)
      {
        return Results.Unauthorized();
      }

      try
      {
        var user = await userService.CreateAdminUser(new CreateUserDto(adminDto.Name, adminDto.Email, adminDto.Password));

        return Results.Created($"/api/users/{user.Id}", new
        {
          message = "Admin user created successfully",
          id = user.Id
        });
      }
      catch (EmailAlreadyExistsException ex)
      {
        return Results.Conflict(new { message = ex.Message });
      }
    });
  }
}
