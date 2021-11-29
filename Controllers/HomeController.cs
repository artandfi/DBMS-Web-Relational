using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DBMS_Web_Relational.Models;

namespace DBMS_Web_Relational.Controllers {
    public class HomeController : Controller {
        public readonly DatabaseManager _dbManager = DatabaseManager.Instance;

        public IActionResult Index() {
            ViewBag.DbManager = _dbManager;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
