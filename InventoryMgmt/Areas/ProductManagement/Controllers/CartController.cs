using System.Text.Json;
using InventoryMgmt.Data;
using InventoryMgmt.Models;
using InventoryMgmt.Areas.ProductManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace InventoryMgmt.Areas.ProductManagement.Controllers;

[Authorize]
[Area("ProductManagement")]
[Route("[area]/[controller]")]
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<CartController> _logger;

    public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<CartController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        try
        {
            var cart = GetCart();
            return View(cart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cart");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [HttpGet("AddToCart/{productId:int}")]
    public async Task<IActionResult> AddToCart(int productId)
    {
        try
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
            {
                cart.Add(new Cart
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Quantity = 1
                });
            }
            else
            {
                item.Quantity++;
            }

            SaveCart(cart);
            return Json(new { message = $"{product.ProductName} added to cart." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product to cart");
            return StatusCode(500);
        }
    }

    [HttpGet("RemoveFromCart/{productId:int}")]
    public IActionResult RemoveFromCart(int productId)
    {
        try
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.ProductId == productId);
            SaveCart(cart);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing item from cart");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [HttpPost("PlaceOrder")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(string Name)
    {
        try
        {
            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction("Index");

            string? userEmail = null;
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                userEmail = user?.Email;
            }

            var order = new Order
            {
                CustomerName = Name,
                OrderDate = DateTime.UtcNow,
                Total = cart.Sum(i => i.Price * i.Quantity),
                OrderItems = cart.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList(),
                UserEmail = userEmail
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity -= item.Quantity;
                    _context.Products.Update(product);
                }
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("Cart");

            TempData["success"] = $"Your order (#{order.OrderId}) has been placed!";
            return RedirectToAction("MyOrders", "Order");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing order");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    private List<Cart> GetCart()
    {
        var cartJson = HttpContext.Session.GetString("Cart");
        return cartJson != null ? JsonSerializer.Deserialize<List<Cart>>(cartJson) ?? new List<Cart>() : new List<Cart>();
    }

    private void SaveCart(List<Cart> cart)
    {
        HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
    }
}
