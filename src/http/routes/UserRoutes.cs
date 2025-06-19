using api__dapper.domain.models;
using api__dapper.domain.services;
using api__dapper.dtos;
using api__dapper.http.middlewares;
using api__dapper.utils.exceptions;

using Microsoft.AspNetCore.Authorization;

using System.Security.Claims;

namespace api__dapper.http.routes;

public static class UserRoutes
{
  public static void MapUserEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("/api/users").WithTags("Users");

    group.MapGet("/", [Authorize(Policy = AuthorizationPolicies.AdminOnly)] async (IUserService service) =>
        Results.Ok(await service.GetAllUsers()));

    group.MapGet("/{id}", [Authorize(Policy = AuthorizationPolicies.UserOrAdmin)] async (string id, IUserService service, ClaimsPrincipal user) =>
    {
      var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
      var role = user.FindFirstValue(ClaimTypes.Role);

      if (role != UserRoles.Admin.ToString() && id != userId)
        return Results.Forbid();

      var userData = await service.GetUserById(id);
      return userData is null ? Results.NotFound() : Results.Ok(userData);
    });

    group.MapPost("/", async (CreateUser userDto, IUserService service) =>
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

    group.MapPut("/{id}", [Authorize(Policy = AuthorizationPolicies.UserOrAdmin)] async (string id, UpdateUser userDto, IUserService service, ClaimsPrincipal user) =>
    {
      var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
      var role = user.FindFirstValue(ClaimTypes.Role);

      if (role != UserRoles.Admin.ToString() && id != userId)
        return Results.Forbid();

      var updatedUser = await service.UpdateUser(id, userDto);
      return updatedUser is null ? Results.NotFound() : Results.Ok(updatedUser);
    });

    group.MapDelete("/{id}", [Authorize(Policy = AuthorizationPolicies.AdminOnly)] async (string id, IUserService service) =>
    {
      var result = await service.DeleteUser(id);
      return result ? Results.NoContent() : Results.NotFound();
    });
  }
}
