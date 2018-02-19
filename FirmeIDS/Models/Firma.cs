using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FirmeIDS.Models
{
    public class Firma
    {
        [Key]
        public int Id { get; set; }
        public string CodFirma { get; set;}
        public string Nume { get; set; }
        public string DataInscrierii { get; set; }
        public string AnulInfintarii { get; set; }
        public string ActivitatePrincipala { get; set; }
        public int Flag { get; set; }
    }
}