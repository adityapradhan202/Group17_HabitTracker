using Microsoft.AspNetCore.Mvc;

namespace HabitTrackerAspNetMVCWebApp.Controllers;

public class PagesController : Controller
{
    public IActionResult Calendar()
    {
        return View();
    }

    public IActionResult Kanban()
    {
        return View();
    }
}
