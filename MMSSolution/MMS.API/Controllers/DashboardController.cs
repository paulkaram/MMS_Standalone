using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MMS.API.Common;
using MMS.API.Common.Attributes;
using MMS.BLL.Constants;
using MMS.BLL.Managers;
using MMS.DAL.Enumerations;
using MMS.DTO;
using MMS.DTO.Dashboard;

namespace MMS.API.Controllers
{
	[Route("api/dashboard")]
	[ApiController]
	public class DashboardController : IntalioBaseController
	{
		private readonly DashboardManager _dashboardManager;

		public DashboardController(DashboardManager dashboardManager)
		{
			_dashboardManager = dashboardManager;
		}

		/// <summary>
		/// Full MMS dashboard summary for the unified IAM landing page.
		/// No permission required — returns only the caller's own data.
		/// </summary>
		[HttpGet("my-summary")]
		[Authorize(AuthenticationSchemes = "JWT,OIDC")]
		public async Task<IActionResult> GetMySummary()
		{
			try
			{
				// Same pattern as Case Portal: NameIdentifier → sub → fallback to MMS claim
				var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
					?? User.FindFirst("sub")?.Value
					?? UserId;

				var stats = await _dashboardManager.GetDashboardStats(userId);
				var upcoming = await _dashboardManager.GetUpcomingMeetings(userId, 5);
				var activities = await _dashboardManager.GetRecentActivities(userId, 5);
				var recs = await _dashboardManager.GetRecommendationsCount(userId);

				var result = new ModuleDashboardSummaryDto
				{
					Stats = stats,
					UpcomingMeetings = upcoming,
					RecentActivities = activities,
					Recommendations = new RecommendationSummaryDto
					{
						Total = recs != null && recs.Values.Count >= 2
							? (int)(recs.Values[0] + recs.Values[1])
							: 0,
						Completed = recs != null && recs.Values.Count >= 1
							? (int)recs.Values[0]
							: 0,
						Pending = recs != null && recs.Values.Count >= 2
							? (int)recs.Values[1]
							: 0
					}
				};

				return Ok(new ApiResponseDto<ModuleDashboardSummaryDto>(result));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("councils-comittees-count")]
		[RequiredPermission(PermissionDbEnum.Dashboard, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetCouncilsComitteesCount()
		{
			try
			{
				var chartData=await _dashboardManager.GetCouncilsComitteesCount(UserId);
				return Ok(new ApiResponseDto<DougnutChartDto>(chartData));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("recommendations-count")]
		[RequiredPermission(PermissionDbEnum.Dashboard, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetRecommendationsCounts()
		{
			try
			{
				var chartData = await _dashboardManager.GetRecommendationsCount(UserId);
				return Ok(new ApiResponseDto<DougnutChartDto>(chartData));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("tasks-count")]
		[RequiredPermission(PermissionDbEnum.Dashboard, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetTasksCounts()
		{
			try
			{
				var chartData = await _dashboardManager.GetTasksCounts(UserId);
				return Ok(new ApiResponseDto<DougnutChartDto>(chartData));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("users-count")]
		[RequiredPermission(PermissionDbEnum.Dashboard, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetUsersCounts()
		{
			try
			{
				var chartData = await _dashboardManager.GetUsersCounts();
				return Ok(new ApiResponseDto<DougnutChartDto>(chartData));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("meeting-minutes-count")]
		[RequiredPermission(PermissionDbEnum.Dashboard, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetMeetingMinutesCounts()
		{
			try
			{
				var chartData = await _dashboardManager.GetMeetingMinutesCounts(UserId);
				return Ok(new ApiResponseDto<DougnutChartDto>(chartData));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("meetings-count")]
		[RequiredPermission(PermissionDbEnum.Dashboard, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetMeetingCountForStatuses()
		{
			try
			{
				var chartData = await _dashboardManager.GetMeetingCountForStatuses(UserId,Language);
				return Ok(new ApiResponseDto<BarChartDto>(chartData));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("stats")]
		[RequiredPermission(PermissionDbEnum.Dashboard, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetDashboardStats()
		{
			try
			{
				var stats = await _dashboardManager.GetDashboardStats(UserId);
				return Ok(new ApiResponseDto<DashboardStatsDto>(stats));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("upcoming-meetings")]
		[RequiredPermission(PermissionDbEnum.Dashboard, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetUpcomingMeetings([FromQuery] int count = PaginationConstants.DefaultListCount)
		{
			try
			{
				var meetings = await _dashboardManager.GetUpcomingMeetings(UserId, count);
				return Ok(new ApiResponseDto<List<UpcomingMeetingDto>>(meetings));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("recent-activities")]
		[RequiredPermission(PermissionDbEnum.Dashboard, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetRecentActivities([FromQuery] int count = PaginationConstants.DefaultListCount)
		{
			try
			{
				var activities = await _dashboardManager.GetRecentActivities(UserId, count);
				return Ok(new ApiResponseDto<List<RecentActivityDto>>(activities));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

		[HttpGet("user-summary")]
		[RequiredPermission(PermissionDbEnum.Dashboard, PermissionLevelDbEnum.Read)]
		public async Task<IActionResult> GetUserDashboardSummary()
		{
			try
			{
				var summary = await _dashboardManager.GetUserDashboardSummary(UserId);
				return Ok(new ApiResponseDto<UserDashboardSummaryDto>(summary));
			}
			catch (Exception ex)
			{
				return ErrorResponse(ex);
			}
		}

	}
}
