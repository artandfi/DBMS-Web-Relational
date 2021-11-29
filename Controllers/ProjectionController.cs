using DBMS_Web_Relational.Models;
using Microsoft.AspNetCore.Mvc;

namespace DBMS_Web_Relational.Controllers {
    public class ProjectionController : Controller {
        private readonly DatabaseManager _dbManager = DatabaseManager.Instance;

        public IActionResult Index(int tableId, int[] columnIds) => View(_dbManager.Project(tableId, columnIds));
    }
}
