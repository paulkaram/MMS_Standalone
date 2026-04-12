using MMS.DAL.Models.MMS;
using System.Linq.Expressions;
namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IRecommendationNoteRepository : IRepository<RecommendationNote>
	{
		Task<List<RecommendationNote>> ListWithPaginationAsync(Expression<Func<RecommendationNote, bool>> filter,int Page,int PageSize);

	}
}
