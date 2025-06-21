namespace api__dapper.domain.models
{
  public class Sell
  {
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public SellStatus Status { get; set; } = SellStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string PaymentMethod { get; set; } = "PIX";

    // Navigation properties (opcional para queries)
    public User? User { get; set; }
    public Service? Service { get; set; }
  }

  public enum SellStatus
  {
    Pending,
    Completed,
    Cancelled,
    Refunded
  }
}
