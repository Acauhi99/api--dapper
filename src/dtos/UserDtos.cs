namespace api__dapper.dtos
{
  public record CreateUserDto(string Name, string Email);
  public record UpdateUserDto(string Name, string Email);
}