using NanoidDotNet;

namespace api__dapper.domain.models
{
  public class Service
  {
    public string Id { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty; // gold, dungeons, raids, pvp, itens
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FeaturesTitle { get; set; } = string.Empty;
    public string DetailsTitle { get; set; } = string.Empty;
    public string PackagesTitle { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ServiceFeature> Features { get; set; } = new();
    public List<ServiceDetail> Details { get; set; } = new();
    public List<Package> Packages { get; set; } = new();
  }

  public class ServiceFeature
  {
    public string Id { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
  }

  public class ServiceDetail
  {
    public string Id { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
  }

  public class Package
  {
    public string Id { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public bool IsPopular { get; set; } = false;
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; }
  }
}
