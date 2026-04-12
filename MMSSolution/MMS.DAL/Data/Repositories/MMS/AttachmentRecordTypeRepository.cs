using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class AttachmentRecordTypeRepository : Repository<AttachmentRecordType>, IAttachmentRecordTypeRepository
	{
		public AttachmentRecordTypeRepository(DbContext context) : base(context)
		{
		}

	}
}