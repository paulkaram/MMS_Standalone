
using System.Linq.Expressions;

namespace MMS.BLL.Common.Helpers
{
	public interface IFilterHelper
	{
		Expression<Func<T, bool>> Combine<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2);
		Expression Replace(Expression expression);
	}
}
