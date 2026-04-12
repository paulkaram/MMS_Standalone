using MMS.DAL.Core.Repositories.AuditLogs;
using MMS.DAL.Models.AuditLogs;
using Microsoft.EntityFrameworkCore;

namespace MMS.DAL.Data.Repositories.AuditLogs
{
    internal class ActivityLogRepository : Repository<ActivityLog>, IAuditLogActivityRepository
    {
        public ActivityLogRepository(DbContext context) : base(context)
        {
        }
    }
}
