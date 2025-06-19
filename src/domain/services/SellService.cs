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

  public async Task<SellResponse> CreateSell(string userId, CreateSell sellDto)
  {
    var sell = new Sell
    {
      Id = Nanoid.Generate(),
      UserId = userId,
      ServiceId = sellDto.ServiceId,
      PackageId = sellDto.PackageId,
      Amount = sellDto.Amount,
      Status = SellStatus.Pending,
      CreatedAt = DateTime.UtcNow
    };

    await _repository.CreateSell(sell);

    return new SellResponse
    (
      Id: sell.Id,
      UserId: sell.UserId,
      ServiceId: sell.ServiceId,
      PackageId: sell.PackageId,
      Amount: sell.Amount,
      Status: (int)sell.Status,
      CreatedAt: sell.CreatedAt,
      CompletedAt: sell.CompletedAt,
      UserName: null,
      ServiceTitle: null
    );
  }

  public async Task<bool> DeleteSell(string id)
  {
    return await _repository.DeleteSell(id);
  }

  public async Task<IEnumerable<SellResponse>> GetAllSells()
  {
    return await _repository.GetAllSells();
  }

  public async Task<SellResponse?> GetSellById(string id)
  {
    return await _repository.GetSellById(id);
  }

  public async Task<IEnumerable<SellResponse>> GetSellsByUserId(string userId)
  {
    return await _repository.GetSellsByUserId(userId);
  }

  public async Task<SellResponse?> UpdateSellStatus(string id, UpdateSellStatus statusDto)
  {
    var sellResponse  = await _repository.GetSellById(id);
    if (sellResponse  == null)
    {
      return null;
    }

    await _repository.UpdateSellStatus(id, (SellStatus)statusDto.Status);

    sellResponse = await _repository.GetSellById(id);

    return sellResponse ;
  }
}
