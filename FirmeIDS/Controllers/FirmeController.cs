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
    public class FirmeController : Controller
    {

        public ActionResult Register(Firma model) // register a firm in the db to check if it works 
        {
            ApplicationDbContext db = new ApplicationDbContext();
           
            db.Firms.Add(model);

            db.SaveChanges();

            return RedirectToAction("ViewFirms");
        }



        public ActionResult ViewFirms() // view all the firms
        {
            ApplicationDbContext db = new ApplicationDbContext();
            List<Firma> model = db.Firms.ToList();
            return View(model);
        }

        public ActionResult ResolveName()
        {
            ApplicationDbContext db = new ApplicationDbContext();



            List<Firma> list = db.Firms.ToList();

            string[] names;

            foreach(var i in list)
            {
                string nume = i.Nume;
                if(i.Nume.Contains("span"))
                {
                    names = nume.Split('>');

                    StringBuilder sb = new StringBuilder();

                    sb.Append(names[0]);

                    sb.Replace("<span title=", "");
                    sb.Replace("'", "");

                    nume = sb.ToString();

                    i.Nume = nume;
                }
                db.SaveChanges();
            }

            return RedirectToAction("ViewFirms");
        }

        public ActionResult HotFix()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            var list = db.Firms.ToList();

            foreach (var i in list )
            {
                i.Flag = 0;
            }

            db.SaveChanges();

            return RedirectToAction("ViewFirms");
        }

        public ActionResult ViewSpanFirms()
        {
            ApplicationDbContext db = new ApplicationDbContext();

            List<FirmaDTO> model = (from a in db.Firms
                                       where a.Nume.Contains("span")
                                       select new FirmaDTO()
                                       {
                                           Nume = a.Nume,
                                           CodFirma = a.CodFirma,
                                           Flag = a.Flag
                                       }).ToList();

            string[] names;

            foreach(var i in model)
            {
                string nume = i.Nume;

                names = nume.Split('>');

                StringBuilder sb = new StringBuilder();

                sb.Append(names[0]);

                sb.Replace("<span title=", "");
                sb.Replace("'", "");

                nume = sb.ToString();

                i.Nume = nume;
            }

            return View(model);
        }

        public ActionResult GetAllFirms() // the scraping part 
        {
            ApplicationDbContext db = new ApplicationDbContext();
            Random rand = new Random();

            int numberOfPages = 527; //  the total number of pages to the scraper
            int numberOfFirms = 15766; // the total number of firms 

            string SesId = LogIn(); // get the session ID   

            var Pages = db.Pages.Where(x => x.Flag == 0).ToList(); // check if there are pages left to scrap from 
            if (Pages.Count > 0) // if there are any pages left to scrap 
            {

                if (Pages.Count >= 50) // check if there are 50 pages left to scrap so that the program will not throw and exception 
                {
                    for (int i = 0; i < 50; i++)
                    {
                        var PoolPages = db.Pages.Where(x => x.Flag == 0).ToList(); // select the unscraped pages 

                        int random = rand.Next(0, PoolPages.Count); // select one random page from the unscraped pages 

                        List<Firma> results = ScrapFirms(SesId, PoolPages[random].PageNumber); // scrap the firms from the page. It must send the PageNumber, not the possition of the firm in the list 

                        foreach (var fir in results)
                        {
                            db.Firms.Add(fir);
                            db.SaveChanges();
                        }

                        PoolPages[random].Flag = 1; // make sure that the flag is changed to 1 when the page was scraped 

                        db.SaveChanges(); // save the changes

                        Thread.Sleep(30 * 1000); // delay the requests by 30 seconds 
                    }
                }
                else
                {
                    for (int i = 0; i < Pages.Count; i++) // if there aren't 50 pages left the program will scrap all the remaining pages
                    {
                        var PoolPages = db.Pages.Where(x => x.Flag == 0).ToList();

                        int random = rand.Next(0, PoolPages.Count); // select a random page to scrap the results from 

                        List<Firma> results = ScrapFirms(SesId, PoolPages[random].PageNumber);

                        foreach (var fir in results)
                        {
                            db.Firms.Add(fir); // add the firms to the DB 
                            db.SaveChanges(); // save the changes 
                        }

                        PoolPages[random].Flag = 1; // make the flag 1 

                        db.SaveChanges(); // save the changes 

                        Thread.Sleep(30 * 1000); // delay the requests by 30 seconds 
                    }
                }
                db.SaveChanges();

                return RedirectToAction("ViewFirms");
            }
            else
            {
                return RedirectToAction("Message");
            }
            
        }

        public ActionResult Message()
        {
            return View();
        }

        //Scraping
        public List<Firma> ScrapFirms(string s, int a)
        {
            List<Firma> list = new List<Firma>();
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            string url;

            // THESE ARE THE URLS FOR CLUJ-NAPOCA FIRMS !!!!!!!!! THEY MUST BE CHANGED IN ORDER TO WORK FOR ALL THE FIRMS !!!!!!!!!!

            if (a == 1)
                url = "https://www.bursatransport.com/memberlist?doSearch=1&privateexchange_id=&MemberSearch2%5Btext%5D=&MemberSearch2%5Bdescription%5D=&source_id_txt=&source_id=RO&MemberSearch2%5Bactivity%5D=&MemberSearch2%5BactiveTransportLicence%5D=0&MemberSearch2%5BactiveForwardingLicence%5D=0&MemberSearch2%5BactiveCompanies%5D=0&searchType=1&yt0=";
            else
                url = "https://www.bursatransport.com/memberlist?doSearch=1&privateexchange_id=&MemberSearch2%5Btext%5D=&MemberSearch2%5Bdescription%5D=&MemberSearch2%5Bactivity%5D=&MemberSearch2%5BactiveTransportLicence%5D=0&MemberSearch2%5BactiveForwardingLicence%5D=0&MemberSearch2%5BactiveCompanies%5D=0&source_id_txt=&source_id=RO&searchType=1&yt0=&range=0&page=" + a;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url); // create the request 

            CookieContainer cookies = new CookieContainer();

            Uri target = new Uri("https://www.bursatransport.com/");

            cookies.Add(new Cookie("PHPSESSID", s) { Domain = target.Host });

            request.CookieContainer = cookies;

            string responseContent = null;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            using (Stream responseStream = response.GetResponseStream())

            using (StreamReader responseReader = new StreamReader(responseStream))
            {
                responseContent = responseReader.ReadToEnd();
            }
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();

            doc.LoadHtml(responseContent);

            var Codes = doc.DocumentNode.SelectNodes("//*[@id=\"main_content\"]/div[4]/table//tr/td[1]");
            var Names = doc.DocumentNode.SelectNodes("//*[@id=\"main_content\"]/div[4]/table//tr/td[2]/a");
            var Inscriere = doc.DocumentNode.SelectNodes("//*[@id=\"main_content\"]/div[4]/table//tr/td[3]");
            var Infintare = doc.DocumentNode.SelectNodes("//*[@id=\"main_content\"]/div[4]/table//tr/td[4]");
            var Activitate = doc.DocumentNode.SelectNodes("//*[@id=\"main_content\"]/div[4]/table//tr/td[5]");

            string[] names;

            for (int i = 0; i < Codes.Count; i++)
            {
                string cod = Codes[i].InnerHtml;
                string nume = Names[i].InnerHtml;
                string inscriere = Inscriere[i].InnerHtml;
                string infintare = Infintare[i].InnerHtml;
                string activitate = Activitate[i].InnerHtml;

               
                if(nume.Contains("span"))
                {
                    names = nume.Split('>');

                    StringBuilder sb = new StringBuilder();

                    sb.Append(names[0]);

                    sb.Replace("<span title=", "");
                    sb.Replace("'", "");

                    nume = sb.ToString();
                }


                list.Add(new Firma() { CodFirma = cod, Nume = nume, DataInscrierii = inscriere, AnulInfintarii = infintare, ActivitatePrincipala = activitate, Flag = 0 });
            }

            return list;
        }

        //Login
        public string LogIn()
        {

            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // every request you make with HTTPS doesn't work without this

            CookieContainer cookieJar = new CookieContainer();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.bursatransport.com/login");
            request.CookieContainer = cookieJar;

            request.Method = "POST"; // we will post the data using post method

            request.UseDefaultCredentials = false;

            request.ContentType = "application/x-www-form-urlencoded";

            request.UserAgent = "Mozilla/5.0(Windows NT 10.0;Win64;x64) AppleWebKit/537.36(KHTML,like Gecko)Chrome/60.0.3112.113Safari/537.36";

            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";

            request.AllowAutoRedirect = false; // this is needed in order for the login to work

            //Change the credentials to match the given Account
            var eco = Encoding.ASCII.GetBytes("Login[username]=RAVCAR1&Login[password]=ROBERTADINA"); // encode the credentials with the form fields

            var requestStream = request.GetRequestStream(); //Get a request stream
            requestStream.Write(eco, 0, eco.Length); // Post the data 
            requestStream.Close(); //Close the request stream

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();  // Get the response.
            response.Close();   // Close the response
            CookieCollection cook = response.Cookies; // Get the Session ID 

            Cookie cookieID = cook[0];

            string sisID = cookieID.Value;

            return sisID;
        }
    }
}