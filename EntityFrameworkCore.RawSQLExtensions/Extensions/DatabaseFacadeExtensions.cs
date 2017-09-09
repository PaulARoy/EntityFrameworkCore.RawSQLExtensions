using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace EntityFrameworkCore.RawSQLExtensions.Extensions
{
    public static class DatabaseFacadeExtensions
    {
        public static ISqlQuery<T> SqlQuery<T>(this DatabaseFacade database, string sqlQuery, params SqlParameter[] parameters) where T : class
        {
            return new SQLRawQuery<T>(database, sqlQuery, parameters);
        }
        public static ISqlQuery<T> SqlQuery<T>(this DatabaseFacade database, string sqlQuery, IEnumerable<SqlParameter> parameters) where T : class
        {
            return new SQLRawQuery<T>(database, sqlQuery, parameters.ToArray());
        }

        public static ISqlQuery<T> StoredProcedure<T>(this DatabaseFacade database, string storedProcName, params SqlParameter[] parameters) where T : class
        {
            return new SQLRawQuery<T>(database, storedProcName, parameters);
        }
        public static ISqlQuery<T> StoredProcedure<T>(this DatabaseFacade database, string storedProcName, IEnumerable<SqlParameter> parameters) where T : class
        {
            return new SQLRawQuery<T>(database, storedProcName, parameters.ToArray());
        }
    }
}
