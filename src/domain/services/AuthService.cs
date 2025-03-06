using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using api__dapper.domain.models;
using api__dapper.dtos;

using Microsoft.IdentityModel.Tokens;

namespace api__dapper.domain.services;

public interface IAuthService
{
  Task<string?> Login(LoginDto loginDto);
  Task<(User User, string Token)> Register(CreateUserDto registerDto);
  string GenerateJwtToken(User user);
}

public class AuthService(
    IUserService userService,
    IPasswordService passwordService,
    IConfiguration configuration) : IAuthService
{
  private readonly IUserService _userService = userService;
  private readonly IPasswordService _passwordService = passwordService;
  private readonly IConfiguration _configuration = configuration;

  public async Task<string?> Login(LoginDto loginDto)
  {
    var user = await _userService.GetUserByEmail(loginDto.Email);

    if (user == null)
      return null;

    if (!_passwordService.VerifyPassword(loginDto.Password, user.Password))
      return null;

    return GenerateJwtToken(user);
  }

  public async Task<(User User, string Token)> Register(CreateUserDto registerDto)
  {
    var user = await _userService.CreateUser(registerDto);
    var token = GenerateJwtToken(user);

    return (user, token);
  }

  public string GenerateJwtToken(User user)
  {
    var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
        throw new InvalidOperationException("JWT key not configured"));

    var tokenHandler = new JwtSecurityTokenHandler();

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id),
        new(ClaimTypes.Name, user.Name),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.Role, user.Role)
    };

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims),
      Expires = DateTime.UtcNow.AddMinutes(double.Parse(
            _configuration["Jwt:ExpirationMinutes"]!)),
      Issuer = _configuration["Jwt:Issuer"],
      Audience = _configuration["Jwt:Audience"],
      SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
  }
}
