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
  Task<ClientOrderHistoryResponse> GetClientOrderHistory(string userId, OrderQueryFilter filter);
  Task<AdminOrdersResponse> GetAdminOrdersList(OrderQueryFilter filter);
  Task<OrderSummaryResponse> GetOrdersSummary(SummaryQueryFilter filter);
}

public class SellRepository(IConfiguration configuration) : ISellRepository
{
  private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new ArgumentNullException(nameof(configuration), "A connection string 'DefaultConnection' não foi encontrada.");

  public async Task<string> CreateSell(Sell sell)
  {
    using var connection = new SqliteConnection(_connectionString);
    var sql = @"INSERT INTO Sells (Id, UserId, ServiceId, PackageId, Amount, Status, CreatedAt, CompletedAt, PaymentMethod)
                VALUES (@Id, @UserId, @ServiceId, @PackageId, @Amount, @Status, @CreatedAt, @CompletedAt, @PaymentMethod)";

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

  public async Task<ClientOrderHistoryResponse> GetClientOrderHistory(string userId, OrderQueryFilter filter)
  {
    using var connection = new SqliteConnection(_connectionString);

    var whereClause = "WHERE s.UserId = @UserId";
    var parameters = new DynamicParameters();
    parameters.Add("UserId", userId);

    // Adicionar filtros opcionais
    if (filter.StartDate.HasValue)
    {
      whereClause += " AND s.CreatedAt >= @StartDate";
      parameters.Add("StartDate", filter.StartDate.Value.ToString("o"));
    }

    if (filter.EndDate.HasValue)
    {
      whereClause += " AND s.CreatedAt <= @EndDate";
      parameters.Add("EndDate", filter.EndDate.Value.ToString("o"));
    }

    if (!string.IsNullOrEmpty(filter.OrderCode))
    {
      whereClause += " AND s.Id LIKE @OrderCode";
      parameters.Add("OrderCode", $"%{filter.OrderCode}%");
    }

    if (!string.IsNullOrEmpty(filter.ServiceName))
    {
      whereClause += " AND srv.Title LIKE @ServiceName";
      parameters.Add("ServiceName", $"%{filter.ServiceName}%");
    }

    var query = $@"
        SELECT
            s.Id,
            s.Id as OrderCode,
            srv.Title as ServiceName,
            CAST(s.Amount as DECIMAL(10,2)) as Price,
            s.CreatedAt as Date,
            CASE
                WHEN s.Status = 0 THEN 'Pending'
                WHEN s.Status = 1 THEN 'Completed'
                WHEN s.Status = 2 THEN 'Cancelled'
                WHEN s.Status = 3 THEN 'Refunded'
                ELSE 'Unknown'
            END as Status
        FROM Sells s
        JOIN Services srv ON s.ServiceId = srv.Id
        {whereClause}
        ORDER BY s.CreatedAt DESC";

    var ordersRaw = await connection.QueryAsync<dynamic>(query, parameters);

    var ordersList = ordersRaw.Select(r => new OrderHistoryItem(
        Id: r.Id,
        OrderCode: r.OrderCode,
        ServiceName: r.ServiceName,
        Price: Convert.ToDecimal(r.Price),
        Date: DateTime.Parse(r.Date),
        Status: r.Status
    )).ToList();

    // Calcula totais
    var totalCountQuery = $@"
        SELECT COUNT(*), SUM(CAST(s.Amount as DECIMAL(10,2)))
        FROM Sells s
        JOIN Services srv ON s.ServiceId = srv.Id
        {whereClause}";

    var totalResult = await connection.QueryFirstOrDefaultAsync<(int Count, decimal Sum)>(totalCountQuery, parameters);

    return new ClientOrderHistoryResponse(
        Orders: ordersList,
        TotalCount: totalResult.Count,
        TotalAmount: totalResult.Sum
    );
  }

  public async Task<AdminOrdersResponse> GetAdminOrdersList(OrderQueryFilter filter)
  {
    using var connection = new SqliteConnection(_connectionString);

    var whereClause = "WHERE 1=1";
    var parameters = new DynamicParameters();

    // Adicionar filtros opcionais
    if (filter.StartDate.HasValue)
    {
      whereClause += " AND s.CreatedAt >= @StartDate";
      parameters.Add("StartDate", filter.StartDate.Value.ToString("o"));
    }

    if (filter.EndDate.HasValue)
    {
      whereClause += " AND s.CreatedAt <= @EndDate";
      parameters.Add("EndDate", filter.EndDate.Value.ToString("o"));
    }

    if (!string.IsNullOrEmpty(filter.OrderCode))
    {
      whereClause += " AND s.Id LIKE @OrderCode";
      parameters.Add("OrderCode", $"%{filter.OrderCode}%");
    }

    if (!string.IsNullOrEmpty(filter.ClientName))
    {
      whereClause += " AND u.Name LIKE @ClientName";
      parameters.Add("ClientName", $"%{filter.ClientName}%");
    }

    if (!string.IsNullOrEmpty(filter.ServiceName))
    {
      whereClause += " AND srv.Title LIKE @ServiceName";
      parameters.Add("ServiceName", $"%{filter.ServiceName}%");
    }

    // Calcula totais para paginação
    var countQuery = $@"
        SELECT COUNT(*), SUM(CAST(s.Amount as DECIMAL(10,2)))
        FROM Sells s
        JOIN Users u ON s.UserId = u.Id
        JOIN Services srv ON s.ServiceId = srv.Id
        {whereClause}";

    var totalResult = await connection.QueryFirstOrDefaultAsync<(int Count, decimal Sum)>(countQuery, parameters);

    // Adiciona paginação
    var totalPages = (int)Math.Ceiling(totalResult.Count / (double)filter.PageSize);
    var offset = (filter.Page - 1) * filter.PageSize;

    var query = $@"
        SELECT
            s.Id,
            s.Id as OrderCode,
            s.UserId as ClientId,
            u.Name as ClientName,
            srv.Title as ServiceName,
            CAST(s.Amount as DECIMAL(10,2)) as Price,
            s.CreatedAt as Date,
            CASE
                WHEN s.Status = 0 THEN 'Pending'
                WHEN s.Status = 1 THEN 'Completed'
                WHEN s.Status = 2 THEN 'Cancelled'
                WHEN s.Status = 3 THEN 'Refunded'
                ELSE 'Unknown'
            END as Status
        FROM Sells s
        JOIN Users u ON s.UserId = u.Id
        JOIN Services srv ON s.ServiceId = srv.Id
        {whereClause}
        ORDER BY s.CreatedAt DESC
        LIMIT @PageSize OFFSET @Offset";

    parameters.Add("PageSize", filter.PageSize);
    parameters.Add("Offset", offset);

    var ordersRaw = await connection.QueryAsync<dynamic>(query, parameters);

    var ordersList = ordersRaw.Select(r => new AdminOrderItem(
        Id: r.Id,
        OrderCode: r.OrderCode,
        ClientId: r.ClientId,
        ClientName: r.ClientName,
        ServiceName: r.ServiceName,
        Price: Convert.ToDecimal(r.Price),
        Date: DateTime.Parse(r.Date),
        Status: r.Status
    )).ToList();

    return new AdminOrdersResponse(
        Orders: ordersList,
        Page: filter.Page,
        PageSize: filter.PageSize,
        TotalPages: totalPages,
        TotalCount: totalResult.Count,
        TotalAmount: totalResult.Sum
    );
  }

  public async Task<OrderSummaryResponse> GetOrdersSummary(SummaryQueryFilter filter)
  {
    using var connection = new SqliteConnection(_connectionString);

    // Determina o período base no filtro
    var year = filter.Year ?? DateTime.Now.Year;
    var month = filter.Month ?? DateTime.Now.Month;
    var isPeriodMonth = filter.Period.ToLower() == "month";

    var dateClause = isPeriodMonth
        ? $"strftime('%Y', s.CreatedAt) = '{year}' AND strftime('%m', s.CreatedAt) = '{month:D2}'"
        : $"strftime('%Y', s.CreatedAt) = '{year}'";

    // Consulta estatísticas gerais
    var statsQuery = $@"
        SELECT COUNT(*), SUM(CAST(s.Amount as DECIMAL(10,2)))
        FROM Sells s
        WHERE {dateClause}";

    var stats = await connection.QueryFirstOrDefaultAsync<(int Count, decimal Sum)>(statsQuery);

    var topServicesQuery = $@"
        SELECT
            srv.Title as ServiceName,
            COUNT(*) as Count,
            SUM(CAST(s.Amount as DECIMAL(10,2))) as Revenue
        FROM Sells s
        JOIN Services srv ON s.ServiceId = srv.Id
        WHERE {dateClause}
        GROUP BY srv.Id
        ORDER BY Revenue DESC
        LIMIT 5";

    var topServicesRaw = await connection.QueryAsync<dynamic>(topServicesQuery);

    var topServicesList = topServicesRaw.Select(r => new TopServiceItem(
        ServiceName: r.ServiceName,
        Count: Convert.ToInt32(r.Count),
        Revenue: Convert.ToDecimal(r.Revenue)
    )).ToList();

    return new OrderSummaryResponse(
        Period: filter.Period,
        Year: year,
        Month: isPeriodMonth ? month : null,
        OrderCount: stats.Count,
        TotalRevenue: stats.Sum,
        TopServices: topServicesList
    );
  }
}
