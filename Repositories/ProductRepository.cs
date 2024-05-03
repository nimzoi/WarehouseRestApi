using System.Data.SqlClient;

namespace WebApplication2.Repositories;

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
        await using var connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        await connection.OpenAsync();

        var query = "SELECT COUNT(1) FROM Product WHERE IdProduct = @IdProduct";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IdProduct", productId);

        var count = (int)await command.ExecuteScalarAsync();
        return count > 0;
    }
}
