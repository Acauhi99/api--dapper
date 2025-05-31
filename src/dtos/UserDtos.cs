namespace api__dapper.dtos
{
  public record CreateUser(string Name, string Email, string Password);
  public record UpdateUser(string Name, string Email, string? Password);
  public record Login(string Email, string Password);
}
