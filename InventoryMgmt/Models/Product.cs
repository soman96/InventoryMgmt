using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryMgmt.Models;

public class Product
{
    public int ProductId { get; set; }
    
    [Required]
    public required string ProductName { get; set; }
    
    [Required]
    public int CategoryId { get; set; }  // Foreign key

    [ForeignKey("CategoryId")]
    public Category? Category { get; set; } // Navigation property
    
    [Required]
    public required double Price { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public int LowStockThreshold { get; set; }

}