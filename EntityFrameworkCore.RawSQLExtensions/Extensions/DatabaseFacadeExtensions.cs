using EntityFrameworkCore.RawSQLExtensions.SqlQuery;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace EntityFrameworkCore.RawSQLExtensions.Extensions
{
    public static class DatabaseFacadeExtensions
    {
        public static ISqlQuery<T> SqlQuery<T>(this DatabaseFacade database, string sqlQuery, params SqlParameter[] parameters)
        {
            return new SqlRawQuery<T>(database, sqlQuery, parameters);
        }
        public static ISqlQuery<T> SqlQuery<T>(this DatabaseFacade database, string sqlQuery, IEnumerable<SqlParameter> parameters)
        {
            return new SqlRawQuery<T>(database, sqlQuery, parameters.ToArray());
        }

        public static ISqlQuery<T> StoredProcedure<T>(this DatabaseFacade database, string storedProcName, params SqlParameter[] parameters)
        {
            return new StoredProcedure<T>(database, storedProcName, parameters);
        }
        public static ISqlQuery<T> StoredProcedure<T>(this DatabaseFacade database, string storedProcName, IEnumerable<SqlParameter> parameters)
        {
            return new StoredProcedure<T>(database, storedProcName, parameters.ToArray());
        }
    }
}
