using api__dapper.domain.models;
using api__dapper.dtos;
using api__dapper.infra.repositories;

using NanoidDotNet;

namespace api__dapper.domain.services;

public interface ISellService
{
  Task<IEnumerable<SellResponse>> GetAllSells();
  Task<IEnumerable<SellResponse>> GetSellsByUserId(string userId);
  Task<SellResponse?> GetSellById(string id);
  Task<SellResponse> CreateSell(string userId, CreateSell sellDto);
  Task<SellResponse?> UpdateSellStatus(string id, UpdateSellStatus statusDto);
  Task<bool> DeleteSell(string id);
}

public class SellService(ISellRepository repository) : ISellService
{
  private readonly ISellRepository _repository = repository;

  Task<SellResponse> ISellService.CreateSell(string userId, CreateSell sellDto)
  {
    throw new NotImplementedException();
  }

  Task<bool> ISellService.DeleteSell(string id)
  {
    throw new NotImplementedException();
  }

  Task<IEnumerable<SellResponse>> ISellService.GetAllSells()
  {
    throw new NotImplementedException();
  }

  Task<SellResponse?> ISellService.GetSellById(string id)
  {
    throw new NotImplementedException();
  }

  Task<IEnumerable<SellResponse>> ISellService.GetSellsByUserId(string userId)
  {
    throw new NotImplementedException();
  }

  Task<SellResponse?> ISellService.UpdateSellStatus(string id, UpdateSellStatus statusDto)
  {
    throw new NotImplementedException();
  }
}
