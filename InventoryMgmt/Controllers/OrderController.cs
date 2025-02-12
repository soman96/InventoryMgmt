using InventoryMgmt.Data;
using InventoryMgmt.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryMgmt.Controllers;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context; // Holds the database context

    // Dependency injection
    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Add(int id)
    {
        var product = _context.Products.Find(id);
        if (product == null)
            return NotFound();
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(Product product)
    {
        Order order = new Order();
        order.Products.Add(product);
        order.OrderDate = DateTime.Now;
        order.Total += product.Price * order.Quantity;
        
        if (ModelState.IsValid)
        { 
            _context.Orders.Add(order); // Add new category
            _context.SaveChanges(); // Commit changes
            
            TempData["success"] = $"The product {product.ProductName} has been added successfully to your order.";
            
            return RedirectToAction("Index"); // Redirect to Index (List of categories)
        }
        return View(product);
    }

}