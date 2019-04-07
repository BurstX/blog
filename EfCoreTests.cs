using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestProject1.DbModels;

namespace UnitTestProject1
{
    [TestClass]
    public class EfCoreTests
    {
        private EfCoreContext _dbCoreContext;

        [TestInitialize]
        public void TestInit()
        {
            var optionsBuilder = new DbContextOptionsBuilder<EfCoreContext>();
            optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["DEV26"].ConnectionString);

            _dbCoreContext = new EfCoreContext(optionsBuilder.Options);

            _dbCoreContext.Database.EnsureCreated();

            // clean the DB
            _dbCoreContext.Database.ExecuteSqlCommand("DELETE FROM Reviews");
            _dbCoreContext.Database.ExecuteSqlCommand("DELETE FROM Books");

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
                _dbCoreContext.Add(book);
            }

            foreach (var review in reviews)
            {
                _dbCoreContext.Add(review);
            }

            _dbCoreContext.SaveChanges();

            // stored proc for reads
            try
            {
                _dbCoreContext.Database.ExecuteSqlCommand("DROP PROCEDURE sp_ReadBooks");
            }
            catch
            {
                // intentionally left blank
            }

            _dbCoreContext.Database.ExecuteSqlCommand(@"
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
        public void FromSqlReadFailsWhenNotAllColumnsInSelect()
        {
            // stored proc for reads is dropped to be recreated with "SELECT <column>, ..." instead of "SELECT *..."
            try
            {
                _dbCoreContext.Database.ExecuteSqlCommand("DROP PROCEDURE sp_ReadBooks");
            }
            catch
            {
                // intentionally left blank
            }

            _dbCoreContext.Database.ExecuteSqlCommand(@"
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

            var books = _dbCoreContext.Books.FromSql(@"
                            EXEC sp_ReadBooks
                        ")
                        .ToList();
        }

        [TestMethod]
        public void EfCoreFromSqlReadsWithIncludeProducesTwoConsecutiveQueries()
        {
            // TODO: use MSSQL profiler to track the T-SQL query generated
            var books = _dbCoreContext.Books
                .FromSql("SELECT * FROM Books")
                .Include(b => b.Reviews)
                .ToList();
        }

        [TestMethod]
        public void EfCoreReadsWithIncludeProducesSingleJoinQuery()
        {
            // TODO: use MSSQL profiler to track the T-SQL query generated
            var books = _dbCoreContext.Books
                        .Include(b => b.Reviews)
                        .ToList();
        }

        [TestMethod]
        public void EfCoreReadsWithProjections()
        {
            // TODO: use MSSQL profiler to track the T-SQL query generated
            var books = _dbCoreContext.Books.Select(b => new { b.Author })
                .ToList();

            foreach (var book in books)
            {
                Assert.IsFalse(string.IsNullOrEmpty(book.Author));
            }
        }

        [TestMethod]
        public void IncludeCanBeUsedAfterSelectQueryExecution()
        {
            var books = _dbCoreContext.Books.FromSql(@"
                            SELECT * FROM Books
                        ")
                        .Include(b => b.Reviews)
                        .ToList();

            foreach (var book in books)
            {
                Assert.IsTrue(book.Reviews?.Any() ?? false);
            }
        }

        [TestMethod]
        public void IncludeCanNotBeUsedAfterStoredProcExecution()
        {
            var books = _dbCoreContext.Books.FromSql(@"
                            EXEC sp_ReadBooks
                        ")
                .Include(b => b.Reviews)
                .ToList();

            foreach (var book in books)
            {
                Assert.IsTrue(book.Reviews?.Any() ?? false);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _dbCoreContext?.Dispose();
        }
    }
}