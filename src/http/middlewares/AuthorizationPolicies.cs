namespace api__dapper.http.middlewares;

public static class AuthorizationPolicies
{
  public const string AdminOnly = "AdminOnly";
  public const string UserOrAdmin = "UserOrAdmin";

  public static void AddAuthorizationPolicies(this IServiceCollection services)
  {
    services.AddAuthorizationBuilder()
      .AddPolicy(AdminOnly, policy => policy
        .RequireRole("Admin"))
      .AddPolicy(UserOrAdmin, policy => policy
        .RequireAuthenticatedUser());
  }
}
