using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace UnitTestProject1.DbModels
{
    public class EfCoreContext : DbContext
    {
        public EfCoreContext()
        {
        }

        public EfCoreContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        public DbSet<Review> Reviews { get; set; }
    }
}