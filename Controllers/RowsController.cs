using DBMS_Web_Relational.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DBMS_Web_Relational.Controllers {
    public class RowsController : Controller {
        private readonly DatabaseManager _dbManager = DatabaseManager.Instance;

        public IActionResult Create(int tableId) {
            ViewBag.TableId = tableId;
            return View(_dbManager.GetTable(tableId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int tableId, List<string> values) {
            _dbManager.AddRow(tableId);

            var table = _dbManager.GetTable(tableId);
            for (int i = 1; i < table.Columns.Count; i++) {
                dynamic column = table.Columns[i];

                if (!_dbManager.ChangeCellValue(values[i-1], tableId, i, table.Rows.Count - 1)) {
                    ModelState.AddModelError("", $"{column.Name}: введіть значення типу {column.type}");
                    ViewBag.TableId = tableId;
                    _dbManager.DeleteRow(tableId, table.Rows.Count - 1);

                    return View(_dbManager.GetTable(tableId));
                }
            }

            return RedirectToAction("Index", "Tables", tableId);
        }

        public IActionResult Edit(int tableId, int rowId) {
            ViewBag.TableId = tableId;
            ViewBag.RowId = rowId;

            return View(_dbManager.GetTable(tableId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int tableId, int rowId, List<string> values) {
            var table = _dbManager.GetTable(tableId);
            
            for (int i = 1; i < table.Columns.Count; i++) {
                dynamic column = table.Columns[i];

                if (!_dbManager.ChangeCellValue(values[i-1], tableId, i, rowId)) {
                    ModelState.AddModelError("", $"{column.Name}: введіть значення типу {column.type}");
                    ViewBag.TableId = tableId;
                    ViewBag.RowId = rowId;

                    return View(_dbManager.GetTable(tableId));
                }
            }

            return RedirectToAction("Index", "Tables", tableId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int tableId, int rowId) {
            _dbManager.DeleteRow(tableId, rowId);
            return RedirectToAction("Index", "Tables", tableId);
        }
    }
}
