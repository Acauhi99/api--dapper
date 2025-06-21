namespace api__dapper.dtos
{
  public record CreateSell(string ServiceId, string PackageId, decimal Amount);
  public record UpdateSellStatus(int Status); // 0=Pending, 1=Completed, 2=Cancelled, 3=Refunded

  public record SellResponse(
    string Id,
    string UserId,
    string ServiceId,
    string PackageId,
    decimal Amount,
    int Status,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? UserName,
    string? ServiceTitle
  );

  // New DTOs for the updated contract
  public record SellItemDto(
    string Title,
    string Description,
    decimal UnitPrice,
    int Quantity
  );

  public record CreateSellOrderRequest(
    List<SellItemDto> Items,
    decimal Subtotal,
    decimal ServiceFee,
    decimal Total,
    string PaymentMethod
  );

  public record SellCreateResponse(
    string Id,
    string ServiceId,
    string PackageId,
    decimal Amount,
    int Status,
    DateTime CreatedAt,
    string ServiceTitle,
    string PaymentMethod
  );
}
