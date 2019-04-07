using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace UnitTestProject1.DbModels
{
    public class Review
    {
        public int Id { get; set; }

        public Book Book { get; set; }

        public string ReviewText { get; set; }
    }
}