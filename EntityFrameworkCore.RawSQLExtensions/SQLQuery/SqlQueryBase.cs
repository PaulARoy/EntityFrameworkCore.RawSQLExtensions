using EntityFrameworkCore.RawSQLExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EntityFrameworkCore.RawSQLExtensions.SqlQuery
{
    public abstract class SqlQueryBase<T> : ISqlQuery<T>
    {
        protected DatabaseFacade _databaseFacade;
        protected SqlParameter[] _sqlParameters;

        public SqlQueryBase(DatabaseFacade databaseFacade, params SqlParameter[] sqlParameters)
        {
            _databaseFacade = databaseFacade;
            _sqlParameters = sqlParameters;
        }


        #region "ISQLQuery<T>"

        public async Task<IList<T>> ToListAsync()
        {
            return await ExecuteAsync((dbReader) => dbReader.ToListAsync<T>());
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            return await ExecuteAsync((dbReader) => dbReader.FirstOrDefaultAsync<T>());
        }

        public async Task<T> SingleOrDefaultAsync()
        {
            return await ExecuteAsync((dbReader) => dbReader.SingleOrDefaultAsync<T>());
        }

        public async Task<T> FirstAsync()
        {
            var result = await FirstOrDefaultAsync();
            if (result == null)
                throw new InvalidOperationException("Sequence contains no elements");

            return result;
        }

        public async Task<T> SingleAsync()
        {
            var result = await SingleOrDefaultAsync();
            if (result == null)
                throw new InvalidOperationException("Sequence contains no elements");

            return result;
        }

        #endregion


        #region "Implementation"

        // customization of command (sql / stored procedure)
        protected abstract void InitCommand(DbCommand command);

        private async Task<U> ExecuteAsync<U>(Func<DbDataReader, Task<U>> databaseReaderAction)
        {
            U result = default(U);

            var conn = _databaseFacade.GetDbConnection();
            try
            {
                await conn.OpenAsync();
                using (var command = conn.CreateCommand())
                {
                    InitCommand(command);

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
