using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FirmeIDS.Models
{
    public class CompleteFirm
    {
        public string CodFirma { get; set; }
        public string Nume { get; set; }
        public string DataInscrierii { get; set; }
        public string AnulInfintarii { get; set; }
        public string ActivitatePrincipala { get; set; }   
        public string CIF { get; set; }
        public string SediuSocial { get; set; }
        public string Administrator { get; set; }
        public string Adresa { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
    }
}