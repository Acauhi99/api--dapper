using api__dapper.domain.services;
using api__dapper.dtos;
using api__dapper.http.middlewares;

using Microsoft.AspNetCore.Authorization;

namespace api__dapper.http.routes;

public static class ServiceRoutes
{
  public static void MapServiceEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("/api/services").WithTags("Services");

    // Public endpoints - para o frontend consumir
    group.MapGet("/", async (IServiceManagementService service) =>
        Results.Ok(await service.GetAllServices()));

    group.MapGet("/key/{key}", async (string key, IServiceManagementService service) =>
    {
      var serviceData = await service.GetServiceByKey(key);
      return serviceData is null ? Results.NotFound() : Results.Ok(serviceData);
    });

    // Admin endpoints - para gerenciar os serviços
    group.MapPost("/", [Authorize(Policy = AuthorizationPolicies.AdminOnly)] async (CreateService serviceDto, IServiceManagementService service) =>
    {
      var createdService = await service.CreateService(serviceDto);
      return Results.Created($"/api/services/{createdService.Id}", createdService);
    });

    group.MapGet("/{id}", [Authorize(Policy = AuthorizationPolicies.AdminOnly)] async (string id, IServiceManagementService service) =>
    {
      var serviceData = await service.GetServiceById(id);
      return serviceData is null ? Results.NotFound() : Results.Ok(serviceData);
    });

    group.MapPut("/{id}", [Authorize(Policy = AuthorizationPolicies.AdminOnly)] async (string id, UpdateService serviceDto, IServiceManagementService service) =>
    {
      var updatedService = await service.UpdateService(id, serviceDto);
      return updatedService is null ? Results.NotFound() : Results.Ok(updatedService);
    });

    group.MapDelete("/{id}", [Authorize(Policy = AuthorizationPolicies.AdminOnly)] async (string id, IServiceManagementService service) =>
    {
      var result = await service.DeleteService(id);
      return result ? Results.NoContent() : Results.NotFound();
    });
  }
}
