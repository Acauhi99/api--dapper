using api__dapper.dtos;
using api__dapper.infra.repositories;

namespace api__dapper.domain.services;

public interface IDashboardService
{
  Task<ClientOrderHistoryResponse> GetClientOrderHistory(string userId, OrderQueryFilter filter);
  Task<AdminOrdersResponse> GetAdminOrdersList(OrderQueryFilter filter);
  Task<OrderSummaryResponse> GetOrdersSummary(SummaryQueryFilter filter);
}

public class DashboardService(ISellRepository sellRepository) : IDashboardService
{
  private readonly ISellRepository _sellRepository = sellRepository;

  public async Task<ClientOrderHistoryResponse> GetClientOrderHistory(string userId, OrderQueryFilter filter)
  {
    return await _sellRepository.GetClientOrderHistory(userId, filter);
  }

  public async Task<AdminOrdersResponse> GetAdminOrdersList(OrderQueryFilter filter)
  {
    return await _sellRepository.GetAdminOrdersList(filter);
  }

  public async Task<OrderSummaryResponse> GetOrdersSummary(SummaryQueryFilter filter)
  {
    return await _sellRepository.GetOrdersSummary(filter);
  }
}
