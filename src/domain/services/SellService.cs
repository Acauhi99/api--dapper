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
  Task<SellCreateResponse> CreateSellOrder(string userId, CreateSellOrderRequest request);
  Task<SellResponse?> UpdateSellStatus(string id, UpdateSellStatus statusDto);
  Task<bool> DeleteSell(string id);
}

public class SellService(ISellRepository repository, IServiceRepository serviceRepository) : ISellService
{
  private readonly ISellRepository _repository = repository;
  private readonly IServiceRepository _serviceRepository = serviceRepository;
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

  public async Task<SellCreateResponse> CreateSellOrder(string userId, CreateSellOrderRequest request)
  {
    // Validação inicial
    if (request.Items == null || !request.Items.Any())
      throw new ArgumentException("É necessário pelo menos um item no pedido");

    var firstItem = request.Items.First();

    var services = await _serviceRepository.GetAllServices();
    var service = services.FirstOrDefault(s =>
        s.Title.Contains(firstItem.Title, StringComparison.OrdinalIgnoreCase) ||
        firstItem.Title.Contains(s.Title, StringComparison.OrdinalIgnoreCase));

    if (service == null)
      throw new InvalidOperationException($"Não foi possível encontrar um serviço para: {firstItem.Title}");

    // Encontrar um pacote adequado com base no preço ou nome
    var package = service.Packages.FirstOrDefault(p =>
        p.Name.Contains(firstItem.Description, StringComparison.OrdinalIgnoreCase) ||
        decimal.TryParse(p.Price.Replace("R$ ", "").Replace(",", "."), out var price) &&
        Math.Abs(price - firstItem.UnitPrice) < 0.01m);

    if (package == null && service.Packages.Any())
      package = service.Packages.First();

    if (package == null)
      throw new InvalidOperationException("Não foi possível determinar o pacote para este serviço");

    string sellId = Nanoid.Generate();

    // Criar o registro de venda com dados reais
    var sell = new Sell
    {
      Id = sellId,
      UserId = userId,
      ServiceId = service.Id,
      PackageId = package.Id,
      Amount = request.Total,
      Status = SellStatus.Pending,
      CreatedAt = DateTime.UtcNow,
      PaymentMethod = request.PaymentMethod
    };

    await _repository.CreateSell(sell);

    return new SellCreateResponse(
        Id: sell.Id,
        ServiceId: sell.ServiceId,
        PackageId: sell.PackageId,
        Amount: sell.Amount,
        Status: (int)sell.Status,
        CreatedAt: sell.CreatedAt,
        ServiceTitle: service.Title,
        PaymentMethod: sell.PaymentMethod
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
    var sellResponse = await _repository.GetSellById(id);
    if (sellResponse == null)
    {
      return null;
    }

    await _repository.UpdateSellStatus(id, (SellStatus)statusDto.Status);

    sellResponse = await _repository.GetSellById(id);

    return sellResponse;
  }
}
