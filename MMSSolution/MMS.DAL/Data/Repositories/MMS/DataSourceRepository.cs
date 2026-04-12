using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class DataSourceRepository : Repository<DataSource>, IDataSourceRepository
    {
        MmsContext ContextAsMMSContext => (Context as MmsContext)!;

        public DataSourceRepository(DbContext context) : base(context)
        {
        }

        public DataSource? Get(Expression<Func<DataSource, bool>> filter)
        {
            return ContextAsMMSContext.DataSources.FirstOrDefault(filter);
        }
    }
}
