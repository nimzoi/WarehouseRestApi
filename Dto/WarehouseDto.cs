using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Dto;

// Przykładowa klasa DTO dla zapytania
public class RegisterProductInWarehouseRequestDTO
{
    public int? IdProduct { get; set; }
    public int? IdWarehouse { get; set; }
    public int? Amount { get; set; }
    public DateTime? CreatedAt { get; set; }
}
