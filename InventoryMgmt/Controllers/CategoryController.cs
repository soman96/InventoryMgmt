using InventoryMgmt.Data;
using InventoryMgmt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryMgmt.Controllers;

public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context; // Holds the database context

    // Dependency injection for the database within the constructor
    public CategoryController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        // Get all the categories
        var categories = _context.Categories.ToList();
        return View(categories);
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(Category category)
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
    
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var category = _context.Categories.Find(id);
        if (category == null)
            return NotFound();
        return View(category);
    }

    [HttpPost]
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
            }

            return RedirectToAction("Index");
        }

        return View(category);
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.CategoryId == id);
    }
    
}
