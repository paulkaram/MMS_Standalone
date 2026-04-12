using MMS.DAL.Core.Repositories.AuditLogs;
using MMS.DAL.Core.UnitOfWork.AuditLogs;
using MMS.DAL.Data.Repositories.AuditLogs;
using MMS.DAL.Models.AuditLogs;

namespace MMS.DAL.Data.UnitOfWork.AuditLogs
{
    internal class AuditLogsUnitOfWork : UnitOfWork, IAuditLogUnitOfWork
    {
        public IAuditLogActivityRepository ActivityLogs { get; private set; }

        public AuditLogsUnitOfWork(MomraAuditTrailContext dbContext) : base(dbContext)
        {
            ActivityLogs = new ActivityLogRepository(dbContext);
        }
    }
}
