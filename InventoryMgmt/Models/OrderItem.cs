using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryMgmt.Models;

public class OrderItem
{
    public int OrderItemId { get; set; }
    
    [Required]
    public int OrderId { get; set; }
    
    [ForeignKey("OrderId")]
    public Order Order { get; set; } = null!;
    
    [Required]
    public int ProductId { get; set; }
    
    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;
    
    [Required]
    public int Quantity { get; set; }
    
    [Required]
    public double Price { get; set; }
}