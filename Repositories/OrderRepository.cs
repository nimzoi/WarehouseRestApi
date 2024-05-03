using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WebApplication2.Models;  

namespace WebApplication2.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> GetOrderById(int orderId);
        Task UpdateOrderFulfilledAt(int orderId, DateTime fulfilledAt);
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly IConfiguration _configuration;

        public OrderRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<Order> GetOrderById(int orderId)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();
            var command = new SqlCommand("SELECT IdOrder, IdProduct, Amount, Price, CreatedAt, FulfilledAt FROM [Order] WHERE IdOrder = @IdOrder", connection);
            command.Parameters.AddWithValue("@IdOrder", orderId);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Order
                {
                    Id = reader.GetInt32(reader.GetOrdinal("IdOrder")),
                    ProductId = reader.GetInt32(reader.GetOrdinal("IdProduct")),
                    Amount = reader.GetInt32(reader.GetOrdinal("Amount")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    FulfilledAt = reader.IsDBNull(reader.GetOrdinal("FulfilledAt")) ? null : reader.GetDateTime(reader.GetOrdinal("FulfilledAt"))
                };
            }
            return null;
        }

        public async Task UpdateOrderFulfilledAt(int orderId, DateTime fulfilledAt)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();
            var command = new SqlCommand("UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder", connection);
            command.Parameters.AddWithValue("@IdOrder", orderId);
            command.Parameters.AddWithValue("@FulfilledAt", fulfilledAt);

            await command.ExecuteNonQueryAsync();
        }
    }
}
