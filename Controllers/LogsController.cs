using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ReservationSystem.Controllers
{
    public class LogsController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public LogsController(IWebHostEnvironment env)
        {
            _env = env;
        }
        public IActionResult ListImportLogs()
        {
            //check login
            if (HttpContext.Session.GetInt32("UserType") == null)
            {
                return RedirectToAction("Login", "User");
            }
            string[] filePaths = Directory.GetFiles(_env.WebRootPath + "\\logs", "*.txt");

            List<string> ListFiles = new List<string>();
            // return Json(filePaths);

            foreach (var dir in filePaths)
            {
                string[] items = dir.Split("\\");
                ListFiles.Add(items[items.Length - 1]);
            }
            ViewBag.ListFiles = ListFiles;
            //return Json(ListFiles);
            return View();
        }
    }
}
