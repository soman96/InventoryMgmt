using System.ComponentModel.DataAnnotations;

namespace InventoryMgmt.Models;

public class Order
{
    public int OrderId { get; set; }
    
    [Required]
    public required string CustomerName { get; set; }
    
    [DataType(DataType.Date)]
    public DateTime OrderDate { get; set; }
    
    public double Total { get; set; }
    
    // public List<Product> Products { get; set; } = new List<Product>();
    
}