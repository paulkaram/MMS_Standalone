using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;
using System.Linq.Expressions;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class AttachmentRepository : Repository<Attachment>, IAttachmentRepository
	{
		MmsContext ContextAsMMSContext => (Context as MmsContext)!;
		public AttachmentRepository(DbContext context) : base(context)
		{
		}

		public async Task<Attachment> GetLastAsync(Expression<Func<Attachment, bool>> filter)
		{
			return await ContextAsMMSContext.Attachments
									 .Where(filter)
									 .OrderByDescending(a => a.Id)
									 .FirstOrDefaultAsync();
		}
		public async Task<Attachment> GetAsyncIncludeSignatures(Expression<Func<Attachment, bool>> filter)
		{
			return await ContextAsMMSContext.Attachments
									 .Where(filter)
									 .Include(x=>x.AttachmentsSignatures)
									 .FirstOrDefaultAsync();
		}

		public async Task<List<Attachment>> ListWithPaginationAsync(Expression<Func<Attachment, bool>> filter, int Page, int PageSize)
		{
			return await ContextAsMMSContext.Attachments.Include(x=>x.Privacy).Include(x => x.RecordType).AsNoTracking()
                                     .Where(filter).Skip((Page-1)*PageSize).Take(PageSize).ToListAsync();
		}
		public async Task<List<Attachment>> ListIncludePrivacyAndType(Expression<Func<Attachment, bool>> filter)
		{
			return await ContextAsMMSContext.Attachments.Include(x => x.Privacy).Include(x=>x.RecordType).AsNoTracking()
                                     .Where(filter).ToListAsync();
		}
	}
}