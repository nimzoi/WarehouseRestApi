using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WebApplication2.Repositories
{
    public interface IOrderRepository
    {
        Task<bool> CheckOrderExists(int productId, int amount, DateTime createdAt);
        Task UpdateOrderFulfilledAt(int orderId, DateTime fulfilledAt);
        Task<int> GetOrderId(int productId, int amount, DateTime createdAt);
        Task<decimal> GetOrderPrice(int orderId);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly IConfiguration _configuration;

        public OrderRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> CheckOrderExists(int productId, int amount, DateTime createdAt)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT COUNT(1) FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt AND FulfilledAt IS NULL", connection);
            command.Parameters.AddWithValue("@IdProduct", productId);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            int result = (int)await command.ExecuteScalarAsync();
            return result > 0;
        }

        public async Task<int> GetOrderId(int productId, int amount, DateTime createdAt)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT IdOrder FROM [Order] WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt AND FulfilledAt IS NULL", connection);
            command.Parameters.AddWithValue("@IdProduct", productId);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            var reader = await command.ExecuteReaderAsync();
            if (reader.Read())
            {
                return reader.GetInt32(0);
            }
            return 0; // or throw an exception if not found
        }

        public async Task UpdateOrderFulfilledAt(int orderId, DateTime fulfilledAt)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder", connection);
            command.Parameters.AddWithValue("@IdOrder", orderId);
            command.Parameters.AddWithValue("@FulfilledAt", fulfilledAt);
            await command.ExecuteNonQueryAsync();
        }

        public async Task<decimal> GetOrderPrice(int orderId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT Price FROM [Order] WHERE IdOrder = @IdOrder", connection);
            command.Parameters.AddWithValue("@IdOrder", orderId);
            var result = await command.ExecuteScalarAsync();
            if (result != DBNull.Value)
            {
                return (decimal)result;
            }
            throw new Exception("Order not found or price is null.");
        }
    }
}
