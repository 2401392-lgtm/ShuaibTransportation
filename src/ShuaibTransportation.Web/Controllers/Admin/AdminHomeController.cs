using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ShuaibTransportation.Web.Controllers.Admin;

[Authorize(Roles = "Admin")]
public class AdminHomeController : Controller
{
    public IActionResult Index() => View();
}
