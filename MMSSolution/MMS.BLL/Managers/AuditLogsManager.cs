using MapsterMapper;
using MMS.DAL.Core.UnitOfWork.AuditLogs;
using MMS.DAL.Models.AuditLogs;
using MMS.DTO;

namespace MMS.BLL.Managers
{
    public class AuditLogsManager
    {
        private readonly IMapper _mapper;
        private readonly IAuditLogUnitOfWork _auditLogUnitOfWork;

        public AuditLogsManager(IMapper mapper, IAuditLogUnitOfWork auditLogUnitOfWork)
        {
            _mapper = mapper;
            _auditLogUnitOfWork = auditLogUnitOfWork;
        }
        public async Task<GenericPaginationListDto<ActivityLog>?> ListAuditLogsAsync(int page, int pageSize, string? search)
        {
            var totalLogs = await _auditLogUnitOfWork.ActivityLogs.CountAsync();
            var logs = await _auditLogUnitOfWork.ActivityLogs.ListAsync(
                    page,
                    pageSize, x => search == null ||
                    x.Username.Contains(search)
                    || search.Contains(x.LetterId.Value.ToString())
                    || search.Contains(x.RecordId.Value.ToString())
                    || x.Description.Contains(search)
                    || search.Contains(x.CommentId.Value.ToString()),
                    orderBy: x => x.Id, true);
            if (!string.IsNullOrWhiteSpace(search))
            {
                totalLogs = logs.Count();
            }

            return new GenericPaginationListDto<ActivityLog>(totalLogs, logs.ToList());
        }
    }
}
