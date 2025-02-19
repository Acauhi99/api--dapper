using api__dapper.domain.models;
using api__dapper.domain.repositories;
using api__dapper.dtos;
using NanoidDotNet;

namespace api__dapper.domain.services;

public interface IUserService
{
  Task<IEnumerable<User>> GetAllUsersAsync();
  Task<User?> GetUserByIdAsync(string id);
  Task<string> CreateUserAsync(CreateUserDto userDto);
  Task<bool> UpdateUserAsync(string id, UpdateUserDto userDto);
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

  public async Task<string> CreateUserAsync(CreateUserDto userDto)
  {
    var user = new User
    {
      Id = Nanoid.Generate(size: 12),
      Name = userDto.Name,
      Email = userDto.Email,
      CreatedAt = DateTime.UtcNow
    };

    return await _repository.CreateAsync(user);
  }

  public async Task<bool> UpdateUserAsync(string id, UpdateUserDto userDto)
  {
    var existingUser = await _repository.GetByIdAsync(id);
    if (existingUser is null) return false;

    existingUser.Name = userDto.Name;
    existingUser.Email = userDto.Email;

    return await _repository.UpdateAsync(existingUser);
  }

  public async Task<bool> DeleteUserAsync(string id)
  {
    return await _repository.DeleteAsync(id);
  }
}