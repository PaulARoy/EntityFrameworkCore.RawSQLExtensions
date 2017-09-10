using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;
using System.Data.SqlClient;

namespace EntityFrameworkCore.RawSQLExtensions.SqlQuery
{
    public class StoredProcedure<T> : SqlQueryBase<T>
    {
        private string _storedProcedureName;

        public StoredProcedure(DatabaseFacade databaseFacade, string storedProcedureName, params SqlParameter[] sqlParameters) : base(databaseFacade, sqlParameters)
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
}
