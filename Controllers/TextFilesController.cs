using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMS_Web_Relational.Controllers {
    public class TextFilesController : Controller {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Save(string fileContents) {
            int asteriskIndex = fileContents.IndexOf('*');
            string fileName = fileContents.Substring(0, asteriskIndex);
            fileContents = fileContents.Substring(asteriskIndex + 1, fileContents.Length - asteriskIndex - 1);

            return File(Encoding.ASCII.GetBytes(fileContents), "text/plain", fileName);
        }
    }
}
