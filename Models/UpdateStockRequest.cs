using System.ComponentModel.DataAnnotations;

public class UpdateStockRequest
{
    [Required]
    public int Quantity { get; set; } // Quantity to update
}
