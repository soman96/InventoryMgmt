using InventoryMgmt.Data;
using InventoryMgmt.Areas.ProductManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace InventoryMgmt.Areas.ProductManagement.Controllers;

[Area("ProductManagement")]
[Route("[area]/[controller]")]
public class CategoryController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ApplicationDbContext context, ILogger<CategoryController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var categories = await _context.Categories.Include(c => c.Products).ToListAsync();
            return View(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading category list");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("Manage")]
    public async Task<IActionResult> Manage()
    {
        try
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Manage page for categories");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (!ModelState.IsValid)
            return View(category);

        try
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            TempData["success"] = $"The category {category.Name} has been added successfully.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            return category == null ? NotFound() : View(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Edit page for category {CategoryId}", id);
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CategoryId, Name, Description")] Category category)
    {
        if (id != category.CategoryId)
            return NotFound();

        if (!ModelState.IsValid)
            return View(category);

        try
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            TempData["success"] = $"The category {category.Name} has been edited successfully.";
            return RedirectToAction("Index");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (!CategoryExists(category.CategoryId))
            {
                _logger.LogWarning("Attempted to edit non-existent category with ID {CategoryId}", category.CategoryId);
                return NotFound();
            }
            _logger.LogError(ex, "Concurrency error while editing category {CategoryId}", category.CategoryId);
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", category.CategoryId);
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.CategoryId == id);
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            return category == null ? NotFound() : View(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Delete page for category {CategoryId}", id);
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("Delete/{categoryId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int categoryId)
    {
        try
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category deleted successfully!";
                return RedirectToAction("Index");
            }
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", categoryId);
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }
}
