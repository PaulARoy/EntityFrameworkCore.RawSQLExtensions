using System.Data.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EntityFrameworkCore.RawSQLExtensions.SqlQuery;

public class StoredProcedure<T> : SqlQueryBase<T>
{
    private readonly string _storedProcedureName;

    public StoredProcedure(
        DatabaseFacade databaseFacade,
        string storedProcedureName,
        params DbParameter[] sqlParameters
    )
        : base(databaseFacade, sqlParameters)
    {
        _storedProcedureName = storedProcedureName;
    }

    #region "Private Helpers"

    protected override void InitCommand(DbCommand command)
    {
        command.CommandText = _storedProcedureName;
        command.CommandType = System.Data.CommandType.StoredProcedure;
    }

    #endregion
}
