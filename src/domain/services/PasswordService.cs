using BCrypt.Net;

namespace api__dapper.domain.services;

public interface IPasswordService
{
  string HashPassword(string password);
  bool VerifyPassword(string password, string hash);
}

public class PasswordService : IPasswordService
{
  public string HashPassword(string password)
  {
    return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
  }

  public bool VerifyPassword(string password, string hash)
  {
    return BCrypt.Net.BCrypt.Verify(password, hash);
  }
}
