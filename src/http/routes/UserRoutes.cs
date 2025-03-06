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
        Results.Ok(await service.GetAllUsers()));

    group.MapGet("/{id}", async (string id, IUserService service) =>
    {
      var user = await service.GetUserById(id);
      return user is null ? Results.NotFound() : Results.Ok(user);
    });

    group.MapPost("/", async (CreateUserDto userDto, IUserService service) =>
    {
      try
      {
        var user = await service.CreateUser(userDto);
        return Results.Created($"/api/users/{user.Id}", user);
      }
      catch (EmailAlreadyExistsException ex)
      {
        return Results.Conflict(new { message = ex.Message });
      }
    });

    group.MapPut("/{id}", async (string id, UpdateUserDto userDto, IUserService service) =>
    {
      var updatedUser = await service.UpdateUser(id, userDto);
      return updatedUser is null ? Results.NotFound() : Results.Ok(updatedUser);
    });

    group.MapDelete("/{id}", async (string id, IUserService service) =>
    {
      var result = await service.DeleteUser(id);
      return result ? Results.NoContent() : Results.NotFound();
    });
  }
}
