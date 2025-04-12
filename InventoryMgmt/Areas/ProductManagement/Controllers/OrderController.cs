using InventoryMgmt.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace InventoryMgmt.Areas.ProductManagement.Controllers;

[Area("ProductManagement")]
[Route("[area]/[controller]")]
public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<OrderController> _logger;

    public OrderController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<OrderController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    [Authorize(Roles = "Admin, Manager")]
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderId)
                .ToListAsync();

            return View(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in Order Index action");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }

    [Authorize]
    [HttpGet("MyOrders")]
    public async Task<IActionResult> MyOrders()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserEmail == user.Email)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in MyOrders action");
            return RedirectToAction("ServerError", "Error", new { area = "" });
        }
    }
}
