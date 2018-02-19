using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FirmeIDS.Models
{
    public class Detail
    {
        [Key]
        public int Id { get; set; }

        public string CodFirma { get; set; }      
        public string CIF { get; set; }
        public string SediuSocial { get; set; }
        public string Administrator { get; set; }
        public string Adresa { get; set; }
        public string Email { get; set; }
        public string Telefon { get; set; }
    }
}