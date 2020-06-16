using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationSystem.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;

namespace ReservationSystem.Controllers
{
    public class InformationController : Controller
    {
        private readonly DbConnectionClass _db;
        private readonly IWebHostEnvironment _env;
        private static readonly Random random = new Random();
        public InformationController(DbConnectionClass db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }
        public IActionResult Index()
        {
            // check login
            if (HttpContext.Session.GetInt32("UserType") == null)
            {
                return RedirectToAction("Login", "User");
            }
            else
            {
                if (HttpContext.Session.GetInt32("UserType") == 1)
                {
                    var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
                    var culture = langRequest.RequestCulture.Culture;
                    ViewBag.Lang = culture.ToString();

                    return View();
                }
            }
            return NotFound();
        }

        /// <summary>
        /// Returns a list of infomation
        /// </summary>
        /// <returns></returns>
        public IActionResult GetListInfo()
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var fileName = (_lang == "en-US") ? "[View the registered file]" : "[登録済みのファイルを見る]";
            var fileName_Error = (_lang == "en-US") ? "There is No registered file." : "登録はありません";
            string format_Day = (_lang == "en-US") ? "MM/dd/yyyy" : "yyyy/MM/dd";

            var lists_info = (from info in _db.Infomations
                              orderby info.InfomationDate descending
                              select new
                              {
                                  infomationId = info.InfomationId,
                                  infomationDate = (info.InfomationDate).ToString(format_Day),
                                  title = info.Title,
                                  note = info.Note,
                                  infomationFile = !string.IsNullOrEmpty(info.InfomationFile) ? info.InfomationFile : "",
                                  fileName = !string.IsNullOrEmpty(info.InfomationFile) ? fileName : fileName_Error,
                                  userId = info.UserId
                              }).ToList();
            return Json(lists_info);
        }

