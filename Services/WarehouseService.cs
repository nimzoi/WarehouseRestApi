using System;
using System.Threading.Tasks;
using WebApplication2.Dto;
using WebApplication2.Exceptions;
using WebApplication2.Repositories;

namespace WebApplication2.Services
{
    public interface IWarehouseService
    {
        Task<int> RegisterProductInWarehouseAsync(RegisterProductInWarehouseRequestDTO dto);
        
    }

    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;

        public WarehouseService(IWarehouseRepository warehouseRepository, IProductRepository productRepository, IOrderRepository orderRepository)
        {
            _warehouseRepository = warehouseRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        public async Task<int> RegisterProductInWarehouseAsync(RegisterProductInWarehouseRequestDTO dto)
        {
            if (!await _productRepository.ProductExists(dto.IdProduct.Value))
                throw new NotFoundException("Product does not exist.");

            if (!await _warehouseRepository.WarehouseExists(dto.IdWarehouse.Value))
                throw new NotFoundException("Warehouse does not exist.");

            var order = await _orderRepository.GetOrderById(dto.IdOrder.Value);
            if (order == null || order.Amount < dto.Amount.Value || order.CreatedAt >= dto.CreatedAt.Value)
                throw new NotFoundException("Order does not meet the requirements or does not exist.");

            if (await _warehouseRepository.IsOrderFulfilled(order.Id))
                throw new ConflictException("Order has already been fulfilled.");

            await _orderRepository.UpdateOrderFulfilledAt(order.Id, DateTime.UtcNow);

            var price = order.Price * dto.Amount.Value;
            return await _warehouseRepository.AddProductToWarehouse(dto.IdWarehouse.Value, dto.IdProduct.Value, order.Id, price, DateTime.UtcNow);
        }
    }
}
