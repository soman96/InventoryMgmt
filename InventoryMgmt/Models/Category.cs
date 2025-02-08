using System.ComponentModel.DataAnnotations;

namespace InventoryMgmt.Models;

public class Category
{
    public int CategoryId { get; set; }
    
    [Required]
    public required string Name { get; set; }
    
    // public string Slug { get; set; } -- may include later for sorting
    
    public string? Description { get; set; } // Can be null
    
}