using MapsterMapper;
using MMS.BLL.Common.Helpers;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Dashboard;
using MMS.DTO.Meetings;
using MMS.DTO.Reports;
using System.Linq.Expressions;

namespace MMS.BLL.Managers
{
	public class ReportsManager
	{
		private readonly IMMSUnitOfWork _mmsUnitOfWork;
		private readonly IMapper _mapper;
		private readonly DictionaryManager _dictionaryManager;
        private readonly IFilterHelper _filterHelper;

        public ReportsManager(IMMSUnitOfWork mmsUnitOfWork, IMapper mapper, DictionaryManager dictionaryManager, IFilterHelper filterHelper)
		{
			_mmsUnitOfWork = mmsUnitOfWork;
			_mapper = mapper;
			_dictionaryManager = dictionaryManager;
			_filterHelper = filterHelper;
		}

		public async Task<GenericPaginationListDto<AttendanceReportDto>?> GetAttendnaceReport(AttendanceReportSearchDto searchDto, int page, int pageSize, LanguageDbEnum language)
		{
            Expression<Func<MeetingAttendee, bool>> filter = x =>  true;


            if (!string.IsNullOrEmpty(searchDto.MeetingReferenceNo))
            {
                Expression<Func<MeetingAttendee, bool>> MeetingRefCondition = x => x.Meeting.ReferenceNumber.Contains(searchDto.MeetingReferenceNo);
                filter = _filterHelper.Combine(filter, MeetingRefCondition);
            }
            if (!string.IsNullOrEmpty(searchDto.Title))
            {
                Expression<Func<MeetingAttendee, bool>> TitleCondition = x => x.Meeting.Title.Contains(searchDto.Title);
                filter = _filterHelper.Combine(filter, TitleCondition);
            }
            if (searchDto.FromDate != null)
            {
                Expression<Func<MeetingAttendee, bool>> FromDateCondition = x => x.Meeting.Date >= searchDto.FromDate;
                filter = _filterHelper.Combine(filter, FromDateCondition);
            }
            if (searchDto.ToDate != null)
            {
                Expression<Func<MeetingAttendee, bool>> ToDateCondition = x => x.Meeting.Date <= searchDto.ToDate;
                filter = _filterHelper.Combine(filter, ToDateCondition);
            }
            var attendees = await _mmsUnitOfWork.MeetingAttendees.ListIncludeUserAndMeetingAndComitteeAsync(filter,page,pageSize);
			var count = await _mmsUnitOfWork.MeetingAttendees.CountAsync(filter);
			var reportList=attendees.Select(x=>_mapper.Map<AttendanceReportDto>((x,language))).ToList();
			return new GenericPaginationListDto<AttendanceReportDto>() { Total = count, Data = reportList };
		}

		public async Task<GenericPaginationListDto<ComitteeSummaryReportDto>> GetComitteeSummaryReport(int Page, int PageSize, LanguageDbEnum language)
		{
			var comittees = await _mmsUnitOfWork.Committees.ListIncludeMembersAsync(
				page: Page,
				pageSize: PageSize);
			var count = await _mmsUnitOfWork.Committees.CountAsync();
			var reportList = comittees.Select(x => _mapper.Map<ComitteeSummaryReportDto>((x, language))).ToList();

			// PERFORMANCE FIX: Batch all counts in single queries instead of N+1
			var committeeIds = reportList.Select(x => x.Id).ToList();

			// Get all sub-committee counts in one query
			var subCommitteeCounts = await _mmsUnitOfWork.Committees.GetCountsByParentIdsAsync(committeeIds);

			// Get all recommendation counts in one query
			var recommendationCounts = await _mmsUnitOfWork.MeetingAgendaRecommendations.GetCountsByCommitteeIdsAsync(committeeIds);

			// Get all meeting counts in one query (excluding drafts)
			var meetingCounts = await _mmsUnitOfWork.Meetings.GetCountsByCommitteeIdsAsync(committeeIds, excludeStatus: (int)MeetingStatusDbEnum.Draft);

			// Get attendance stats in one query
			var attendanceStats = await _mmsUnitOfWork.MeetingAttendees.GetAttendanceStatsByCommitteeIdsAsync(committeeIds);

			// Map the results
			foreach (var item in reportList)
			{
				item.SubCommitteesCount = subCommitteeCounts.GetValueOrDefault(item.Id, 0);
				item.RecommendationsCount = recommendationCounts.GetValueOrDefault(item.Id, 0);
				item.MeetingsCount = meetingCounts.GetValueOrDefault(item.Id, 0);

				if (attendanceStats.TryGetValue(item.Id, out var stats) && stats.TotalAttendees > 0)
				{
					item.AttendanceRate = Math.Round((double)stats.AttendedCount / stats.TotalAttendees * 100.0, 2);
				}
				else
				{
					item.AttendanceRate = 0;
				}
			}

			return new GenericPaginationListDto<ComitteeSummaryReportDto>() { Total = count, Data = reportList };

		}
		public async Task<BarChartDto?> GetMeetingCountForStatuses(int CommitteeId,LanguageDbEnum language)
		{
			var chartData = new BarChartDto();
			var series = new BarChartSeries();
			series.Name = await _dictionaryManager.GetByKeyTranslated(DictionaryConstansts.Meetings, language);

			// PERFORMANCE FIX: Get all status counts in a single query instead of N queries
			var statusesToInclude = Enum.GetValues(typeof(MeetingStatusDbEnum))
				.Cast<MeetingStatusDbEnum>()
				.Where(s => s != MeetingStatusDbEnum.Draft)
				.Select(s => (int)s)
				.ToList();

			var statusCounts = await _mmsUnitOfWork.Meetings.GetCountsByStatusForCommitteeAsync(CommitteeId, statusesToInclude);

			foreach (var enumItem in Enum.GetValues(typeof(MeetingStatusDbEnum)))
			{
				if ((MeetingStatusDbEnum)enumItem == MeetingStatusDbEnum.Draft)
				{
					continue; // Skip Draft Meetings
				}
				string label = await _dictionaryManager.GetByKeyTranslated(Enum.GetName(typeof(MeetingStatusDbEnum), enumItem), language);
				var meetingsCount = statusCounts.GetValueOrDefault((int)enumItem, 0);
				chartData.Labels.Add(label);
				series.Data.Add(meetingsCount);
			}
			chartData.Series.Add(series);
			return chartData;
		}
	}
}
