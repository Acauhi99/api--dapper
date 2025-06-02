using System.Data;

using api__dapper.domain.models;

using Dapper;

using Microsoft.Data.Sqlite;

namespace api__dapper.infra.repositories;

public interface IServiceRepository
{
  Task<IEnumerable<Service>> GetAllServices();
  Task<Service?> GetServiceById(string id);
  Task<Service?> GetServiceByKey(string key);
  Task<Service> CreateService(Service service);
  Task<bool> UpdateService(Service service);
  Task<bool> DeleteService(string id);
}

public class ServiceRepository(IConfiguration configuration) : IServiceRepository
{
  private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

  Task<Service> IServiceRepository.CreateService(Service service)
  {
    throw new NotImplementedException();
  }

  Task<bool> IServiceRepository.DeleteService(string id)
  {
    throw new NotImplementedException();
  }

  Task<IEnumerable<Service>> IServiceRepository.GetAllServices()
  {
    throw new NotImplementedException();
  }

  Task<Service?> IServiceRepository.GetServiceById(string id)
  {
    throw new NotImplementedException();
  }

  Task<Service?> IServiceRepository.GetServiceByKey(string key)
  {
    throw new NotImplementedException();
  }

  Task<bool> IServiceRepository.UpdateService(Service service)
  {
    throw new NotImplementedException();
  }
}
