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
  Task<ServiceDetailsResponse> IServiceManagementService.CreateService(CreateService serviceDto)
  {
    throw new NotImplementedException();
  }

  Task<bool> IServiceManagementService.DeleteService(string id)
  {
    throw new NotImplementedException();
  }

  Task<IEnumerable<ServiceDetailsResponse>> IServiceManagementService.GetAllServices()
  {
    throw new NotImplementedException();
  }

  Task<ServiceDetailsResponse?> IServiceManagementService.GetServiceById(string id)
  {
    throw new NotImplementedException();
  }

  Task<ServiceDetailsResponse?> IServiceManagementService.GetServiceByKey(string key)
  {
    throw new NotImplementedException();
  }

  Task<ServiceDetailsResponse?> IServiceManagementService.UpdateService(string id, UpdateService serviceDto)
  {
    throw new NotImplementedException();
  }
}
