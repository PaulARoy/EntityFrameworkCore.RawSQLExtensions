using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityFrameworkCore.RawSQLExtensions.SqlQuery
{
    public interface ISqlQuery<T>
    {
        Task<IList<T>> ToListAsync();

        Task<T> FirstAsync();
        Task<T> FirstOrDefaultAsync();

        Task<T> SingleAsync();
        Task<T> SingleOrDefaultAsync();

        IList<T> ToList();

        T First();
        T FirstOrDefault();

        T Single();
        T SingleOrDefault();
    }
}