using api__dapper.domain.services;
using api__dapper.dtos;
using api__dapper.utils.exceptions;


namespace api__dapper.http.routes;

public static class UserRoutes
{
  public static void MapUserEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("/api/users");

    group.MapGet("/", async (IUserService service) =>
        Results.Ok(await service.GetAllUsersAsync()));

    group.MapGet("/{id}", async (string id, IUserService service) =>
    {
      var user = await service.GetUserByIdAsync(id);
      return user is null ? Results.NotFound() : Results.Ok(user);
    });

    group.MapPost("/", async (CreateUserDto userDto, IUserService service) =>
    {
      try
      {
        var user = await service.CreateUserAsync(userDto);
        return Results.Created($"/api/users/{user.Id}", user);
      }
      catch (EmailAlreadyExistsException ex)
      {
        return Results.Conflict(new { message = ex.Message });
      }
    });

    group.MapPut("/{id}", async (string id, UpdateUserDto userDto, IUserService service) =>
    {
      var updatedUser = await service.UpdateUserAsync(id, userDto);
      return updatedUser is null ? Results.NotFound() : Results.Ok(updatedUser);
    });

    group.MapDelete("/{id}", async (string id, IUserService service) =>
    {
      var result = await service.DeleteUserAsync(id);
      return result ? Results.NoContent() : Results.NotFound();
    });
  }
}
