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
			paste.IsPrivate = Request.Form["IsPrivate"] == "on"; // aspnetmvc doesn't bind 'on' to True apparently
			try
			{
				paste.Paster = new Guid(Request.Cookies["paster"].Value);
			}
			catch // no Guid.TryParse()  :(
			{
				paste.Paster = Guid.NewGuid();
				Response.Cookies.Add(new HttpCookie("paster", paste.Paster.ToString()));
			}

			dataSource.Add(paste);
			dataSource.AddMany(paste.Errors);

			return Redirect("/ViewPaste/" + paste.Slug);
		}

        public ActionResult EditPaste(string id)
        {
            return ViewPaste(id);
        }

		public ActionResult ViewPaste(string id)
		{
			var paste = dataSource.Single<Paste>(x => x.Slug == id);
			if (paste == null)
				return View("PasteNotFound");

			paste.Errors = dataSource.Find<CompilationError>(x => x.PasteID == paste.ID).ToArray();

			return View(paste);
		}

		public ActionResult Recent()
		{
			var pastes = dataSource.All<Paste>().Where(x => !x.IsPrivate).OrderByDescending(x => x.Created).Take(12);
			return View("List",pastes);
		}

		public ActionResult Mine()
		{
			try
			{
				var myGuid = new Guid(Request.Cookies["paster"].Value);
				var pastes = dataSource.All<Paste>().Where(x => x.Paster == myGuid).OrderByDescending(x => x.Created).Take(20);
				return View("List", pastes);
			}
			catch (NullReferenceException e)
			{
				return View();
			}
		}

		public ActionResult FailBuzz()
		{
			var pastes = dataSource.All<Paste>().Where(x => x.Output.ToUpper().Contains("FIZZ")
														&& x.Output.ToUpper().Contains("BUZZ")
														&& x.Output.ToUpper() != correctFizzBuzzOutput.ToUpper())
																.OrderByDescending(x => x.Created).Take(12);
			return View("List", pastes);
		}

		public ActionResult FizzBuzz()
		{
			var pastes = dataSource.All<Paste>().Where(x => !x.IsPrivate && x.Output.ToUpper() == correctFizzBuzzOutput.ToUpper()).OrderByDescending(x => x.Created).Take(12);
			return View("List", pastes);
		}

		private string correctFizzBuzzOutput
		{
			get
			{
				return @"1
2
Fizz
4
Buzz
Fizz
7
8
Fizz
Buzz
11
Fizz
13
14
FizzBuzz
16
17
Fizz
19
Buzz
Fizz
22
23
Fizz
Buzz
26
Fizz
28
29
FizzBuzz
31
32
Fizz
34
Buzz
Fizz
37
38
Fizz
Buzz
41
Fizz
43
44
FizzBuzz
46
47
Fizz
49
Buzz
Fizz
52
53
Fizz
Buzz
56
Fizz
58
59
FizzBuzz
61
62
Fizz
64
Buzz
Fizz
67
68
Fizz
Buzz
71
Fizz
73
74
FizzBuzz
76
77
Fizz
79
Buzz
Fizz
82
83
Fizz
Buzz
86
Fizz
88
89
FizzBuzz
91
92
Fizz
94
Buzz
Fizz
97
98
Fizz
Buzz";
			}
		}
    }
}
