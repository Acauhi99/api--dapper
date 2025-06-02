namespace api__dapper.dtos
{
  public record CreateService(
    string Key,
    string Title,
    string Description,
    string FeaturesTitle,
    string DetailsTitle,
    string PackagesTitle,
    List<string> Features,
    List<string> Details,
    List<CreatePackageDto> Packages
  );

  public record UpdateService(
    string Title,
    string Description,
    string FeaturesTitle,
    string DetailsTitle,
    string PackagesTitle,
    List<string> Features,
    List<string> Details,
    List<CreatePackageDto> Packages
  );

  public record CreatePackageDto(
    string Name,
    string Price,
    bool IsPopular,
    int Order
  );

  public record ServiceDetailsResponse(
    string Id,
    string Key,
    string Title,
    string Description,
    string FeaturesTitle,
    List<string> Features,
    string DetailsTitle,
    List<string> Details,
    string PackagesTitle,
    List<PackageResponse> Packages
  );

  public record PackageResponse(
    string Id,
    string Name,
    string Price,
    bool IsPopular
  );
}
