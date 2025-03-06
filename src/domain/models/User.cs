namespace api__dapper.domain.models
{
  public class User
  {
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = UserRoles.User.ToString();
    public DateTime CreatedAt { get; set; }
  }

  public enum UserRoles
  {
    User,
    Admin
  }
}
