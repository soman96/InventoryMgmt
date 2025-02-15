namespace InventoryMgmt.Models;

public class Cart
{
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public int Quantity { get; set; }
    public double Price { get; set; }
}