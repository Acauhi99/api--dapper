using api__dapper.domain.services;
using api__dapper.dtos;
using api__dapper.utils.exceptions;

namespace api__dapper.http.routes;

public static class AuthRoutes
{
  public static void MapAuthEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("/api/auth");

    group.MapPost("/login", async (LoginDto loginDto, IAuthService authService) =>
    {
      var token = await authService.Login(loginDto);
      if (token == null)
        return Results.Unauthorized();

      return Results.Ok(new { token });
    });

    group.MapPost("/register", async (CreateUserDto registerDto, IAuthService authService) =>
    {
      try
      {
        var (user, token) = await authService.Register(registerDto);

        return Results.Created($"/api/users/{user.Id}", new { token });
      }
      catch (EmailAlreadyExistsException ex)
      {
        return Results.Conflict(new { message = ex.Message });
      }
    });
  }
}
