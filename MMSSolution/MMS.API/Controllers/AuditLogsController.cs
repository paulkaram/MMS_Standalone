using Microsoft.AspNetCore.Mvc;
using MMS.BLL.Managers;
using MMS.DTO;
using MMS.DAL.Models.AuditLogs;
using MMS.API.Common;

namespace MMS.API.Controllers
{
    [Route("api/auditLogs")]
    [ApiController]
    public class AuditLogsController : IntalioBaseController
    {
        private readonly AuditLogsManager _auditLogsManager;

        public AuditLogsController(AuditLogsManager auditLogsManager)
        {
            _auditLogsManager = auditLogsManager;
        }

        [HttpGet]
        public async Task<IActionResult> ListAuditLogs(int page, int pageSize, string? search)
        {
            try
            {
                var logs = await _auditLogsManager.ListAuditLogsAsync(page, pageSize, search);

                return Ok(new ApiResponseDto<GenericPaginationListDto<ActivityLog>>(logs));
            }
            catch (Exception ex)
            {
                return base.ErrorResponse(ex);
            }
        }
    }
}
