using InventoryMgmt.Data;
using InventoryMgmt.Models;
using InventoryMgmt.Areas.ProductManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryMgmt.Areas.ProductManagement.Controllers;


[Area("ProductManagement")]
[Route("[area]/[controller]")]
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context; // Holds the database context

    // Dependency injection for the database within the constructor
    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("")]
    public IActionResult Index()
    {
        // Load categories along with products
        var categories = _context.Categories
            .Include(c => c.Products)
            .ToList();

        return View(categories);
    }
    
    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("Manage")]
    public IActionResult Manage()
    {
        // Get all the categories
        var categories = _context.Categories.ToList();
        return View(categories);
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("Create")]
    public IActionResult Create()
    {
        Console.WriteLine("Called Correct GET Controller");
        return View();
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Category category)
    {
        if (ModelState.IsValid)
        { 
            _context.Categories.Add(category); // Add new category
            _context.SaveChanges(); // Commit changes
            
            TempData["success"] = $"The category {category.Name} has been added successfully.";
            return RedirectToAction("Index"); // Redirect to Index (List of categories)
        }
        
        return View(category);
    }
    
    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("Edit/{id:int}")]
    public IActionResult Edit(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null)
            return NotFound();
        return View(category);
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("CategoryId, Name, Description")] Category category)
    {
        if (id != category.CategoryId)
        {
            return NotFound(); // ensures the id in the route matches the id in model
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Categories.Update(category); // update the category
                _context.SaveChanges(); // commit changes
            }
            catch (DbUpdateConcurrencyException)
            {
                if (CategoryExists(category.CategoryId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            
            TempData["success"] = $"The category {category.Name} has been edited successfully.";
            return RedirectToAction("Index");
        }

        return View(category);
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.CategoryId == id);
    }
    
    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("Delete/{id:int}")]
    public IActionResult Delete(int id)
    {
        // Get the specific product
        var category = _context.Categories.FirstOrDefault(p => p.CategoryId == id);

        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }
    
    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("Delete/{categoryId:int}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int categoryId)
    {
        var category = _context.Categories.Find(categoryId);

        if (category != null)
        {
            _context.Categories.Remove(category);
            _context.SaveChanges();
            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }
        
        return NotFound();
    }
    
}
