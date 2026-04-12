using Microsoft.EntityFrameworkCore;
using MMS.DAL.Core.Repositories.MMS;
using MMS.DAL.Models.MMS;

namespace MMS.DAL.Data.Repositories.MMS
{
    internal class AttachmentAnnotationRepository : Repository<AttachmentAnnotation>, IAttachmentAnnotationRepository
	{
		public AttachmentAnnotationRepository(DbContext context) : base(context)
		{
		}
	}
}
