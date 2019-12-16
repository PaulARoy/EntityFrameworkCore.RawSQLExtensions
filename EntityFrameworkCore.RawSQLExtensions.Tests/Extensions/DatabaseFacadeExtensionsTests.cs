using NUnit.Framework;
using FakeItEasy;
using Microsoft.EntityFrameworkCore.Infrastructure;
using EntityFrameworkCore.RawSQLExtensions.Extensions;
using EntityFrameworkCore.RawSQLExtensions.SqlQuery;
using System.Data.SqlClient;
using System.Data.Common;

namespace EntityFrameworkCore.RawSQLExtensions.Tests.Extensions
{
    [TestFixture]
    public class DatabaseFacadeExtensionsTests
    {
        [Test]
        public void SqlQueryReturnsSqlRawQuery()
        {
            var databaseFacade = A.Fake<DatabaseFacade>();
            var query = databaseFacade.SqlQuery<string>("");
            Assert.IsInstanceOf<SqlRawQuery<string>>(query);
        }

        [Test]
        public void SqlQueryWithParametersReturnsSqlRawQuery()
        {
            var databaseFacade = A.Fake<DatabaseFacade>();
            var query = databaseFacade.SqlQuery<string>("", new SqlParameter());
            Assert.IsInstanceOf<SqlRawQuery<string>>(query);
        }

        [Test]
        public void StoredProcedureReturnsStoredProcedure()
        {
            var databaseFacade = A.Fake<DatabaseFacade>();
            var query = databaseFacade.StoredProcedure<string>("");
            Assert.IsInstanceOf<StoredProcedure<string>>(query);
        }

        [Test]
        public void StoredProcedureWithParametersReturnsStoredProcedure()
        {
            var databaseFacade = A.Fake<DatabaseFacade>();
            var query = databaseFacade.StoredProcedure<string>("", new SqlParameter());
            Assert.IsInstanceOf<StoredProcedure<string>>(query);
        }
    }
}
