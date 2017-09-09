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
    public abstract class SqlQueryBase<T> : ISqlQuery<T> where T : class
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
            return await FirstOrDefaultAsync() ?? throw new InvalidOperationException("Sequence contains no elements");
        }

        public async Task<T> SingleAsync()
        {
            return await SingleOrDefaultAsync() ?? throw new InvalidOperationException("Sequence contains no elements");
        }

        #endregion


        #region "Implementation"

        protected abstract Task<U> ExecuteAsync<U>(Func<DbDataReader, Task<U>> databaseReaderAction);
        
        #endregion
    }
}
