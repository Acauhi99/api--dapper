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
}
