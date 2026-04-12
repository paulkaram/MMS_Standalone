using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class AttachmentVersionRepository : Repository<AttachmentVersion>, IAttachmentVersionRepository
	{
		public AttachmentVersionRepository(DbContext context) : base(context)
		{
		}

	}
}