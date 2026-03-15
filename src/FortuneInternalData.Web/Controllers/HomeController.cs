using Microsoft.AspNetCore.Mvc;

namespace FortuneInternalData.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
