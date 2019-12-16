using EntityFrameworkCore.RawSQLExtensions.SqlQuery;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace EntityFrameworkCore.RawSQLExtensions.Extensions
{
    public static class DatabaseFacadeExtensions
    {
        public static ISqlQuery<object> SqlQuery(this DatabaseFacade database, Type type, string sqlQuery, params  DbParameter[] parameters)
        {
            var tsrq = typeof(SqlRawQuery<>).MakeGenericType(type);
            return (ISqlQuery<object>)Activator.CreateInstance(tsrq, database, sqlQuery, parameters);
        }

        public static ISqlQuery<object> SqlQuery(this DatabaseFacade database, string sqlQuery, params DbParameter[] parameters)
        {
            var tsrq = typeof(SqlRawQuery<>).MakeGenericType(typeof(object));
            return (ISqlQuery<object>)Activator.CreateInstance(tsrq, database, sqlQuery, parameters);
        }

        public static ISqlQuery<T> SqlQuery<T>(this DatabaseFacade database, string sqlQuery, params DbParameter[] parameters)
        {
            return new SqlRawQuery<T>(database, sqlQuery, parameters);
        }

        public static ISqlQuery<T> SqlQuery<T>(this DatabaseFacade database, string sqlQuery, IEnumerable<DbParameter> parameters)
        {
            return new SqlRawQuery<T>(database, sqlQuery, parameters.ToArray());
        }

        public static ISqlQuery<T> StoredProcedure<T>(this DatabaseFacade database, string storedProcName, params DbParameter[] parameters)
        {
            return new StoredProcedure<T>(database, storedProcName, parameters);
        }

        public static ISqlQuery<T> StoredProcedure<T>(this DatabaseFacade database, string storedProcName, IEnumerable<DbParameter> parameters)
        {
            return new StoredProcedure<T>(database, storedProcName, parameters.ToArray());
        }
    }
}