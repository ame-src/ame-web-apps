using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AmeWebApps.Controllers
{
    public class CommController : Controller
    {
        //
        // GET: /Comm/

        public ActionResult Index(string ds)
        {
            ViewBag.fileCount = 0;
            ViewBag.folderPath = "NULL";
            ViewBag.hashCount = 0;
            ViewBag.lastActivity = DateTime.Now.AddDays(-5);
            ViewBag.ds = ds;
            ViewBag.lastHashFile = null;
            ViewBag.hasntStarted = false;
           

            if (!String.IsNullOrEmpty(ds))
            {
                var file02Path = String.Format(@"\\AME-FILE-02\AMEUpdates\Disc Images\{0}", ds);
                var folderPath = String.Format(@"\\COMM-1\ame\Updates\Media\{0}", ds);

                if (Directory.Exists(folderPath))
                {

                    var imageFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
                    var hashFiles = Directory.GetFiles(folderPath, "*.hash", SearchOption.AllDirectories);

                    int realCount = imageFiles.Except(hashFiles).Count() - 1;
                    int hashCount = hashFiles.Count();
                    if (hashCount == 0)
                    {
                        ViewBag.hasntStarted = true;
                    }
                    else
                    {

                        var lastHash = new DirectoryInfo(folderPath).GetFiles("*.hash", SearchOption.AllDirectories).OrderByDescending(f => f.CreationTime).FirstOrDefault();

                        ViewBag.lastActivity = lastHash.LastWriteTime;
                        ViewBag.lastHashFile = lastHash;
                        ViewBag.fileCount = realCount;
                        ViewBag.hashCount = hashCount;
                        ViewBag.folderPath = folderPath;
                        ViewBag.ds = ds;
                    }
                }
                else
                {
                    ViewBag.lastActivity = "N/A";
                    ViewBag.fileCount = "N/A";
                    ViewBag.hashCount = "N/A";
                    ViewBag.folderPath = "N/A";
                    ViewBag.ds = "N/A";
                }
            }
            
            return View();
        }

    }
}
