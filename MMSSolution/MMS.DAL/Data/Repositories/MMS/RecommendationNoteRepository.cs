using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
	internal class RecommendationNoteRepository : Repository<RecommendationNote>, IRecommendationNoteRepository
	{
		MmsContext ContextAsMMSContext => (Context as MmsContext)!;
		public RecommendationNoteRepository(DbContext context) : base(context)
		{
		}

		public async Task<List<RecommendationNote>> ListWithPaginationAsync(Expression<Func<RecommendationNote, bool>> filter, int Page, int PageSize)
		{
			return await ContextAsMMSContext.RecommendationNotes.Where(filter).Skip((Page - 1) * PageSize).Take(PageSize).ToListAsync();
		}
	}
}
