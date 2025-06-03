using Dapper;
using Microsoft.Data.Sqlite;
using api__dapper.domain.models;
using api__dapper.dtos;

namespace api__dapper.infra.repositories;

public interface ISellRepository
{
  Task<IEnumerable<SellResponse>> GetAllSells();
  Task<IEnumerable<SellResponse>> GetSellsByUserId(string userId);
  Task<SellResponse?> GetSellById(string id);
  Task<string> CreateSell(Sell sell);
  Task<bool> UpdateSellStatus(string id, SellStatus status);
  Task<bool> DeleteSell(string id);
}

public class SellRepository(IConfiguration configuration) : ISellRepository
{
  private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new ArgumentNullException(nameof(configuration), "A connection string 'DefaultConnection' não foi encontrada.");

  Task<string> ISellRepository.CreateSell(Sell sell)
  {
    throw new NotImplementedException();
  }

  Task<bool> ISellRepository.DeleteSell(string id)
  {
    throw new NotImplementedException();
  }

  Task<IEnumerable<SellResponse>> ISellRepository.GetAllSells()
  {
    throw new NotImplementedException();
  }

  Task<SellResponse?> ISellRepository.GetSellById(string id)
  {
    throw new NotImplementedException();
  }

  Task<IEnumerable<SellResponse>> ISellRepository.GetSellsByUserId(string userId)
  {
    throw new NotImplementedException();
  }

  Task<bool> ISellRepository.UpdateSellStatus(string id, SellStatus status)
  {
    throw new NotImplementedException();
  }
}
