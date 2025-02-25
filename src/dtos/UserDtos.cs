namespace api__dapper.dtos
{
  public record CreateUserDto(string Name, string Email, string Password);
  public record UpdateUserDto(string Name, string Email, string? Password);
  public record LoginDto(string Email, string Password);
}
