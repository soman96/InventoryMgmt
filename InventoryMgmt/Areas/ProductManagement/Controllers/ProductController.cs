using InventoryMgmt.Data;
using InventoryMgmt.Models;
using InventoryMgmt.Areas.ProductManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace InventoryMgmt.Areas.ProductManagement.Controllers;

[Area("ProductManagement")]
[Route("[area]/[controller]")]
public class ProductController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductController> _logger;

    public ProductController(ApplicationDbContext context, ILogger<ProductController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string searchQuery, int? categoryId, string sortBy)
    {
        try
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                string lowerSearchQuery = searchQuery.ToLower();
                products = products.Where(p => p.ProductName.ToLower().Contains(lowerSearchQuery));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            products = sortBy switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "name_asc" => products.OrderBy(p => p.ProductName),
                "name_desc" => products.OrderByDescending(p => p.ProductName),
                _ => products.OrderBy(p => p.ProductName),
            };

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            ViewBag.SortBy = sortBy;
            ViewBag.SearchQuery = searchQuery;
            ViewBag.SelectedCategory = categoryId;

            return View(await products.ToListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in Index action");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [HttpGet("FilterProducts")]
    public async Task<IActionResult> FilterProducts(string searchQuery, int? categoryId, string sortBy)
    {
        try
        {
            var products = _context.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                string lowerSearchQuery = searchQuery.ToLower();
                products = products.Where(p => p.ProductName.ToLower().Contains(lowerSearchQuery));
            }

            if (categoryId.HasValue && categoryId > 0)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            products = sortBy switch
            {
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                "name_asc" => products.OrderBy(p => p.ProductName),
                "name_desc" => products.OrderByDescending(p => p.ProductName),
                _ => products.OrderBy(p => p.ProductName),
            };

            return PartialView("_ProductList", await products.ToListAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in FilterProducts action");
            return StatusCode(500);
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("Manage")]
    public async Task<IActionResult> Manage()
    {
        try
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in Manage action");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("Add")]
    public async Task<IActionResult> Add()
    {
        ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
        return View();
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("Add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Product product)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View(product);
        }

        try
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product added successfully";
            return RedirectToAction("Manage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding product");
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View(product);
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            return product == null ? NotFound() : View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in Delete GET action");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("Delete/{productId:int}"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int productId)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully!";
                return RedirectToAction("Manage");
            }
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in DeleteConfirmed action");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("LowStock")]
    public async Task<IActionResult> LowStock()
    {
        try
        {
            var products = await _context.Products
                .Where(p => p.Quantity <= p.LowStockThreshold)
                .OrderBy(p => p.Quantity)
                .ToListAsync();

            return View(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in LowStock action");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in Edit GET action");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ProductId, ProductName, CategoryId, Price, Quantity, LowStockThreshold")] Product product)
    {
        if (id != product.ProductId) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View(product);
        }

        try
        {
            _context.Update(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Product updated successfully";
            return RedirectToAction("Manage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in Edit POST action");
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "Name");
            return View(product);
        }
    }

    [HttpGet("View/{id:int}")]
    public async Task<IActionResult> View(int id)
    {
        try
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);
            return product == null ? NotFound() : View(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in View action");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }
}
