
using System.Linq.Expressions;

namespace MMS.BLL.Common.Helpers
{
	internal class FilterHelper : ExpressionVisitor, IFilterHelper
	{
		private readonly Expression _oldValue;
		private readonly Expression _newValue;

		public FilterHelper(Expression oldValue=null, Expression newValue=null)
		{
			_oldValue = oldValue;
			_newValue = newValue;
		}

		public Expression<Func<T, bool>> Combine<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
		{
			var parameter = Expression.Parameter(typeof(T), "x");

			var leftVisitor = new FilterHelper(expr1.Parameters[0], parameter);
			var left = leftVisitor.Replace(expr1.Body);

			var rightVisitor = new FilterHelper(expr2.Parameters[0], parameter);
			var right = rightVisitor.Replace(expr2.Body);

			return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
		}

		public Expression Replace(Expression expression)
		{
			return Visit(expression);
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			return node == _oldValue ? _newValue : base.VisitParameter(node);
		}
	}
}
