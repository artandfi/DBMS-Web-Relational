using DBMS_Web_Relational.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static DBMS_Web_Relational.Models.DatabaseManager;

namespace DBMS_Web_Relational.Controllers {
    public class ColumnsController : Controller {
        private const string _errorColumnNameDuplicate = "Стовпчик з такою назвою вже існує";
        private readonly string[] _columnTypes = { "INT", "REAL", "CHAR", "STRING", "TEXT FILE", "INT INTERVAL" };
        private readonly DatabaseManager _dbManager = DatabaseManager.Instance;

        public IActionResult Create(int tableId) {
            FillViewBag(tableId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int tableId, ColumnViewModel model) {
            var columnId = _dbManager.Database.Tables[tableId].Columns.Count;
            
            if (!_dbManager.AddColumn(tableId, ColumnFromString(columnId, model.Name, model.Type))) {
                ModelState.AddModelError("Name", _errorColumnNameDuplicate);
                FillViewBag(tableId);
                return View(model);
            }

            return RedirectToAction("Index", "Tables", tableId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int tableId, int columnId) {
            _dbManager.DeleteColumn(tableId, columnId);
            return RedirectToAction("Index", "Tables", tableId);
        }

        private void FillViewBag(int tableId) {
            ViewBag.TableId = tableId;
            ViewBag.ColumnTypes = new SelectList(_columnTypes.ToList());
        }
    }
}
