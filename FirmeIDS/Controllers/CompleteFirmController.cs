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
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace FirmeIDS.Controllers
{
    public class CompleteFirmController : Controller
    {
      public ActionResult ViewCompleteFirms()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            List<CompleteFirm> model = new List<CompleteFirm>();

            IQueryable<CompleteFirm> libr = (from a in db.Firms
                                        join b in db.Details on a.CodFirma equals b.CodFirma

                                        select new CompleteFirm {
                                            CodFirma = a.CodFirma,
                                            Nume = a.Nume,
                                            ActivitatePrincipala = a.ActivitatePrincipala,
                                            Administrator = b.Administrator,
                                            Telefon = b.Telefon,
                                            Email = b.Email,
                                            SediuSocial = b.SediuSocial
                                        });

            model = libr.ToList();

            return View(model);
        }
    }
}