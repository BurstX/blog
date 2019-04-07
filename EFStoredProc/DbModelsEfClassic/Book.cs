using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject1.DbModelsEfClassic
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }
        public string Author { get; set; }

        public List<Review> Reviews { get; set; }
    }
}