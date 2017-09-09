using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityFrameworkCore.RawSQLExtensions
{
    public interface ISqlQuery<T> where T : class
    {
        Task<IList<T>> ToListAsync();
        
        Task<T> FirstAsync();
        Task<T> FirstOrDefaultAsync();

        Task<T> SingleAsync();
        Task<T> SingleOrDefaultAsync();
    }
}
