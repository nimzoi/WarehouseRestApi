using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
namespace WebApplication2.Repositories
{
    public interface IProductRepository
    {
        Task<bool> ProductExists(int productId);
    }

    
        public class ProductRepository : IProductRepository
        {
            private readonly IConfiguration _configuration;
            public ProductRepository(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public async Task<bool> ProductExists(int productId)
            {
                using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                await connection.OpenAsync();
                var command = new SqlCommand("SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct", connection);
                command.Parameters.AddWithValue("@IdProduct", productId);
                int result = (int)await command.ExecuteScalarAsync();
                return result > 0;
            }
        }
    }
