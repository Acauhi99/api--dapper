using Dapper;
using Microsoft.Data.Sqlite;
using api__dapper.domain.models;
using api__dapper.utils.exceptions;
using Microsoft.Extensions.Configuration;

namespace api__dapper.infra.repositories;

public interface IUserRepository
{
  Task<User?> GetByIdAsync(string id);
  Task<IEnumerable<User>> GetAllAsync();
  Task<User?> GetByEmailAsync(string email);
  Task<string> CreateAsync(User user);
  Task<bool> UpdateAsync(User user);
  Task<bool> DeleteAsync(string id);
}

public class UserRepository : IUserRepository
{
  private readonly string _connectionString;
  private const int UniqueConstraintErrorCode = 19;

  public UserRepository(IConfiguration configuration)
  {
    _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new ArgumentNullException(nameof(configuration), "A connection string 'DefaultConnection' não foi encontrada.");
  }

  public async Task<User?> GetByIdAsync(string id)
  {
    using var connection = new SqliteConnection(_connectionString);
    return await connection.QueryFirstOrDefaultAsync<User>(
        "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
  }

  public async Task<IEnumerable<User>> GetAllAsync()
  {
    using var connection = new SqliteConnection(_connectionString);
    return await connection.QueryAsync<User>("SELECT * FROM Users");
  }

  public async Task<User?> GetByEmailAsync(string email)
  {
    using var connection = new SqliteConnection(_connectionString);
    return await connection.QueryFirstOrDefaultAsync<User>(
        "SELECT * FROM Users WHERE Email = @Email", new { Email = email });
  }

  public async Task<string> CreateAsync(User user)
  {
    using var connection = new SqliteConnection(_connectionString);
    var sql = @"INSERT INTO Users (Id, Name, Email, CreatedAt)
                    VALUES (@Id, @Name, @Email, @CreatedAt)";
    try
    {
      await connection.ExecuteAsync(sql, user);
      return user.Id;
    }
    catch (SqliteException ex) when (ex.SqliteErrorCode == UniqueConstraintErrorCode)
    {
      throw new EmailAlreadyExistsException("O email informado já está em uso.");
    }
  }

  public async Task<bool> UpdateAsync(User user)
  {
    using var connection = new SqliteConnection(_connectionString);
    var result = await connection.ExecuteAsync(
        "UPDATE Users SET Name = @Name, Email = @Email WHERE Id = @Id",
        user);
    return result > 0;
  }

  public async Task<bool> DeleteAsync(string id)
  {
    using var connection = new SqliteConnection(_connectionString);
    var result = await connection.ExecuteAsync(
        "DELETE FROM Users WHERE Id = @Id",
        new { Id = id });
    return result > 0;
  }
}
