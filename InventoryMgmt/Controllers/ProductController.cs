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
    public IActionResult Index(string searchQuery, int? categoryId, string sortBy)
    {
        var products = _context.Products.Include(p => p.Category).AsQueryable();

        // Searching (Case-Insensitive, Partial Match)
        if (!string.IsNullOrEmpty(searchQuery))
        {
            string lowerSearchQuery = searchQuery.ToLower();
            products = products.Where(p => p.ProductName.ToLower().Contains(lowerSearchQuery));
        }

        // Filtering by category
        if (categoryId.HasValue && categoryId > 0)
        {
            products = products.Where(p => p.CategoryId == categoryId);
        }

        // Sorting
        switch (sortBy)
        {
            case "price_asc":
                products = products.OrderBy(p => p.Price);
                break;
            case "price_desc":
                products = products.OrderByDescending(p => p.Price);
                break;
            case "name_asc":
                products = products.OrderBy(p => p.ProductName);
                break;
            case "name_desc":
                products = products.OrderByDescending(p => p.ProductName);
                break;
            default:
                products = products.OrderBy(p => p.ProductName);
                break;
        }

        ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
        ViewBag.SortBy = sortBy;
        ViewBag.SearchQuery = searchQuery;
        ViewBag.SelectedCategory = categoryId;

        return View(products.ToList());
    }
    
    [HttpGet]
    public IActionResult Manage()
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
            return RedirectToAction("Manage");
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

    [HttpGet]
    public IActionResult Delete(int id)
    {
        // Get the specific product
        var product = _context.Products.FirstOrDefault(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int productId)
    {
        var product = _context.Products.Find(productId);

        if (product != null)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction("Manage");
        }
        
        return NotFound();
    }

    [HttpGet]
    public IActionResult LowStock()
    {
        var products = _context.Products.Where(p => p.Quantity <= p.LowStockThreshold).OrderBy(p => p.Quantity).ToList();
        
        return View(products);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var product = _context.Products.FirstOrDefault(p => p.ProductId == id);

        if (product == null)
        {
            return NotFound();
        }
        
        // Get all the categories and their IDs for the dropdown menu
        ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
        
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id,
        [Bind("ProductId, ProductName, CategoryId, Price, Quantity, LowStockThreshold")] Product product)
    {
        if (id != product.ProductId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            _context.Update(product);
            _context.SaveChanges();
            
            TempData["Success"] = "Product updated successfully";
            return RedirectToAction("Manage");
        }
        
        // Get all the categories and their IDs for the dropdown menu
        ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
        return View(product);
    }

    [HttpGet]
    public IActionResult View(int id)
    {
        var product = _context.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductId == id);
        if (product == null)
        {
            return NotFound();
        }
        
        return View(product);
    }
    
}