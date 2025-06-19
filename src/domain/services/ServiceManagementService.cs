using api__dapper.domain.models;
using api__dapper.dtos;
using api__dapper.infra.repositories;
using NanoidDotNet;

namespace api__dapper.domain.services;

public interface IServiceManagementService
{
  Task<IEnumerable<ServiceDetailsResponse>> GetAllServices();
  Task<ServiceDetailsResponse?> GetServiceById(string id);
  Task<ServiceDetailsResponse?> GetServiceByKey(string key);
  Task<ServiceDetailsResponse> CreateService(CreateService serviceDto);
  Task<ServiceDetailsResponse?> UpdateService(string id, UpdateService serviceDto);
  Task<bool> DeleteService(string id);
}

public class ServiceManagementService(IServiceRepository repository) : IServiceManagementService
{
  private readonly IServiceRepository _repository = repository;

  public async Task<IEnumerable<ServiceDetailsResponse>> GetAllServices()
  {
    var services = await _repository.GetAllServices();
    return services.Select(MapToResponse);
  }

  public async Task<ServiceDetailsResponse?> GetServiceById(string id)
  {
    var service = await _repository.GetServiceById(id);
    return service == null ? null : MapToResponse(service);
  }

  public async Task<ServiceDetailsResponse?> GetServiceByKey(string key)
  {
    var service = await _repository.GetServiceByKey(key);
    return service == null ? null : MapToResponse(service);
  }

  public async Task<ServiceDetailsResponse> CreateService(CreateService serviceDto)
  {
    var service = new Service
    {
      Id = Nanoid.Generate(size: 12),
      Key = serviceDto.Key,
      Title = serviceDto.Title,
      Description = serviceDto.Description,
      FeaturesTitle = serviceDto.FeaturesTitle,
      DetailsTitle = serviceDto.DetailsTitle,
      PackagesTitle = serviceDto.PackagesTitle,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow,
      Features = serviceDto.Features.Select((text, index) => new ServiceFeature
      {
        Id = Nanoid.Generate(size: 12),
        ServiceId = "",
        Text = text,
        Order = index
      }).ToList(),
      Details = serviceDto.Details.Select((text, index) => new ServiceDetail
      {
        Id = Nanoid.Generate(size: 12),
        ServiceId = "",
        Text = text,
        Order = index
      }).ToList(),
      Packages = serviceDto.Packages.Select((pkg, index) => new Package
      {
        Id = Nanoid.Generate(size: 12),
        ServiceId = "",
        Name = pkg.Name,
        Price = pkg.Price,
        IsPopular = pkg.IsPopular,
        Order = pkg.Order,
        CreatedAt = DateTime.UtcNow
      }).ToList()
    };

    foreach (var feature in service.Features)
      feature.ServiceId = service.Id;

    foreach (var detail in service.Details)
      detail.ServiceId = service.Id;

    foreach (var package in service.Packages)
      package.ServiceId = service.Id;

    await _repository.CreateService(service);
    return MapToResponse(service);
  }

  public async Task<ServiceDetailsResponse?> UpdateService(string id, UpdateService serviceDto)
  {
    var existingService = await _repository.GetServiceById(id);
    if (existingService == null) return null;

    existingService.Title = serviceDto.Title;
    existingService.Description = serviceDto.Description;
    existingService.FeaturesTitle = serviceDto.FeaturesTitle;
    existingService.DetailsTitle = serviceDto.DetailsTitle;
    existingService.PackagesTitle = serviceDto.PackagesTitle;
    existingService.UpdatedAt = DateTime.UtcNow;

    existingService.Features = serviceDto.Features.Select((text, index) => new ServiceFeature
    {
      Id = Nanoid.Generate(size: 12),
      ServiceId = existingService.Id,
      Text = text,
      Order = index
    }).ToList();

    existingService.Details = serviceDto.Details.Select((text, index) => new ServiceDetail
    {
      Id = Nanoid.Generate(size: 12),
      ServiceId = existingService.Id,
      Text = text,
      Order = index
    }).ToList();

    existingService.Packages = serviceDto.Packages.Select((pkg, index) => new Package
    {
      Id = Nanoid.Generate(size: 12),
      ServiceId = existingService.Id,
      Name = pkg.Name,
      Price = pkg.Price,
      IsPopular = pkg.IsPopular,
      Order = pkg.Order,
      CreatedAt = DateTime.UtcNow
    }).ToList();

    var success = await _repository.UpdateService(existingService);
    return success ? MapToResponse(existingService) : null;
  }

  public async Task<bool> DeleteService(string id)
  {
    return await _repository.DeleteService(id);
  }

  private static ServiceDetailsResponse MapToResponse(Service service)
  {
    return new ServiceDetailsResponse(
      service.Id,
      service.Key,
      service.Title,
      service.Description,
      service.FeaturesTitle,
      service.Features.OrderBy(f => f.Order).Select(f => f.Text).ToList(),
      service.DetailsTitle,
      service.Details.OrderBy(d => d.Order).Select(d => d.Text).ToList(),
      service.PackagesTitle,
      service.Packages.OrderBy(p => p.Order).Select(p => new PackageResponse(
        p.Id,
        p.Name,
        p.Price,
        p.IsPopular
      )).ToList()
    );
  }
}
