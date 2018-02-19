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
    public class DetailController : Controller
    {
         

        public ActionResult NewDetail(Detail model) // create a new detail to see if the db works. 
        {
            ApplicationDbContext db = new ApplicationDbContext();

            db.Details.Add(model);

            db.SaveChanges();

            return RedirectToAction("ViewDetails");

        }

        public ActionResult ViewDetails() // view all the firms with their details 
        {
            ApplicationDbContext db = new ApplicationDbContext();

            var model = db.Details.ToList();

            return View(model);
        }

        public ActionResult HotFix() // hot fix the firms that are empty in the DB (delete the corrupted rows and cahnged the firm's flag to 0, marking it as unscrapped )
        {
            ApplicationDbContext db = new ApplicationDbContext();

            var HotDetails = db.Details.Where(x => x.Administrator == "" && x.CIF == "").ToList(); // select the empty details from the DB 

            List <Firma> HotFirms = new List<Firma>();

            foreach(var detail in HotDetails)
            {
               var i =  db.Firms.Where(x => x.CodFirma == detail.CodFirma).ToList(); // select the Firms that are connected to  the details

                foreach(var firm in i)
                {
                    HotFirms.Add(firm); // add them to the list 
                }
                                 
            }

            foreach(var firm in HotFirms)
             {
                 firm.Flag = 0; // change their state to unscrapped
             }

             foreach(var detail in HotDetails)
             {
                 db.Details.Remove(detail); // remove the corrupted details from the DB 
             }

             db.SaveChanges(); // save the changes in the DB 

            return RedirectToAction("ViewDetails");
        }

        

        public ActionResult GetDetails() // the scraping part beggins here 
        {
            ApplicationDbContext db = new ApplicationDbContext();

            IWebDriver driver = new ChromeDriver(); // instance the chrome driver 

            string username = "RAVCAR1"; // set the username
            string password = "Robert2018"; // set the password

            Random rand = new Random(); // create the random object 

            LogIn(driver, username, password); // make the log in

            var Firms = db.Firms.Where(x => x.Flag == 0).ToList(); // select the firms that are left to be scraped 

            if (Firms.Count > 0) // if there are firms to be scraped beggin the proccess 
            {

                if (Firms.Count > 200) // if there are more than 200 pages left to scrap 
                {
                    for (int i = 0; i < 200; i++)
                    {
                        var FirmsPool = db.Firms.Where(x => x.Flag == 0).ToList(); // select the pool of firms 

                        int random = rand.Next(0, FirmsPool.Count); // make a random selection of a firm 

                        Detail details = ScrapDetails(FirmsPool[random].CodFirma, driver); // scrap the details for that firm 

                        // parse the results 

                        Parse(details); // parse  the result

                        // here ends the parsing proccess 

                        //create the Firm with all the details
                        Detail final = new Detail() // create the detail  ********* see if there is a possibility to make some adjustments to this 
                        {
                            CodFirma = FirmsPool[random].CodFirma,                         
                            CIF = details.CIF,
                            Administrator = details.Administrator,
                            SediuSocial = details.SediuSocial,
                            Adresa = details.Adresa,
                            Email = details.Email,
                            Telefon = details.Telefon
                        };

                        if(final.Administrator != "" || final.Email != "")
                        {
                            db.Details.Add(final); // add the details to the DB 

                            FirmsPool[random].Flag = 1; // mark the firm as scraped 

                            db.SaveChanges(); // save changes 

                        }
                        

                       

                        Thread.Sleep(10 * 1000); // delay the next request by 15 seconds 
                    }
                }
                else
                {
                    int last = Firms.Count;
                    for (int i = 0; i < last; i++)
                    {
                        var FirmsPool = db.Firms.Where(x => x.Flag == 0).ToList();

                        int random = rand.Next(0, FirmsPool.Count);

                        Detail details = ScrapDetails(FirmsPool[random].CodFirma, driver);

                        // parse the results 

                        Parse(details);

                        // here ends the parsing proccess 

                        //create the Firm with all the details
                        Detail final = new Detail()
                        {
                            CodFirma = FirmsPool[i].CodFirma,                          
                            CIF = details.CIF,
                            Administrator = details.Administrator,
                            SediuSocial = details.SediuSocial,
                            Adresa = details.Adresa,
                            Email = details.Email,
                            Telefon = details.Telefon
                        };

                        if(final.Administrator != "" || final.Email != "")
                        {
                            db.Details.Add(final);

                            FirmsPool[random].Flag = 1;

                            db.SaveChanges();
                        }

                        Thread.Sleep(10 * 1000); // delay yhe next request by 10 seconds
                    }
                }
                db.SaveChanges();

                return RedirectToAction("ViewDetails");
            }

            return RedirectToAction("Message");
          
        }

        public ActionResult Message()
        {
            return View();
        }

        public void LogOut(IWebDriver driver)
        {
            string url = "https://www.bursatransport.com";

            driver.Navigate().GoToUrl(url);

            driver.FindElement(By.XPath("//*[@id=\"main-menu -holder\"]/ul/li[8]/a/span")).Click();

            driver.FindElement(By.XPath("//*[@id=\"menu_user_box\"]/div[5]/div[2]/button")).Click();

        }

        public void LogIn(IWebDriver driver, string user, string pw)
        {
            string url = "https://www.bursatransport.com/login";

            driver.Navigate().GoToUrl(url);

            var username = driver.FindElement(By.Name("Login[username]")); // find the username field 

            var password = driver.FindElement(By.Name("Login[password]")); // find the password field

            username.SendKeys(user); // type in the username

            Thread.Sleep(2 * 1000); // await 2 seconds 

            password.SendKeys(pw); //type in the password 

            var loginbutton = driver.FindElement(By.XPath("//*[@id=\"loginForm\"]/form/div[4]/button")); // click the login button

            loginbutton.Click();
        }

        public void Parse ( Detail detail) // call this methode to fix the parsing problems 
        {
            // get the content of the scraped firm.
            string CIF = detail.CIF;
            string SediuSocial = detail.SediuSocial;
            string Administrator = detail.Administrator;
            string Adresa = detail.Adresa;
            string Telephone = detail.Telefon;
            string Email = detail.Email;


            StringBuilder s = new StringBuilder();

            if (Telephone != null)
            {
                // parse the phone number if there is one 
                s.Append(Telephone);

                // Convert the StringBuilder into a string
                var ph = s.ToString();

                //Finish the parsing and add the new value to the "Telefon" field
                detail.Telefon = ph;

            }

            s.Clear();

            if (Email != null)
            {
                s.Append(Email);

                //convert de SB to string 
                var em = s.ToString();

                //add the new valuer to the "Email" field 
                detail.Email = em;
            }

            s.Clear();

            if (SediuSocial != null)
            {
                s.Append(SediuSocial);

                //make the appropriate changes
                s.Replace("\r\n", " ");

                //convert de SB to string 
                var ss = s.ToString();

                //add the new valuer to the "SediuSocial" field 
                detail.SediuSocial = ss;
            }

            s.Clear();

            if (Adresa != null)
            {
                s.Append(Adresa);

                //make the appropriate changes
                s.Replace("\r\n", " ");

                //convert de SB to string 
                var aa = s.ToString();

                //add the new valuer to the "SediuSocial" field 
                detail.Adresa = aa;
            }

            s.Clear();
        }

        public int found(List<string> p, string a) // this is a funtion that returns if an element was found or not 
        {
            for (int i = 0; i < p.Count; i++)
            {
                if (p[i] == a)
                    return 0;
            }
            return 1;
        }

        public Detail ScrapDetails(string code, IWebDriver driver) // this is the scraping methode 
        {


            string CIF = "";
            string SediuSocial = "";
            string Administrator = "";
            string Adresa = "";
            string Telefon = "";
            string Email = "";


            string url = "https://www.bursatransport.com/account/default/profile/company_id/" + code + "/reftype/300"; // create the url 

            driver.Navigate().GoToUrl(url); // navigate to the url 

            //Try to get the CIF
            try
            {
                CIF = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/table/tbody/tr[1]/td[2]")).Text;
            }
            catch
            {
                CIF = "";
            }

            //Try to get the HeadQuarters Adress
            try
            {
                SediuSocial = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/table/tbody/tr[4]/td[2]")).Text;
            }
            catch
            {
                SediuSocial = "";
            }

            //Try to get the Administrator
            try
            {
                Administrator = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/table/tbody/tr[5]/td[2]")).Text;
            }
            catch
            {
                Administrator = "";
            }

            //Try to get the Adress
            try
            {
                Adresa = driver.FindElement(By.XPath(" //*[@id=\"toggle\"]/div[1]/table//tr[1]/td[2]")).Text;
            }
            catch
            {
                Adresa = "";
            }

            //Try to get the Phone Number
            try
            {
                Telefon = driver.FindElement(By.XPath("//*[@id=\"toggle\"]/div[1]/table//tr[2]/td[2]/div/span")).Text;
            }
            catch
            {
                Telefon = "";
            }

            //try to get the Email
            try
            {
                Email = driver.FindElement(By.XPath(" //*[@id=\"toggle\"]/div[1]/table//tr[3]/td[2]/a")).Text;
            }
            catch
            {
                Email = "";
            }


            List<string> Phones = new List<string>();
            if (Telefon != "")
                Phones.Add(Telefon);

            List<IWebElement> phones = new List<IWebElement>();
            try
            {
                phones = driver.FindElements(By.ClassName("mobile")).ToList();
            }
            catch
            {
                phones[0] = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/table/tbody/tr[1]/td[2]"));

            }
            if (phones.Count > 0)
            {
                if (phones[0].Text != CIF)
                {
                    foreach (var d in phones)
                    {

                        string phone = d.Text;

                        if (found(Phones, phone) == 1)
                        {
                            Phones.Add(phone);

                        }
                    }
                }
            }
            if(Phones.Count > 0)
            {
                if (Phones[0] != null)
                {
                    Telefon = Phones[0];
                }
            }
            
           

            if (Phones.Count > 1)
            {
                for (int i = 1; i < Phones.Count; i++)
                {
                    Telefon += ", " + Phones[i]; // add all the phone numbers to the "Telefon" field
                }
            }

            List<string> Emails = new List<string>();

            if (Email != "")
                Emails.Add(Email);

            List<IWebElement> emails = new List<IWebElement>();
            try
            {
                emails = driver.FindElements(By.ClassName("contactValue")).ToList();
            }
            catch
            {
                emails[0] = driver.FindElement(By.XPath("/html/body/div[2]/div[1]/div[1]/table/tbody/tr[1]/td[2]"));
            }


            if(emails.Count > 0 )
            {
                if (emails[0].Text != CIF)
                {
                    foreach (var em in emails)
                    {
                        string ema = em.Text;
                        if (ema.Contains("@") && found(Emails, ema) == 1)
                        {
                            Emails.Add(ema);
                        }
                    }
                }
            }
            
            if(Emails.Count > 0 )
            {
                Email = Emails[0];
            }
            

            if (Emails.Count > 1)
            {
                for (int i = 1; i < Emails.Count; i++)
                {
                    Email += ", " + Emails[i];
                }
            }

            Detail details = new Detail() { //create the final detail with all of the information in it
                CIF = CIF,
                SediuSocial = SediuSocial,
                Administrator = Administrator,
                Adresa = Adresa,
                Email = Email,
                Telefon = Telefon
            };

            return details;
        }

    }
}