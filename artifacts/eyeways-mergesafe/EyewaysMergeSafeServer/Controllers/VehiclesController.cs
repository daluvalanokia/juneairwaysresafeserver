using EyewaysMergeSafeServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace EyewaysMergeSafeServer.Controllers;

public class VehiclesController : Controller
{
    public IActionResult Index()
    {
        return View(VehicleRegistry.All);
    }
}
