using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using SubSonic.Repository;
using Gobiner.CSharpPad.Web.Models;

namespace Gobiner.CSharpPad.Web.Controllers
{
    public class HomeController : Controller
    {
		SimpleRepository dataSource;

		public HomeController()
		{
			dataSource = new SimpleRepository("SqlLite", SimpleRepositoryOptions.RunMigrations);
		}

		[AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {
            return View();
        }

		public ActionResult About()
		{
			return View();
		}

		[ValidateInput(false)]
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult Submit([Bind] Paste paste)
		{
			var evaller = new Eval(Server.MapPath("~/App_Data/"));
			evaller.CompileAndEval(paste.Code);
			paste.AddCompilerErrors(evaller.Errors);
			paste.Output = string.Join(Environment.NewLine, evaller.Output ?? new string[] {});

			dataSource.Add(paste);
			dataSource.AddMany(paste.Errors);

			return Redirect("/Paste/" + paste.Slug);
		}

		public ActionResult Paste(string id)
		{
			var paste = dataSource.Single<Paste>(x => x.Slug == id);
			if (paste == null)
				return View("PasteNotFound");

			paste.Errors = dataSource.Find<CompilationError>(x => x.PasteID == paste.ID).ToArray();

			return View(paste);
		}

		public ActionResult Recent()
		{
			return View();
		}
    }
}
