using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;

namespace EntityFrameworkCore.RawSQLExtensions.SqlQuery
{
    public class SqlRawQuery<T> : SqlQueryBase<T>
    {
        private string _sqlQuery;

        public SqlRawQuery(DatabaseFacade databaseFacade, string sqlQuery, params DbParameter[] sqlParameters) : base(databaseFacade, sqlParameters)
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
}