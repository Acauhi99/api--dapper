using System.Security.Claims;

using api__dapper.domain.services;
using api__dapper.dtos;
using api__dapper.http.middlewares;

using Microsoft.AspNetCore.Authorization;

namespace api__dapper.http.routes;

public static class DashboardRoutes
{
  public static void MapDashboardEndpoints(this WebApplication app)
  {
    var group = app.MapGroup("/api/orders").WithTags("Dashboard");

    // Client dashboard: Order history
    group.MapGet("/client", [Authorize(Policy = AuthorizationPolicies.UserOrAdmin)]
    async (
        ClaimsPrincipal user,
        IDashboardService dashboardService,
        [AsParameters] OrderQueryFilter filter
    ) =>
    {
      var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
      if (string.IsNullOrEmpty(userId))
        return Results.Unauthorized();

      var result = await dashboardService.GetClientOrderHistory(userId, filter);
      return Results.Ok(result);
    });

    // Admin dashboard: All orders with pagination
    group.MapGet("/", [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    async (
        IDashboardService dashboardService,
        [AsParameters] OrderQueryFilter filter
    ) =>
    {
      var result = await dashboardService.GetAdminOrdersList(filter);
      return Results.Ok(result);
    });

    // Admin dashboard: Summary statistics
    group.MapGet("/summary", [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    async (
        IDashboardService dashboardService,
        [AsParameters] SummaryQueryFilter filter
    ) =>
    {
      var result = await dashboardService.GetOrdersSummary(filter);
      return Results.Ok(result);
    });
  }
}
