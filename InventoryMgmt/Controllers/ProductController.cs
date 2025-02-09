using InventoryMgmt.Data;
using InventoryMgmt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryMgmt.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _context; // Holds the database context
    
    // Dependency injection
    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        // var products = _context.Products.ToList();
        var products = _context.Products.Include(p => p.Category).ToList();
        return View(products);
    }
    
    [HttpGet]
    public IActionResult Add()
    {
        // Get all the categories and their IDs for the dropdown menu
        ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
        
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(Product product)
    {
        if (ModelState.IsValid)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
            
            TempData["Success"] = "Product added successfully";
            return RedirectToAction("Index");
        }
        
        // Debugging
        foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine($"Validation Error: {modelError.ErrorMessage}");
        }
        
        foreach (var key in ModelState.Keys)
        {
            foreach (var error in ModelState[key].Errors)
            {
                Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
            }
        }
        
        // Get the list of categories again
        ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
        return View(product);
    }
    
}