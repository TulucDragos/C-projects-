using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FirmeIDS.Models;
using System.Text;
using System.Net;
using System.Threading;
using System.Configuration;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using Newtonsoft.Json.Linq;

namespace FirmeIDS.Controllers
{
    public class PagesController : Controller
    {

      public ActionResult ViewPages() //This is just to see all the pages
        {
            ApplicationDbContext db = new ApplicationDbContext();

            var list = db.Pages.ToList();

            return View(list);
        }

        public ActionResult Create() //This will create the pages table. The pages from where the scraping begins 
        {
            ApplicationDbContext db = new ApplicationDbContext();

            for (int i = 1; i<=527; i++) // Check the number of pages from the site. MENDATORY 
            {
                Page page = new Page { PageNumber = i, Flag = 0 };

                db.Pages.Add(page);

                db.SaveChanges();
            }
            return RedirectToAction("ViewPages");
        }
    }
}