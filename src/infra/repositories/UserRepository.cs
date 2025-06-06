using Dapper;
using Microsoft.Data.Sqlite;
using api__dapper.domain.models;
using api__dapper.utils.exceptions;

namespace api__dapper.infra.repositories;

public interface IUserRepository
{
  Task<User?> GetUserById(string id);
  Task<IEnumerable<User>> GetAllUsers();
  Task<User?> GetUserByEmail(string email);
  Task<string> CreateUser(User user);
  Task<bool> UpdateUser(User user);
  Task<bool> DeleteUser(string id);
}

public class UserRepository(IConfiguration configuration) : IUserRepository
{
  private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new ArgumentNullException(nameof(configuration), "A connection string 'DefaultConnection' não foi encontrada.");
  private const int UniqueConstraintErrorCode = 19;

  public async Task<User?> GetUserById(string id)
  {
    using var connection = new SqliteConnection(_connectionString);
    return await connection.QueryFirstOrDefaultAsync<User>(
        "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
  }

  public async Task<IEnumerable<User>> GetAllUsers()
  {
    using var connection = new SqliteConnection(_connectionString);
    return await connection.QueryAsync<User>("SELECT * FROM Users");
  }

  public async Task<User?> GetUserByEmail(string email)
  {
    using var connection = new SqliteConnection(_connectionString);
    return await connection.QueryFirstOrDefaultAsync<User>(
        "SELECT * FROM Users WHERE Email = @Email", new { Email = email });
  }

  public async Task<string> CreateUser(User user)
  {
    using var connection = new SqliteConnection(_connectionString);
    var sql = @"INSERT INTO Users (Id, Name, Email, Password, Role, CreatedAt)
                    VALUES (@Id, @Name, @Email, @Password, @Role, @CreatedAt)";

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

  public async Task<bool> UpdateUser(User user)
  {
    using var connection = new SqliteConnection(_connectionString);
    var result = await connection.ExecuteAsync(
        "UPDATE Users SET Name = @Name, Email = @Email WHERE Id = @Id",
        user);
    return result > 0;
  }

  public async Task<bool> DeleteUser(string id)
  {
    using var connection = new SqliteConnection(_connectionString);
    var result = await connection.ExecuteAsync(
        "DELETE FROM Users WHERE Id = @Id",
        new { Id = id });
    return result > 0;
  }
}
