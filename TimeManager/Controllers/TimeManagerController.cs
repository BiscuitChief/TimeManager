using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TimeManager.Controllers
{
    public class TimeManagerController : Controller
    {
        //
        // GET: /TimeManager/
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult PrintTasks()
        {
            return View();
        }

    }
}
