using Dapper;
using Microsoft.Data.Sqlite;
using api__dapper.domain.models;
using api__dapper.dtos;

namespace api__dapper.infra.repositories;

public interface ISellRepository
{
  Task<IEnumerable<SellResponse>> GetAllSells();
  Task<IEnumerable<SellResponse>> GetSellsByUserId(string userId);
  Task<SellResponse?> GetSellById(string id);
  Task<string> CreateSell(Sell sell);
  Task<bool> UpdateSellStatus(string id, SellStatus status);
  Task<bool> DeleteSell(string id);
}

public class SellRepository(IConfiguration configuration) : ISellRepository
{
  private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new ArgumentNullException(nameof(configuration), "A connection string 'DefaultConnection' não foi encontrada.");

  public async Task<string> CreateSell(Sell sell)
  {
    using var connection = new SqliteConnection(_connectionString);
    var sql = @"INSERT INTO Sells (Id, UserId, ServiceId, PackageId, Amount, Status, CreatedAt, CompletedAt)
                    VALUES (@Id, @UserId, @ServiceId, @PackageId, @Amount, @Status, @CreatedAt, @CompletedAt)";

    await connection.ExecuteAsync(sql, sell);
    return sell.Id;
  }

  public async Task<bool> DeleteSell(string id)
  {
    using var connection = new SqliteConnection(_connectionString);
    var result = await connection.ExecuteAsync("DELETE FROM Sells WHERE Id = @Id", new { Id = id });
    return result > 0;
  }

  public async Task<IEnumerable<SellResponse>> GetAllSells()
  {
    using var connection = new SqliteConnection(_connectionString);
    return await connection.QueryAsync<SellResponse>("SELECT * FROM Sells");
  }

  public async Task<SellResponse?> GetSellById(string id)
  {
    using var connection = new SqliteConnection(_connectionString);
    return await connection.QueryFirstOrDefaultAsync<SellResponse>("SELECT * FROM Sells WHERE Id = @Id", new { Id = id });
  }

  public async Task<IEnumerable<SellResponse>> GetSellsByUserId(string userId)
  {
    using var connection = new SqliteConnection(_connectionString);
    return await connection.QueryAsync<SellResponse>("SELECT * FROM Sells WHERE UserId = @UserId", new { UserId = userId });
  }

  public async Task<bool> UpdateSellStatus(string id, SellStatus status)
  {
    using var connection = new SqliteConnection(_connectionString);
    var result = await connection.ExecuteAsync("UPDATE Sells SET Status = @Status WHERE Id = @Id", new { Id = id, Status = status });
    return result > 0;
  }
}
