using api__dapper.domain.services;
using api__dapper.dtos;


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
      var id = await service.CreateUserAsync(userDto);
      return Results.Created($"/api/users/{id}", id);
    });

    group.MapPut("/{id}", async (string id, UpdateUserDto userDto, IUserService service) =>
    {
      var result = await service.UpdateUserAsync(id, userDto);
      return result ? Results.NoContent() : Results.NotFound();
    });

    group.MapDelete("/{id}", async (string id, IUserService service) =>
    {
      var result = await service.DeleteUserAsync(id);
      return result ? Results.NoContent() : Results.NotFound();
    });
  }
}