        /// <summary>
        /// Deleting infomation by infomationId
        /// </summary>
        /// <param name="infomationId"></param>
        /// <returns></returns>
        public IActionResult DeleteInfoById(int id) //id = infomationId
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            var info = _db.Infomations.Find(id);
            if (info == null)
            {
                var result = new
                {
                    status = 400,
                    message = (_lang == "en-US") ? "Specified Infromation does not exist." : "指定されたお知らせは存在しません。"
                };
                return Json(result);
            }
            else
            {
                _db.Infomations.Remove(info);
                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = (_lang == "en-US") ? "Information was deleted." : "お知らせを削除しました。"
                };
                return Json(result);
            }
        }

        /// <summary>
        /// Update or Delete infomation
        /// </summary>
        /// <param name="para">Record to be updated</param>
        /// <returns></returns>
        public IActionResult CreateUpdateInfo(ParamaterInfo para)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();

            string filename = string.Empty;

            // Check file
            if (Request.Form.Files.Count() > 0)
            {
                IFormFile file = Request.Form.Files[0];

                string extention = Path.GetExtension(file.FileName);
                string filenamewithoutextension = RandomFilename();

                //filename = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                filename = filenamewithoutextension + extention;

                filename = EnsureCorrectFilename(filename);

                FileStream fs = System.IO.File.Create(GetPathAndFilename(filename));
                file.CopyTo(fs);
                fs.Close();
            }

            if (para.InfomationId != 0) // update
            {

                var getInfoById = _db.Infomations.Find(para.InfomationId);

                if (getInfoById == null)
                {
                    var result = new
                    {
                        status = 400,
                        message = (_lang == "en-US") ? "Specified Infromation does not exist" : "指定されたお知らせは存在しません。"
                    };
                    return Json(result);
                }
                else
                {
                    // Check delete file
                    if (para.StatusRemoveFile != 0)
                    {
                        string _fileToBeDeleted = _env.WebRootPath + "\\files\\" + para.InfomationFile_Old.Split("/")[1];
                        // Check if file exists with its full path    
                        if (System.IO.File.Exists(_fileToBeDeleted))
                        {
                            // If file found, delete it    
                            System.IO.File.Delete(_fileToBeDeleted);

                            //Update name infomation file
                            getInfoById.InfomationFile = !string.IsNullOrEmpty(filename) ? "files/" + filename : "";
                        }
                    }
                    else
                    {
                        getInfoById.InfomationFile = string.IsNullOrEmpty(filename) ? getInfoById.InfomationFile : "files/" + filename;
                    }

                    getInfoById.UserId = para.UserId;
                    getInfoById.InfomationDate = para.InfomationDate;
                    getInfoById.Title = para.Title;
                    getInfoById.Note = para.Note;

                    _db.Infomations.Update(getInfoById);
                    _db.SaveChanges();

                    var result = new
                    {
                        status = 200,
                        message = (_lang == "en-US") ? "Information was updated." : "お知らせ情報を更新しました。",
                    };
                    return Json(result);
                }
            }
            else // insert
            {

                var info = new Infomation()
                {
                    UserId = para.UserId,
                    InfomationDate = para.InfomationDate,
                    Title = para.Title,
                    Note = para.Note,
                    InfomationFile = string.IsNullOrEmpty(filename) ? "" : "files/" + filename
                };

                _db.Infomations.Add(info);
                _db.SaveChanges();
                var result = new
                {
                    status = 200,
                    message = (_lang == "en-US") ? "Information was registed." : "お知らせ情報を登録しました。"
                };
                return Json(result);
            }
        }

        /// <summary>
        /// Ensure Correct Filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string EnsureCorrectFilename(string filename)
        {
            if (filename.Contains("\\"))
                filename = filename.Substring(filename.LastIndexOf("\\") + 1);

            return filename;
        }

        /// <summary>
        /// Get Path And Filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string GetPathAndFilename(string filename)
        {
            return _env.WebRootPath + "\\files\\" + filename;
        }

        /// <summary>
        /// Returns search results based on the start date and end date
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public ActionResult SearchInfo(string startDate, string endDate)
        {
            var langRequest = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = langRequest.RequestCulture.Culture;
            var _lang = culture.ToString();
            string format_Day = (_lang == "en-US") ? "MM/dd/yyyy" : "yyyy/MM/dd";
            var fileName = (_lang == "en-US") ? "[View the registered file]" : "[登録済みのファイルを見る]";
            var fileName_Error = (_lang == "en-US") ? "There is No registered file." : "登録はありません";

            if (!string.IsNullOrEmpty(startDate) && string.IsNullOrEmpty(endDate))
            {
                var query = (from info in _db.Infomations.Where(i => i.InfomationDate.Date >= Convert.ToDateTime(startDate))
                             orderby info.InfomationDate descending
                             select new
                             {
                                 infomationId = info.InfomationId,
                                 infomationDate = (info.InfomationDate).ToString(format_Day),
                                 title = info.Title,
                                 note = info.Note,
                                 infomationFile = !string.IsNullOrEmpty(info.InfomationFile) ? info.InfomationFile : "",
                                 fileName = !string.IsNullOrEmpty(info.InfomationFile) ? fileName : fileName_Error,
                             }).ToList();

                return Json(query);
            }

            if (string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                var query = (from info in _db.Infomations.Where(i => i.InfomationDate.Date <= Convert.ToDateTime(endDate))
                             orderby info.InfomationDate descending
                             select new
                             {
                                 infomationId = info.InfomationId,
                                 infomationDate = (info.InfomationDate).ToString(format_Day),
                                 title = info.Title,
                                 note = info.Note,
                                 infomationFile = !string.IsNullOrEmpty(info.InfomationFile) ? info.InfomationFile : "",
                                 fileName = !string.IsNullOrEmpty(info.InfomationFile) ? fileName : fileName_Error,
                             }).ToList();
                return Json(query);
            }

            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                var query = (from info in _db.Infomations.Where(i => i.InfomationDate.Date >= Convert.ToDateTime(startDate) && i.InfomationDate.Date <= Convert.ToDateTime(endDate))
                             orderby info.InfomationDate descending
                             select new
                             {
                                 infomationId = info.InfomationId,
                                 infomationDate = (info.InfomationDate).ToString(format_Day),
                                 title = info.Title,
                                 note = info.Note,
                                 infomationFile = !string.IsNullOrEmpty(info.InfomationFile) ? info.InfomationFile : "",
                                 fileName = !string.IsNullOrEmpty(info.InfomationFile) ? fileName : fileName_Error,
                             }).ToList();
                return Json(query);
            }

            var lists_info = (from info in _db.Infomations
                              orderby info.InfomationDate descending
                              select new
                              {
                                  infomationId = info.InfomationId,
                                  infomationDate = (info.InfomationDate).ToString(format_Day),
                                  title = info.Title,
                                  note = info.Note,
                                  infomationFile = !string.IsNullOrEmpty(info.InfomationFile) ? info.InfomationFile : "",
                                  fileName = !string.IsNullOrEmpty(info.InfomationFile) ? fileName : fileName_Error,
                              }).ToList();
            return Json(lists_info);

        }
        
        /// <summary>
        /// Random filename
        /// </summary>
        /// <returns></returns>
        public static string RandomFilename()
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray()) + "-" +
                   new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray()) + "-" +
                   new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray()) + "-" +
                   new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray()) + "-" +
                   new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}