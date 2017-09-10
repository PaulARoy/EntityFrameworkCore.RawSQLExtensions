using EntityFrameworkCore.RawSQLExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EntityFrameworkCore.RawSQLExtensions
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
            return await ExecuteAsync<IList<T>>((dbReader) => dbReader.ToListAsync<T>());
        }

        public async Task<T> FirstOrDefaultAsync()
        {
            return await ExecuteAsync<T>((dbReader) => dbReader.FirstOrDefaultAsync<T>());
        }

        public async Task<T> SingleOrDefaultAsync()
        {
            return await ExecuteAsync<T>((dbReader) => dbReader.SingleOrDefaultAsync<T>());
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

        protected abstract Task<U> ExecuteAsync<U>(Func<DbDataReader, Task<U>> databaseReaderAction);
        
        #endregion
    }
}
