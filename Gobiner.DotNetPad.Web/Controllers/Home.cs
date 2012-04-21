using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Gobiner.CSharpPad.Web.Models;
using SubSonic.Repository;
using Gobiner.DotNetPad.Web;

namespace Gobiner.CSharpPad.Web.Controllers
{
	[ValidateInput(false)]
	public class HomeController : Controller
	{
		SimpleRepository dataSource;

		public HomeController()
		{
			dataSource = new SimpleRepository("SqlLite", SimpleRepositoryOptions.RunMigrations);
			ValidateRequest = false;
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult Index()
		{
			return View();
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult EditPaste(string id)
		{
			return ViewPaste(id);
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult Gist(string id)
		{
			try
			{
				//extract gist content and language
				JavaScriptSerializer dcjs = new JavaScriptSerializer();
				Dictionary<string, object> gist;

				using (var wc = new WebClient())
					gist = dcjs.DeserializeObject(wc.DownloadString(string.Format("https://api.github.com/gists/{0}", id))) as Dictionary<string, object>;

				var files = gist["files"] as Dictionary<string, object>;
				var pastefile = files.First().Value as Dictionary<string, object>;

				//create, compile, and save new paste
				var paste = new Paste()
					{
						Code = pastefile["content"] as string,
						Language = LanguageExtension.LanguageFromString(pastefile["language"] as string)
					};

				paste.Compile(Request.Cookies["paster"] != null ? Request.Cookies["paster"].Value : string.Empty, false, Server.MapPath("~/App_Data/"));
				Response.Cookies.Add(new HttpCookie("paster", paste.Paster.ToString()) { Expires = DateTime.Today.AddYears(1) });

				dataSource.Add(paste);
				dataSource.AddMany(paste.Errors);
				dataSource.AddMany(paste.ILDisassemblyText);

				return View("ViewPaste", paste);
			}
			catch (Exception e)
			{
				Elmah.ErrorSignal.FromCurrentContext().Raise(e);
				return View("PasteBroked");
			}
		}

		[ValidateInput(false)]
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult Skybot([Bind] Paste paste)
		{
			//these pastes are not saved, on purpose
			paste.Compile(string.Empty, false, Server.MapPath("~/App_Data/"));
			paste.Output = paste.Output.Substring(0, Math.Min(paste.Output.Length, 500));
			return Json(paste);
		}

		[ValidateInput(false)]
		[AcceptVerbs(HttpVerbs.Post)]
		public ActionResult Submit([Bind] Paste paste)
		{
			try
			{
				paste.Compile(Request.Cookies["paster"] != null ? Request.Cookies["paster"].Value : string.Empty, Request.Form["IsPrivate"] == "on", Server.MapPath("~/App_Data/"));
				Response.Cookies.Add(new HttpCookie("paster", paste.Paster.ToString()) { Expires = DateTime.Today.AddYears(1) });

				dataSource.Add(paste);
				dataSource.AddMany(paste.Errors);
				dataSource.AddMany(paste.ILDisassemblyText);

                if (paste.Errors.Length > 0)
                {
                    return SpamGuard.ContentScore(paste.Language, paste.Code) < 0 ? Redirect("/OhTheHumanity/" + paste.Slug) : Redirect("/ViewPaste/" + paste.Slug + "#" + paste.Errors.Select(x => x.Line - 1).Distinct().Select(x => "c" + x + ",").Aggregate((x, y) => x + y));
                }
                else
                {
                    return Redirect("/ViewPaste/" + paste.Slug);
                }
			}
			catch (Exception e)
			{
				Elmah.ErrorSignal.FromCurrentContext().Raise(e);
				return View("PasteBroked");
			}
		}



		[AcceptVerbs(HttpVerbs.Get)]
		public ActionResult ViewPaste(string id)
		{
			var paste = dataSource.Single<Paste>(x => x.Slug == id);
			if (paste == null)
			{
				Response.StatusCode = 404;
				return View("PasteNotFound");
			}

			paste.Errors = dataSource.Find<CompilationError>(x => x.PasteID == paste.ID).ToArray();
			paste.ILDisassemblyText = dataSource.Find<ILDisassembly>(x => x.PasteID == paste.ID).ToArray();

			//todo: some kind of isCompiled flag?
			//todo: should we not also be saving the errors from this compilation too?
			if (paste.ILDisassemblyText.Length == 0)
			{
				paste.Compile(paste.Paster.ToString(), paste.IsPrivate, Server.MapPath("~/App_Data/"));
				dataSource.AddMany(paste.ILDisassemblyText);
			}

			return View(paste);
		}


        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult OhTheHumanity(string id)
        {
            var paste = dataSource.Single<Paste>(x => x.Slug == id);
            if (paste == null)
            {
                Response.StatusCode = 404;
                return View("PasteNotFound");
            }

            return View(paste);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult OhTheHumanity()
        {
            return Redirect("/ViewPaste/" + Request.Form["slug"]);
        }

		public ActionResult About()
		{
			return View();
		}

		public ActionResult Recent()
		{
			var pastes = dataSource.All<Paste>()
				.Where(x => !x.IsPrivate && x.ContentScore >= 0)
				.OrderByDescending(x => x.Created)
				.Take(12);

			return View("List", pastes);
		}

		public ActionResult Mine()
		{
			Guid paster;
			if (Guid.TryParse(Request.Cookies["paster"] != null ? Request.Cookies["paster"].Value : string.Empty, out paster))
			{
				var pastes = dataSource.All<Paste>()
					.Where(x => x.Paster == paster)
					.OrderByDescending(x => x.Created)
					.Take(20);

				return View("List", pastes);
			}
			else
			{
				return View();
			}
		}

		public ActionResult FailBuzz()
		{
			var failures = dataSource.All<Paste>()
				.Where(x => x.Output.ToUpper().Contains("FIZZ")
							&& x.Output.ToUpper().Contains("BUZZ")
							&& x.Output.ToUpper() != Paste.CorrectFizzBuzzOutput.ToUpper())
				.OrderByDescending(x => x.Created)
				.Take(12);

			return View("List", failures);
		}

		public ActionResult FizzBuzz()
		{
			var successes = dataSource.All<Paste>()
				.Where(x => !x.IsPrivate
							&& x.Output.ToUpper() == Paste.CorrectFizzBuzzOutput.ToUpper())
				.OrderByDescending(x => x.Created)
				.Take(12);

			return View("List", successes);
		}
	}
}
