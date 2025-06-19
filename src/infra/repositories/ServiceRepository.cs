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
  private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new ArgumentNullException(nameof(configuration), "A connection string 'DefaultConnection' não foi encontrada.");

  public async Task<IEnumerable<Service>> GetAllServices()
  {
    using var connection = new SqliteConnection(_connectionString);

    var sql = @"
      SELECT * FROM Services;
      SELECT * FROM ServiceFeatures ORDER BY [Order];
      SELECT * FROM ServiceDetails ORDER BY [Order];
      SELECT * FROM Packages ORDER BY [Order];";

    using var multi = await connection.QueryMultipleAsync(sql);

    var services = (await multi.ReadAsync<Service>()).ToList();
    var features = (await multi.ReadAsync<ServiceFeature>()).ToList();
    var details = (await multi.ReadAsync<ServiceDetail>()).ToList();
    var packages = (await multi.ReadAsync<Package>()).ToList();

    foreach (var service in services)
    {
      service.Features = features.Where(f => f.ServiceId == service.Id).OrderBy(f => f.Order).ToList();
      service.Details = details.Where(d => d.ServiceId == service.Id).OrderBy(d => d.Order).ToList();
      service.Packages = packages.Where(p => p.ServiceId == service.Id).OrderBy(p => p.Order).ToList();
    }

    return services;
  }

  public async Task<Service?> GetServiceById(string id)
  {
    using var connection = new SqliteConnection(_connectionString);

    var sql = @"
      SELECT * FROM Services WHERE Id = @Id;
      SELECT * FROM ServiceFeatures WHERE ServiceId = @Id ORDER BY [Order];
      SELECT * FROM ServiceDetails WHERE ServiceId = @Id ORDER BY [Order];
      SELECT * FROM Packages WHERE ServiceId = @Id ORDER BY [Order];";

    using var multi = await connection.QueryMultipleAsync(sql, new { Id = id });

    var service = await multi.ReadFirstOrDefaultAsync<Service>();
    if (service == null) return null;

    service.Features = (await multi.ReadAsync<ServiceFeature>()).ToList();
    service.Details = (await multi.ReadAsync<ServiceDetail>()).ToList();
    service.Packages = (await multi.ReadAsync<Package>()).ToList();

    return service;
  }

  public async Task<Service?> GetServiceByKey(string key)
  {
    using var connection = new SqliteConnection(_connectionString);

    var sql = @"
      SELECT * FROM Services WHERE Key = @Key;
      SELECT sf.* FROM ServiceFeatures sf
      INNER JOIN Services s ON sf.ServiceId = s.Id
      WHERE s.Key = @Key ORDER BY sf.[Order];
      SELECT sd.* FROM ServiceDetails sd
      INNER JOIN Services s ON sd.ServiceId = s.Id
      WHERE s.Key = @Key ORDER BY sd.[Order];
      SELECT p.* FROM Packages p
      INNER JOIN Services s ON p.ServiceId = s.Id
      WHERE s.Key = @Key ORDER BY p.[Order];";

    using var multi = await connection.QueryMultipleAsync(sql, new { Key = key });

    var service = await multi.ReadFirstOrDefaultAsync<Service>();
    if (service == null) return null;

    service.Features = (await multi.ReadAsync<ServiceFeature>()).ToList();
    service.Details = (await multi.ReadAsync<ServiceDetail>()).ToList();
    service.Packages = (await multi.ReadAsync<Package>()).ToList();

    return service;
  }

  public async Task<Service> CreateService(Service service)
  {
    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();
    using var transaction = connection.BeginTransaction();

    try
    {

      var serviceSql = @"INSERT INTO Services (Id, Key, Title, Description, FeaturesTitle, DetailsTitle, PackagesTitle, CreatedAt, UpdatedAt)
                        VALUES (@Id, @Key, @Title, @Description, @FeaturesTitle, @DetailsTitle, @PackagesTitle, @CreatedAt, @UpdatedAt)";

      await connection.ExecuteAsync(serviceSql, service, transaction);

      if (service.Features.Any())
      {
        var featureSql = @"INSERT INTO ServiceFeatures (Id, ServiceId, Text, [Order])
                          VALUES (@Id, @ServiceId, @Text, @Order)";
        await connection.ExecuteAsync(featureSql, service.Features, transaction);
      }

      if (service.Details.Any())
      {
        var detailSql = @"INSERT INTO ServiceDetails (Id, ServiceId, Text, [Order])
                         VALUES (@Id, @ServiceId, @Text, @Order)";
        await connection.ExecuteAsync(detailSql, service.Details, transaction);
      }

      if (service.Packages.Any())
      {
        var packageSql = @"INSERT INTO Packages (Id, ServiceId, Name, Price, IsPopular, [Order], CreatedAt)
                          VALUES (@Id, @ServiceId, @Name, @Price, @IsPopular, @Order, @CreatedAt)";
        await connection.ExecuteAsync(packageSql, service.Packages, transaction);
      }

      transaction.Commit();
      return service;
    }
    catch
    {
      transaction.Rollback();
      throw;
    }
  }

  public async Task<bool> UpdateService(Service service)
  {
    using var connection = new SqliteConnection(_connectionString);
    await connection.OpenAsync();
    using var transaction = connection.BeginTransaction();

    try
    {
      var serviceSql = @"UPDATE Services SET
                        Title = @Title,
                        Description = @Description,
                        FeaturesTitle = @FeaturesTitle,
                        DetailsTitle = @DetailsTitle,
                        PackagesTitle = @PackagesTitle,
                        UpdatedAt = @UpdatedAt
                        WHERE Id = @Id";

      var result = await connection.ExecuteAsync(serviceSql, service, transaction);
      if (result == 0) return false;

      // Delete existing related data
      await connection.ExecuteAsync("DELETE FROM ServiceFeatures WHERE ServiceId = @Id", new { service.Id }, transaction);
      await connection.ExecuteAsync("DELETE FROM ServiceDetails WHERE ServiceId = @Id", new { service.Id }, transaction);
      await connection.ExecuteAsync("DELETE FROM Packages WHERE ServiceId = @Id", new { service.Id }, transaction);

      // Insert updated features
      if (service.Features.Any())
      {
        var featureSql = @"INSERT INTO ServiceFeatures (Id, ServiceId, Text, [Order])
                          VALUES (@Id, @ServiceId, @Text, @Order)";
        await connection.ExecuteAsync(featureSql, service.Features, transaction);
      }

      // Insert updated details
      if (service.Details.Any())
      {
        var detailSql = @"INSERT INTO ServiceDetails (Id, ServiceId, Text, [Order])
                       VALUES (@Id, @ServiceId, @Text, @Order)";
        await connection.ExecuteAsync(detailSql, service.Details, transaction);
      }

      // Insert updated packages
      if (service.Packages.Any())
      {
        var packageSql = @"INSERT INTO Packages (Id, ServiceId, Name, Price, IsPopular, [Order], CreatedAt)
                        VALUES (@Id, @ServiceId, @Name, @Price, @IsPopular, @Order, @CreatedAt)";
        await connection.ExecuteAsync(packageSql, service.Packages, transaction);
      }

      transaction.Commit();
      return true;
    }
    catch
    {
      transaction.Rollback();
      throw;
    }
  }

  public async Task<bool> DeleteService(string id)
  {
    using var connection = new SqliteConnection(_connectionString);
    var result = await connection.ExecuteAsync(
        "DELETE FROM Services WHERE Id = @Id",
        new { Id = id });
    return result > 0;
  }
}
