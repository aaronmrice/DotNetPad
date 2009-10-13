using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace Gobiner.CSharpPad.Web.Controllers
{
    public class Display : Controller
    {
        //
        // GET: /Display/

        public ActionResult Display(int id)
        {
            return View();
        }

    }
}
