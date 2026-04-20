using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProjectApprovalSystem.Models;

namespace ProjectApprovalSystem.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity!.IsAuthenticated)
        {
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Dashboard", "Student");
            }
            else if (User.IsInRole("Supervisor"))
            {
                return RedirectToAction("Dashboard", "Supervisor");
            }
            else if (User.IsInRole("ModuleLeader") || User.IsInRole("SystemAdmin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
