using WFT.Infra.Application.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WFT.Infra.Application.Contracts.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderAsync(Guid id);
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto> CreateOrderAsync(OrderDto orderDto);
        Task<OrderDto> UpdateOrderAsync(Guid id, OrderDto orderDto);
        Task DeleteOrderAsync(Guid id);
    }
} 