using InventoryMgmt.Models;
using Microsoft.AspNetCore.Mvc;

namespace InventoryMgmt.Controllers;

public class ProductController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var products = new List<Product>();

        return View(products);
    }
}