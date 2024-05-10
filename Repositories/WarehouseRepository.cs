using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WebApplication2.Repositories
{
    public interface IWarehouseRepository
    {
        Task<bool> WarehouseExists(int warehouseId);
        Task<int> AddProductToWarehouse(int idWarehouse, int idProduct, int orderId, decimal price, DateTime createdAt);
        Task<bool> IsOrderFulfilled(int orderId);
        Task<int> AddProductUsingStoredProc(int dtoIdWarehouse, int dtoIdProduct, int orderId, int dtoAmount, DateTime dtoCreatedAt);
    }
    
    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly IConfiguration _configuration;

        public WarehouseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> WarehouseExists(int warehouseId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT COUNT(1) FROM Warehouse WHERE IdWarehouse = @IdWarehouse", connection);
            command.Parameters.AddWithValue("@IdWarehouse", warehouseId);
            int result = (int)await command.ExecuteScalarAsync();
            return result > 0;
        }

        public async Task<int> AddProductToWarehouse(int idWarehouse, int idProduct, int orderId, decimal price, DateTime createdAt)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Price, CreatedAt) VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Price, @CreatedAt); SELECT SCOPE_IDENTITY();", connection);
            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            command.Parameters.AddWithValue("@IdOrder", orderId);
            command.Parameters.AddWithValue("@Price", price);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            var insertedId = await command.ExecuteScalarAsync();
            return Convert.ToInt32(insertedId);
        }

        public async Task<bool> IsOrderFulfilled(int orderId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT COUNT(1) FROM Product_Warehouse WHERE IdOrder = @IdOrder", connection);
            command.Parameters.AddWithValue("@IdOrder", orderId);
            int result = (int)await command.ExecuteScalarAsync();
            return result > 0;
        }

        public async Task<int> AddProductUsingStoredProc(int idWarehouse, int idProduct, int idOrder, int amount, DateTime createdAt)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            using var command = new SqlCommand("AddProductToWarehouse", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);

            var insertedId = await command.ExecuteScalarAsync();
            return Convert.ToInt32(insertedId);
        }
    }
}
