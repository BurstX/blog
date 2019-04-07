using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestProject1.DbModelsEfClassic;

namespace UnitTestProject1
{
    [TestClass]
    public class EfClassicTests
    {
        private EfClassicContext dbClassicContext;

        [TestInitialize]
        public void TestInit()
        {
            dbClassicContext = new EfClassicContext();

            dbClassicContext.Database.CreateIfNotExists();

            // clean the DB
            dbClassicContext.Database.ExecuteSqlCommand("DELETE FROM Reviews");
            dbClassicContext.Database.ExecuteSqlCommand("DELETE FROM Books");

            // create initial test data
            var books = new[]
            {
                new Book()
                {
                    Author = "Vadim",
                    Title = "Title 1",
                },
                new Book()
                {
                    Author = "Dmitry",
                    Title = "Title 2",
                },
                new Book()
                {
                    Author = "Igor",
                    Title = "Title 3",
                },
            };

            var reviews = new[]
            {
                new Review()
                {
                    Book = books[0],
                    ReviewText = "Review 1",
                },
                new Review()
                {
                    Book = books[0],
                    ReviewText = "Review 2",
                },
                new Review()
                {
                    Book = books[1],
                    ReviewText = "Review 1",
                },
                new Review()
                {
                    Book = books[1],
                    ReviewText = "Review 2",
                },
                new Review()
                {
                    Book = books[2],
                    ReviewText = "Review 1",
                },
                new Review()
                {
                    Book = books[2],
                    ReviewText = "Review 2",
                },
            };

            foreach (var book in books)
            {
                dbClassicContext.Books.Add(book);
            }

            foreach (var review in reviews)
            {
                dbClassicContext.Reviews.Add(review);
            }

            dbClassicContext.SaveChanges();

            // stored proc for reads
            try
            {
                dbClassicContext.Database.ExecuteSqlCommand("DROP PROCEDURE sp_ReadBooks");
            }
            catch
            {
                // intentionally left blank
            }

            dbClassicContext.Database.ExecuteSqlCommand(@"
            CREATE PROCEDURE sp_ReadBooks
	                @id int = 0
                AS
                BEGIN
	                SET NOCOUNT ON;

                    if @id = 0
                    begin
                        SELECT * from Books;
                    end
                    else begin
	                    SELECT * from Books where Id = @id;
                    end
                END
            ");
        }

        [TestMethod]
        public void Ef6CascadeDeletesWithStoredProc()
        {
            //var book = new Book() { Id = 1 };
            //dbClassicContext.Books.Attach(book);

            var book = dbClassicContext.Books.Single(b => b.Id == 1);

            dbClassicContext.Books.Remove(book);

            dbClassicContext.SaveChanges();

            //foreach (var book in books)
            //{
            //    Assert.IsFalse(string.IsNullOrEmpty(book.Author));
            //}
        }

        [TestMethod]
        public void SqlQueryReadFailsWhenNotAllColumnsInSelect()
        {
            // stored proc for reads is dropped to be recreated with "SELECT <column>, ..." instead of "SELECT *..."
            try
            {
                dbClassicContext.Database.ExecuteSqlCommand("DROP PROCEDURE sp_ReadBooks");
            }
            catch
            {
                // intentionally left blank
            }

            dbClassicContext.Database.ExecuteSqlCommand(@"
            CREATE PROCEDURE sp_ReadBooks
	                @id int = 0
                AS
                BEGIN
	                SET NOCOUNT ON;

                    if @id = 0
                    begin
                        SELECT Id, Title from Books;
                    end
                    else begin
	                    SELECT Id, Title from Books where Id = @id;
                    end
                END
            ");

            var books = dbClassicContext.Books.SqlQuery(@"
                            EXEC sp_ReadBooks
                        ")
                        .ToList();
        }

        [TestMethod]
        public void EfCoreReadsWithProjections()
        {
            // TODO: use MSSQL profiler to track the T-SQL query generated
            var books = dbClassicContext.Books.Select(b => new { b.Author })
                .ToList();

            foreach (var book in books)
            {
                Assert.IsFalse(string.IsNullOrEmpty(book.Author));
            }
        }

        [TestMethod]
        public void IncludeCanBeUsedAfterSelectQueryExecution()
        {
            var books = dbClassicContext.Books.SqlQuery(@"
                            SELECT * FROM Books
                        ")
                        //.Include(b => b.Reviews)
                        .ToList();

            foreach (var book in books)
            {
                Assert.IsTrue(book.Reviews?.Any() ?? false);
            }
        }

        [TestMethod]
        public void IncludeCanNotBeUsedAfterStoredProcExecution()
        {
            var books = dbClassicContext.Books.SqlQuery(@"
                            EXEC sp_ReadBooks
                        ")
                //.Include(b => b.Reviews)
                .ToList();

            foreach (var book in books)
            {
                Assert.IsTrue(book.Reviews?.Any() ?? false);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            dbClassicContext?.Dispose();
        }
    }
}