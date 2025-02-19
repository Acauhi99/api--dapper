using api__dapper.domain.models;
using api__dapper.dtos;
using api__dapper.infra.repositories;

using NanoidDotNet;

namespace api__dapper.domain.services;

public interface IUserService
{
  Task<IEnumerable<User>> GetAllUsersAsync();
  Task<User?> GetUserByIdAsync(string id);
  Task<User> CreateUserAsync(CreateUserDto userDto);
  Task<User?> UpdateUserAsync(string id, UpdateUserDto userDto);
  Task<bool> DeleteUserAsync(string id);
}

public class UserService : IUserService
{
  private readonly IUserRepository _repository;

  public UserService(IUserRepository repository)
  {
    _repository = repository;
  }

  public async Task<IEnumerable<User>> GetAllUsersAsync()
  {
    return await _repository.GetAllAsync();
  }

  public async Task<User?> GetUserByIdAsync(string id)
  {
    return await _repository.GetByIdAsync(id);
  }

  public async Task<User> CreateUserAsync(CreateUserDto userDto)
  {
    var user = new User
    {
      Id = Nanoid.Generate(size: 12),
      Name = userDto.Name,
      Email = userDto.Email,
      CreatedAt = DateTime.UtcNow
    };

    await _repository.CreateAsync(user);

    return user;
  }

  public async Task<User?> UpdateUserAsync(string id, UpdateUserDto userDto)
  {
    var existingUser = await _repository.GetByIdAsync(id);
    if (existingUser is null) return null;

    existingUser.Name = userDto.Name;
    existingUser.Email = userDto.Email;

    var success = await _repository.UpdateAsync(existingUser);
    return success ? existingUser : null;
  }

  public async Task<bool> DeleteUserAsync(string id)
  {
    return await _repository.DeleteAsync(id);
  }
}
