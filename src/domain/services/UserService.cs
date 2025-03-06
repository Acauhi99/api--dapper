using api__dapper.domain.models;
using api__dapper.dtos;
using api__dapper.infra.repositories;

using NanoidDotNet;

namespace api__dapper.domain.services;

public interface IUserService
{
  Task<IEnumerable<User>> GetAllUsers();
  Task<User?> GetUserById(string id);
  Task<User?> GetUserByEmail(string email);
  Task<User> CreateUser(CreateUserDto userDto);
  Task<User?> UpdateUser(string id, UpdateUserDto userDto);
  Task<bool> DeleteUser(string id);
}

public class UserService(IUserRepository repository, IPasswordService passwordService) : IUserService
{
  private readonly IUserRepository _repository = repository;
  private readonly IPasswordService _passwordService = passwordService;

  public async Task<IEnumerable<User>> GetAllUsers()
  {
    return await _repository.GetAllUsers();
  }

  public async Task<User?> GetUserById(string id)
  {
    return await _repository.GetUserById(id);
  }

  public async Task<User?> GetUserByEmail(string email)
  {
    return await _repository.GetUserByEmail(email);
  }

  public async Task<User> CreateUser(CreateUserDto userDto)
  {
    var user = new User
    {
      Id = Nanoid.Generate(size: 12),
      Name = userDto.Name,
      Email = userDto.Email,
      Password = _passwordService.HashPassword(userDto.Password),
      CreatedAt = DateTime.UtcNow
    };

    await _repository.CreateUser(user);

    return user;
  }

  public async Task<User?> UpdateUser(string id, UpdateUserDto userDto)
  {
    var existingUser = await _repository.GetUserById(id);
    if (existingUser is null) return null;

    existingUser.Name = userDto.Name;
    existingUser.Email = userDto.Email;

    if (!string.IsNullOrEmpty(userDto.Password))
    {
      existingUser.Password = _passwordService.HashPassword(userDto.Password);
    }

    var success = await _repository.UpdateUser(existingUser);
    return success ? existingUser : null;
  }

  public async Task<bool> DeleteUser(string id)
  {
    return await _repository.DeleteUser(id);
  }

}
