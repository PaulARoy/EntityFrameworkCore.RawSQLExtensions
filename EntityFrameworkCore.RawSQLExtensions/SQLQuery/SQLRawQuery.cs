using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EntityFrameworkCore.RawSQLExtensions
{
    public class SQLRawQuery<T> : SqlQueryBase<T> where T : class
    {
        private string _sqlQuery;

        public SQLRawQuery(DatabaseFacade databaseFacade, string sqlQuery, params SqlParameter[] sqlParameters) : base(databaseFacade, sqlParameters)
        {
            _sqlQuery = sqlQuery;
        }
        
        #region "Private Helpers"

        protected override async Task<U> ExecuteAsync<U>(Func<DbDataReader, Task<U>> databaseReaderAction)
        {
            U result = default(U);

            var conn = _databaseFacade.GetDbConnection();
            try
            {
                await conn.OpenAsync();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = _sqlQuery;

                    foreach (var param in _sqlParameters)
                    {
                        var p = command.CreateParameter();
                        p.ParameterName = param.ParameterName;
                        p.Value = param.Value;
                        command.Parameters.Add(p);
                    }

                    DbDataReader reader = await command.ExecuteReaderAsync();
                    result = await databaseReaderAction.Invoke(reader);
                    reader.Dispose();
                }
            }
            finally
            {
                conn.Close();
            }
            return result;
        }

        #endregion
    }
}
