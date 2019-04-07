using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject1.DbModelsEfClassic
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        public Book Book { get; set; }

        public string ReviewText { get; set; }
    }
}