using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FirmeIDS.Models
{
    public class FirmaDTO
    {
        public string CodFirma { get; set; }
        public string Nume { get; set; }
        public string DataInscrierii { get; set; }
        public string AnulInfintarii { get; set; }
        public string ActivitatePrincipala { get; set; }
        public int Flag { get; set; }
    }
}