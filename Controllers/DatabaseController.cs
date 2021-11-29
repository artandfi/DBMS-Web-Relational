using Microsoft.AspNetCore.Mvc;
using DBMS_Web_Relational.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DBMS_Web_Relational.Controllers {
    public class DatabaseController : Controller {
        private const string _errorInvalidCharacters = "Назва БД містить неприпустимі символи";
        private readonly DatabaseManager _dbManager = DatabaseManager.Instance;

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string name) {
            if (!_dbManager.CreateDatabase(name)) {
                ModelState.AddModelError("Name", _errorInvalidCharacters);
                return View(new Database(name));
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete() {
            _dbManager.DeleteDatabase();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Open() {
            ViewBag.DbNames = new SelectList(_dbManager.GetDatabaseNames());
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Open(string name) {
            _dbManager.OpenDatabase(name);
            return RedirectToAction("Index", "Home");
        }
    }
}
