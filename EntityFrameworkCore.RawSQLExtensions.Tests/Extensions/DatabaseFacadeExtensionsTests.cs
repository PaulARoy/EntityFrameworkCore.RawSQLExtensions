using System.Data.SqlClient;
using EntityFrameworkCore.RawSQLExtensions.Extensions;
using EntityFrameworkCore.RawSQLExtensions.SqlQuery;
using FakeItEasy;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;

namespace EntityFrameworkCore.RawSQLExtensions.Tests.Extensions;

[TestFixture]
public class DatabaseFacadeExtensionsTests
{
    [Test]
    public void SqlQueryReturnsSqlRawQuery()
    {
        var databaseFacade = A.Fake<DatabaseFacade>();
        var query = databaseFacade.SqlQuery<string>("");
        Assert.That(query, Is.InstanceOf<SqlRawQuery<string>>());
    }

    [Test]
    public void SqlQueryWithParametersReturnsSqlRawQuery()
    {
        var databaseFacade = A.Fake<DatabaseFacade>();
        var query = databaseFacade.SqlQuery<string>("", new SqlParameter());
        Assert.That(query, Is.InstanceOf<SqlRawQuery<string>>());
    }

    [Test]
    public void StoredProcedureReturnsStoredProcedure()
    {
        var databaseFacade = A.Fake<DatabaseFacade>();
        var query = databaseFacade.StoredProcedure<string>("");
        Assert.That(query, Is.InstanceOf<StoredProcedure<string>>());
    }

    [Test]
    public void StoredProcedureWithParametersReturnsStoredProcedure()
    {
        var databaseFacade = A.Fake<DatabaseFacade>();
        var query = databaseFacade.StoredProcedure<string>("", new SqlParameter());
        Assert.That(query, Is.InstanceOf<StoredProcedure<string>>());
    }
}
