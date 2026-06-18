using AirwaysMergeSafeServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace AirwaysMergeSafeServer.Controllers;

public class VehiclesController : Controller
{
    public IActionResult Index()
    {
        return View(VehicleRegistry.All);
    }
}
