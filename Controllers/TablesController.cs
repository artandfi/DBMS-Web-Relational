using DBMS_Web_Relational.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DBMS_Web_Relational.Controllers {
    public class TablesController : Controller {
        private const string _errorDuplicateTableName = "Таблиця з таким іменем вже існує";
        private readonly DatabaseManager _dbManager = DatabaseManager.Instance;

        public IActionResult Index(int id) => View(_dbManager.GetTable(id));

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string name) {
            if (!_dbManager.AddTable(name)) {
                ModelState.AddModelError("Name", _errorDuplicateTableName);
                return View(new Table(0, name));
            }

            return RedirectToAction("Index", _dbManager.Database.Tables.Last().Id);
        }

        public IActionResult Edit(int id) => View(_dbManager.GetTable(id));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, string name) {
            var table = _dbManager.GetTable(id);

            if (!table.Name.Equals(name) && _dbManager.GetTableNames().Contains(name)) {
                ModelState.AddModelError("Name", _errorDuplicateTableName);
                return View(new Table(id, name));
            }

            _dbManager.EditTable(id, name);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id) {
            _dbManager.DeleteTable(id);
            return RedirectToAction("Index", "Home");
        }
    }
}
