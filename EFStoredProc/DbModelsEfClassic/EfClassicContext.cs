using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProject1.DbModelsEfClassic
{
    public class EfClassicContext : DbContext
    {
        public EfClassicContext() : base("name=DEV26")
        {
        }

        public DbSet<Book> Books { get; set; }

        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .MapToStoredProcedures(p => p.Insert(sp => sp.HasName("sp_InsertBooks")
                                                            .Parameter(pm => pm.Author, nameof(Book.Author))
                                                            .Parameter(pm => pm.Title, nameof(Book.Title))
                                                            .Result(rs => rs.Id, nameof(Book.Id)))
                                            .Update(sp => sp.HasName("sp_UpdateBooks")
                                                            .Parameter(pm => pm.Author, nameof(Book.Author))
                                                            .Parameter(pm => pm.Title, nameof(Book.Title)))
                                            .Delete(sp => sp.HasName("sp_DeleteBooks")
                                                            .Parameter(pm => pm.Id, nameof(Book.Id)))

                );
        }
    }
}