using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BenefitsAllocationUpload.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Benefits Allocation Upload Project";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Benefits Allocation Upload Project";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Computing Resources Unit";

            return View();
        }
    }
}
