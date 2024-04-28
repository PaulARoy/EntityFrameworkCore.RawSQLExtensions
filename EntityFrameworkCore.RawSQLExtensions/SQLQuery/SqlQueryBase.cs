using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using EntityFrameworkCore.RawSQLExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.RawSQLExtensions.SqlQuery;

public abstract class SqlQueryBase<T>(DatabaseFacade databaseFacade, params DbParameter[] sqlParameters) : ISqlQuery<T>
{
    protected DatabaseFacade _databaseFacade = databaseFacade;
    protected DbParameter[] _sqlParameters = sqlParameters;

    #region "ISQLQuery<T>"

    public async Task<IList<T>> ToListAsync() => await ExecuteAsync((dbReader) => dbReader.ToListAsync<T>());

    public async Task<T> FirstOrDefaultAsync() => await ExecuteAsync((dbReader) => dbReader.FirstOrDefaultAsync<T>());

    public async Task<T> SingleOrDefaultAsync() => await ExecuteAsync((dbReader) => dbReader.SingleOrDefaultAsync<T>());

    public async Task<T> FirstAsync()
    {
        var result = await FirstOrDefaultAsync();
        return result == null ? throw new InvalidOperationException("Sequence contains no elements") : result;
    }

    public async Task<T> SingleAsync()
    {
        var result = await SingleOrDefaultAsync();
        return result == null ? throw new InvalidOperationException("Sequence contains no elements") : result;
    }

    public IList<T> ToList() => Execute((dbReader) => dbReader.ToList<T>());

    public DataTable ToDataTable() => Execute((dbReader) => dbReader.ToDataTable());

    public async Task<DataTable> ToDataTableAsync() => await ExecuteAsync((dbReader) => dbReader.ToDataTableAsync());

    public T FirstOrDefault() => Execute((dbReader) => dbReader.FirstOrDefault<T>());

    public T SingleOrDefault() => Execute((dbReader) => dbReader.SingleOrDefault<T>());

    public T First()
    {
        var result = FirstOrDefault();
        return result == null ? throw new InvalidOperationException("Sequence contains no elements") : result;
    }

    public T Single()
    {
        var result = SingleOrDefault();
        return result == null ? throw new InvalidOperationException("Sequence contains no elements") : result;
    }

    #endregion "ISQLQuery<T>"

    #region "Implementation"

    // customization of command (sql / stored procedure)
    protected abstract void InitCommand(DbCommand command);

    private async Task<U> ExecuteAsync<U>(Func<DbDataReader, Task<U>> databaseReaderAction)
    {
        U result = default;

        var conn = _databaseFacade.GetDbConnection();
        try
        {
            await conn.OpenAsync();
            using var command = conn.CreateCommand();
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
        finally
        {
            conn.Close();
        }
        return result;
    }

    private U Execute<U>(Func<DbDataReader, U> databaseReaderAction)
    {
        U result = default;

        var conn = _databaseFacade.GetDbConnection();
        try
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            using var command = conn.CreateCommand();
            InitCommand(command);

            foreach (var param in _sqlParameters)
            {
                var p = command.CreateParameter();
                p.ParameterName = param.ParameterName;
                p.Value = param.Value;
                command.Parameters.Add(p);
            }

            DbDataReader reader = command.ExecuteReader();
            result = databaseReaderAction.Invoke(reader);
            reader.Dispose();
        }
        finally
        {
            conn.Close();
        }
        return result;
    }

    #endregion "Implementation"
}
