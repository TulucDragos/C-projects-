using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using FirmeIDS.Models;
using System.Data;
using OfficeOpenXml.Drawing;
using System.Diagnostics;
using System.Reflection;
using OfficeOpenXml.Table;

namespace FirmeIDS.Controllers
{
    public class ExcelController : Controller
    {
        // GET: Excel
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0 && file.ContentLength < 2097152)
            {
                string extension = Path.GetExtension(file.FileName);
                extension.ToLower();
                if (extension == ".xlsx" || extension == ".xlsm")
                {
                    // extract only the filename
                    var fileName = Path.GetFileName(file.FileName);
                    // store the file inside ~/App_Data/uploads folder
                    var path = Path.Combine(Server.MapPath("~/Uploads"), fileName);

                    file.SaveAs(path);

                    ExcelPackage excel = new ExcelPackage(new System.IO.FileInfo(path));

                    ExcelWorksheet workSheet = excel.Workbook.Worksheets.First();

                    DataTable dt = new DataTable();

                    ApplicationDbContext db = new ApplicationDbContext();

                    var items = db.Firms.ToList();

                    Firma fr = new Firma();

                    var start = workSheet.Dimension.Start;

                    var end = workSheet.Dimension.End;

                    for (int row = start.Row + 1; row <= end.Row; row++)
                    {
                        var _CodFirma = workSheet.Cells[row, 1].Text;
                        var _Nume = workSheet.Cells[row, 2].Text;        
                        var _DataInscrierii = workSheet.Cells[row, 3].Text;
                        var _AnulInfintarii = workSheet.Cells[row, 4].Text;
                        var _ActivitatePrincipala = workSheet.Cells[row, 5].Text;
                        fr.CodFirma = _CodFirma;
                        fr.Nume = _Nume;
                        fr.DataInscrierii = _DataInscrierii;
                        fr.AnulInfintarii = _AnulInfintarii;
                        fr.ActivitatePrincipala = _ActivitatePrincipala;
                
                        db.Firms.Add(fr);
                        db.SaveChanges();

                    }

                }

            }
            return RedirectToAction("ViewFirms", "Firme");
        }

        public ActionResult Export()
        {
            ExcelPackage excel = new ExcelPackage();

            ApplicationDbContext db = new ApplicationDbContext();

            DataTable dt = new DataTable();

            ExcelWorksheet ws = excel.Workbook.Worksheets.Add("Demo");

            //here you have to add your class like student etc.  
            List<Firma> firm = db.Firms.ToList<Firma>();

            ListtoDataTableConverter converter = new ListtoDataTableConverter();

            dt = converter.ToDataTable(firm);

            ws.Cells["A1"].LoadFromDataTable(dt, true, TableStyles.Medium15);

            ws.Cells.AutoFitColumns();

            using (var memoryStream = new MemoryStream())
            {
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=ExportTest" + excel + ".xlsx");
                excel.SaveAs(memoryStream);
                memoryStream.WriteTo(Response.OutputStream);
                Response.Flush();
                Response.End();
            }


            return RedirectToAction("ViewFirms","Firme");
        }

        public class ListtoDataTableConverter
        {
            public DataTable ToDataTable<T>(List<T> items)
            {
                DataTable dataTable = new DataTable(typeof(T).Name);
                //Get all the properties    
                PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo prop in Props)
                {
                    //Setting column names as Property names    
                    dataTable.Columns.Add(prop.Name);
                }

                foreach (T item in items)
                {
                    var values = new object[Props.Length];
                    for (int i = 0; i < Props.Length; i++)
                    {
                        //inserting property values to datatable rows    
                        values[i] = Props[i].GetValue(item, null);
                    }

                    dataTable.Rows.Add(values);

                }
                //put a breakpoint here and check datatable    
                return dataTable;
            }
        }

    }
}