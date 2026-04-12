using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IDataSourceRepository : IRepository<DataSource>
	{
        public DataSource? Get(Expression<Func<DataSource, bool>> filter);

    }
}
