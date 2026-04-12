using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Dashboard;
using MMS.DTO.Reports;

namespace MMS.API.Controllers
{
	[Route("api/reports")]
	[ApiController]
	public class ReportsController : IntalioBaseController
	{
		private readonly ReportsManager _reportsManager;
		public ReportsController(ReportsManager reportsManager)
		{
			_reportsManager = reportsManager;
		}
		[HttpGet("comittees-summary/{page}/{pageSize}")]
		[RequiredPermission(PermissionDbEnum.ComitteesSummaryReport, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetCouncilsComitteesCount(int page,int pageSize)
		{
			try
			{
				var comittees = await _reportsManager.GetComitteeSummaryReport(page,pageSize,Language);
				return Ok(new ApiResponseDto<GenericPaginationListDto<ComitteeSummaryReportDto>>(comittees));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}
		[HttpGet("comittees-meetings-summary/{committeeId}")]
		[RequiredPermission(PermissionDbEnum.ComitteesSummaryReport, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetCouncilsComitteesMeetingsCount( int committeeId)
		{
			try
			{
				var statusesMeetings = await _reportsManager.GetMeetingCountForStatuses(committeeId, Language);
				return Ok(new ApiResponseDto<BarChartDto>(statusesMeetings));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpPost("attendance/{page}/{pageSize}")]
		[RequiredPermission(PermissionDbEnum.AttendanceReport, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetAttendnaceReports(AttendanceReportSearchDto searchDto,int page, int pageSize)
		{
			try
			{
				var attendanceList = await _reportsManager.GetAttendnaceReport(searchDto,page, pageSize, Language);
				return Ok(new ApiResponseDto<GenericPaginationListDto<AttendanceReportDto>>(attendanceList));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

	}
}
