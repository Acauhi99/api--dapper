namespace api__dapper.dtos
{
  // Client Dashboard DTOs
  public record OrderHistoryItem(
      string Id,
      string OrderCode,
      string ServiceName,
      decimal Price,
      DateTime Date,
      string Status
  );

  public record ClientOrderHistoryResponse(
      List<OrderHistoryItem> Orders,
      int TotalCount,
      decimal TotalAmount
  );

  // Admin Dashboard DTOs
  public record AdminOrderItem(
      string Id,
      string OrderCode,
      string ClientId,
      string ClientName,
      string ServiceName,
      decimal Price,
      DateTime Date,
      string Status
  );

  public record AdminOrdersResponse(
      List<AdminOrderItem> Orders,
      int Page,
      int PageSize,
      int TotalPages,
      int TotalCount,
      decimal TotalAmount
  );

  public record TopServiceItem(
      string ServiceName,
      int Count,
      decimal Revenue
  );

  public record OrderSummaryResponse(
      string Period,
      int Year,
      int? Month,
      int OrderCount,
      decimal TotalRevenue,
      List<TopServiceItem> TopServices
  );

  // Query filter DTOs
  public record OrderQueryFilter(
      DateTime? StartDate,
      DateTime? EndDate,
      string? OrderCode,
      string? ServiceName,
      string? ClientName,
      int Page = 1,
      int PageSize = 10
  );

  public record SummaryQueryFilter(
      string Period = "month",
      int? Month = null,
      int? Year = null
  );
}
