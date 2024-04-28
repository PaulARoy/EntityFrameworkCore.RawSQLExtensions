using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.RawSQLExtensions.SqlQuery;

public class SqlRawQuery<T> : SqlQueryBase<T>
{
    private readonly string _sqlQuery;

    public SqlRawQuery(DatabaseFacade databaseFacade, string sqlQuery, params DbParameter[] sqlParameters)
        : base(databaseFacade, sqlParameters)
    {
        _sqlQuery = sqlQuery;
    }

    #region "Private Helpers"

    protected override void InitCommand(DbCommand command)
    {
        command.CommandText = _sqlQuery;
    }

    #endregion
}
