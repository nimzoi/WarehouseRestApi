using System.Data;
using System.Data.SqlClient;

namespace WebApplication2.Repositories
{
    public interface IWarehouseRepository
    {
        Task<int?> RegisterProductInWarehouseAsync(int idWarehouse, int idProduct, int idOrder, DateTime createdAt);
        Task RegisterProductInWarehouseByProcedureAsync(int idWarehouse, int idProduct, DateTime createdAt);
        Task<bool> WarehouseExists(int warehouseId);
        Task<bool> ProductExists(int productId);
        Task<int> AddProductToWarehouse(int idWarehouse, int idProduct, int idOrder, decimal price, DateTime createdAt);
        Task<bool> IsOrderFulfilled(int orderId);
    }

    public class WarehouseRepository : IWarehouseRepository
    {
        private readonly IConfiguration _configuration;

        public WarehouseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int?> RegisterProductInWarehouseAsync(int idWarehouse, int idProduct, int idOrder, DateTime createdAt)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();
            await using SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync(); // Jawne rzutowanie na SqlTransaction

            try
            {
                var command = new SqlCommand("UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder", connection, transaction);
                command.Parameters.AddWithValue("@IdOrder", idOrder);
                command.Parameters.AddWithValue("@FulfilledAt", DateTime.UtcNow);
                await command.ExecuteNonQueryAsync();

                command.CommandText = @"
            INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, CreatedAt, Amount, Price)
            OUTPUT Inserted.IdProductWarehouse
            VALUES (@IdWarehouse, @IdProduct, @IdOrder, @CreatedAt, 0, 0);";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
                command.Parameters.AddWithValue("@IdProduct", idProduct);
                command.Parameters.AddWithValue("@IdOrder", idOrder);
                command.Parameters.AddWithValue("@CreatedAt", createdAt);
                var idProductWarehouse = (int)await command.ExecuteScalarAsync();

                await transaction.CommitAsync();
                return idProductWarehouse;
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
        }


        public async Task RegisterProductInWarehouseByProcedureAsync(int idWarehouse, int idProduct, DateTime createdAt)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand("AddProductToWarehouse", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            await command.ExecuteNonQueryAsync();
        }
        public async Task<int> AddProductToWarehouse(int idWarehouse, int idProduct, int idOrder, decimal price, DateTime createdAt)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();
            var command = new SqlCommand("INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Price, CreatedAt) VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Price, @CreatedAt); SELECT SCOPE_IDENTITY();", connection);
            command.Parameters.AddWithValue("@IdWarehouse", idWarehouse);
            command.Parameters.AddWithValue("@IdProduct", idProduct);
            command.Parameters.AddWithValue("@IdOrder", idOrder);
            command.Parameters.AddWithValue("@Price", price);
            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }

        public async Task<bool> WarehouseExists(int warehouseId)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand("SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse", connection);
            command.Parameters.AddWithValue("@IdWarehouse", warehouseId);
            int result = (int)await command.ExecuteScalarAsync();
            return result > 0;
        }

        public async Task<bool> ProductExists(int productId)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand("SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct", connection);
            command.Parameters.AddWithValue("@IdProduct", productId);
            int result = (int)await command.ExecuteScalarAsync();
            return result > 0;
        }

        public async Task<bool> IsOrderFulfilled(int orderId)
        {
            await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            await connection.OpenAsync();

            await using var command = new SqlCommand("SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder", connection);
            command.Parameters.AddWithValue("@IdOrder", orderId);
            int result = (int)await command.ExecuteScalarAsync();
            return result > 0;
        }
    }
}
