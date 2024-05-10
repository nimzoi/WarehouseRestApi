using System;
using System.Threading.Tasks;
using WebApplication2.Dto;
using WebApplication2.Repositories;
using WebApplication2.Exceptions;

namespace WebApplication2.Services
{
    public interface IWarehouseService
    {
        Task<int> RegisterProductInWarehouseAsync(RegisterProductRequestDTO dto);
        Task<int> RegisterProductUsingProcedureAsync(RegisterProductRequestDTO dto);
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

        public async Task<int> RegisterProductInWarehouseAsync(RegisterProductRequestDTO dto)
        {
            if (!await _productRepository.ProductExists(dto.IdProduct))
                throw new NotFoundException("Product does not exist.");
            if (!await _warehouseRepository.WarehouseExists(dto.IdWarehouse))
                throw new NotFoundException("Warehouse does not exist.");
            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than 0.");

            bool orderExists = await _orderRepository.CheckOrderExists(dto.IdProduct, dto.Amount, dto.CreatedAt);
            if (!orderExists)
                throw new NotFoundException("Order does not meet the requirements or does not exist.");

            int orderId = await _orderRepository.GetOrderId(dto.IdProduct, dto.Amount, dto.CreatedAt);
            if (await _warehouseRepository.IsOrderFulfilled(orderId))
                throw new ConflictException("Order has already been fulfilled.");

            await _orderRepository.UpdateOrderFulfilledAt(orderId, DateTime.UtcNow);
            decimal price = await _orderRepository.GetOrderPrice(orderId) * dto.Amount;
            return await _warehouseRepository.AddProductToWarehouse(dto.IdWarehouse, dto.IdProduct, orderId, price, DateTime.UtcNow);
        }


        public async Task<int> RegisterProductUsingProcedureAsync(RegisterProductRequestDTO dto)
        {
 
            if (!await _productRepository.ProductExists(dto.IdProduct))
                throw new NotFoundException("Product does not exist.");
            if (!await _warehouseRepository.WarehouseExists(dto.IdWarehouse))
                throw new NotFoundException("Warehouse does not exist.");
            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than 0.");

            bool orderExists = await _orderRepository.CheckOrderExists(dto.IdProduct, dto.Amount, dto.CreatedAt);
            if (!orderExists)
                throw new NotFoundException("Order does not meet the requirements or does not exist.");

            int orderId = await _orderRepository.GetOrderId(dto.IdProduct, dto.Amount, dto.CreatedAt);
            if (await _warehouseRepository.IsOrderFulfilled(orderId))
                throw new ConflictException("Order has already been fulfilled.");

            await _orderRepository.UpdateOrderFulfilledAt(orderId, DateTime.UtcNow);
            
            return await _warehouseRepository.AddProductUsingStoredProc(dto.IdWarehouse, dto.IdProduct, orderId, dto.Amount, dto.CreatedAt);
        }
    }


}
