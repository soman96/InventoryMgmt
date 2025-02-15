using InventoryMgmt.Data;
using Microsoft.AspNetCore.Mvc;

namespace InventoryMgmt.Controllers;

public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var orders = _context.Orders.ToList();
        return View(orders);
    }
}