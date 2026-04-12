using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Core.Repositories.MMS
{
	public interface IAttachmentRepository : IRepository<Attachment>
	{
		Task<Attachment> GetLastAsync(Expression<Func<Attachment, bool>> filter);
		Task<Attachment> GetAsyncIncludeSignatures(Expression<Func<Attachment, bool>> filter);
		Task<List<Attachment>> ListWithPaginationAsync(Expression<Func<Attachment, bool>> filter, int Page, int PageSize);
		Task<List<Attachment>> ListIncludePrivacyAndType(Expression<Func<Attachment, bool>> filter);
	}
}