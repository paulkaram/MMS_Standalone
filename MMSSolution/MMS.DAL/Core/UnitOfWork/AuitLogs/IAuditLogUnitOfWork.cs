
using MMS.DAL.Core.Repositories.AuditLogs;

namespace MMS.DAL.Core.UnitOfWork.AuditLogs
{
    public interface IAuditLogUnitOfWork : IUnitOfWork
	{
        IAuditLogActivityRepository ActivityLogs { get; }
    }
}
