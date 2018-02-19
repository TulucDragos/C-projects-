using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace FirmeIDS.Models
{
    public class Page
    {
        [Key]
        public int Id { get; set; }
        public int PageNumber { get; set; }
        public int Flag { get; set; }
    }
}