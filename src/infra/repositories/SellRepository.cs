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

    var sql = @"
      SELECT
        s.Id,
        s.UserId,
        s.ServiceId,
        s.PackageId,
        s.Amount,
        s.Status,
        s.CreatedAt,
        s.CompletedAt,
        u.Name as UserName,
        sv.Title as ServiceTitle
      FROM Sells s
      LEFT JOIN Users u ON s.UserId = u.Id
      LEFT JOIN Services sv ON s.ServiceId = sv.Id
      ORDER BY s.CreatedAt DESC";

    var results = await connection.QueryAsync<dynamic>(sql);

    return results.Select(r => new SellResponse(
      r.Id,
      r.UserId,
      r.ServiceId,
      r.PackageId,
      Convert.ToDecimal(r.Amount),
      Convert.ToInt32(r.Status),
      DateTime.Parse(r.CreatedAt),
      r.CompletedAt != null ? DateTime.Parse(r.CompletedAt) : null,
      r.UserName,
      r.ServiceTitle
    ));
  }

  public async Task<SellResponse?> GetSellById(string id)
  {
    using var connection = new SqliteConnection(_connectionString);

    var sql = @"
      SELECT
        s.Id,
        s.UserId,
        s.ServiceId,
        s.PackageId,
        s.Amount,
        s.Status,
        s.CreatedAt,
        s.CompletedAt,
        u.Name as UserName,
        sv.Title as ServiceTitle
      FROM Sells s
      LEFT JOIN Users u ON s.UserId = u.Id
      LEFT JOIN Services sv ON s.ServiceId = sv.Id
      WHERE s.Id = @Id";

    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id });

    if (result == null) return null;

    return new SellResponse(
      result.Id,
      result.UserId,
      result.ServiceId,
      result.PackageId,
      Convert.ToDecimal(result.Amount),
      Convert.ToInt32(result.Status),
      DateTime.Parse(result.CreatedAt),
      result.CompletedAt != null ? DateTime.Parse(result.CompletedAt) : null,
      result.UserName,
      result.ServiceTitle
    );
  }

  public async Task<IEnumerable<SellResponse>> GetSellsByUserId(string userId)
  {
    using var connection = new SqliteConnection(_connectionString);

    var sql = @"
      SELECT
        s.Id,
        s.UserId,
        s.ServiceId,
        s.PackageId,
        s.Amount,
        s.Status,
        s.CreatedAt,
        s.CompletedAt,
        u.Name as UserName,
        sv.Title as ServiceTitle
      FROM Sells s
      LEFT JOIN Users u ON s.UserId = u.Id
      LEFT JOIN Services sv ON s.ServiceId = sv.Id
      WHERE s.UserId = @UserId
      ORDER BY s.CreatedAt DESC";

    var results = await connection.QueryAsync<dynamic>(sql, new { UserId = userId });

    return results.Select(r => new SellResponse(
      r.Id,
      r.UserId,
      r.ServiceId,
      r.PackageId,
      Convert.ToDecimal(r.Amount),
      Convert.ToInt32(r.Status),
      DateTime.Parse(r.CreatedAt),
      r.CompletedAt != null ? DateTime.Parse(r.CompletedAt) : null,
      r.UserName,
      r.ServiceTitle
    ));
  }

  public async Task<bool> UpdateSellStatus(string id, SellStatus status)
  {
    using var connection = new SqliteConnection(_connectionString);
    var result = await connection.ExecuteAsync("UPDATE Sells SET Status = @Status WHERE Id = @Id", new { Id = id, Status = status });
    return result > 0;
  }
}
