using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace OTS.Controllers
{
  public class HomeController : Controller
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Error()
    {
      ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
      return View();
    }


    // GET api/values
    [Route("api/[controller]"), Authorize, HttpGet]
    public IEnumerable<string> Get()
    {
      return new[] {"value1", "value2"};
    }
  }
}
