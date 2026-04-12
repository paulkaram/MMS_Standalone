using MapsterMapper;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DTO.Dashboard;

namespace MMS.BLL.Managers
{
    public class DashboardManager
    {
        private readonly IMMSUnitOfWork _mmsUnitOfWork;
        private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;
        private readonly DictionaryManager _dictionaryManager;
        private readonly IMapper _mapper;
        public DashboardManager(IMapper mapper, IMMSUnitOfWork mmsUnitOfWork, DictionaryManager dictionaryManager, IUserManagementUnitOfWork userManagementUnitOfWork)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
            _dictionaryManager = dictionaryManager;
            _userManagementUnitOfWork = userManagementUnitOfWork;
        }

        public async Task<DougnutChartDto> GetCouncilsComitteesCount(string userId)
        {
            var chartData = new DougnutChartDto();
            //Chart Values
            var committeeCount = await _mmsUnitOfWork.UserCommittee.CountAsync(x => x.UserId == userId && x.Active && x.Committee.TypeId == (int)ComitteeTypeDbEnum.Comittee && x.Committee.Active);
            var councilsCount = await _mmsUnitOfWork.UserCommittee.CountAsync(x => x.UserId == userId && x.Active && x.Committee.TypeId == (int)ComitteeTypeDbEnum.Council && x.Committee.Active);
            chartData.Values.Add(councilsCount);
            chartData.Values.Add(committeeCount);
            return chartData;

        }

        public async Task<DougnutChartDto?> GetRecommendationsCount(string userId)
        {
            var chartData = new DougnutChartDto();
            var incompleteCount = await _mmsUnitOfWork.MeetingAgendaRecommendations.CountAsync(x => x.Owner == userId && x.StatusId == (int)MeetingAgendaRecommendationStatusDbEnum.InProgess);
            var completeCount = await _mmsUnitOfWork.MeetingAgendaRecommendations.CountAsync(x => x.Owner == userId && x.StatusId == (int)MeetingAgendaRecommendationStatusDbEnum.Completed);
            var total = incompleteCount + completeCount;
            decimal completePercentage = 0, inCompletePercentage = 0;

            if (total > 0)
            {
                completePercentage = Math.Round((decimal)completeCount / total * 100, 2);
                inCompletePercentage = Math.Round((decimal)incompleteCount / total * 100, 2);
            }

            chartData.Values.Add(completePercentage);
            chartData.Values.Add(inCompletePercentage);
            return chartData;
        }

        public async Task<DougnutChartDto?> GetTasksCounts(string userId)
        {
            var chartData = new DougnutChartDto();
            //Chart Values
            var incompleteCount = await _mmsUnitOfWork.Tasks.CountAsync(x => x.UserId == userId && x.StatusId == (int)TaskStatusDbEnum.PendingApproval);
            var completeCount = await _mmsUnitOfWork.Tasks.CountAsync(x => x.UserId == userId && x.StatusId != (int)TaskStatusDbEnum.PendingApproval);
            var total = incompleteCount + completeCount;
            if (total > 0)
            {
                var completePercentage = Math.Round((decimal)completeCount / total * 100, 2);
                var inCompletePercentage = Math.Round((decimal)incompleteCount / total * 100, 2);
                chartData.Values.Add(completePercentage);
                chartData.Values.Add(inCompletePercentage);
            }
            return chartData;
        }

        public async Task<DougnutChartDto?> GetUsersCounts()
        {
            var chartData = new DougnutChartDto();
            //Chart Values
            var activeCount = await _userManagementUnitOfWork.Users.CountAsync(x => x.Approved);
            var inactiveCount = await _userManagementUnitOfWork.Users.CountAsync(x => !x.Approved);
            chartData.Values.Add(activeCount);
            chartData.Values.Add(inactiveCount);
            return chartData;
        }
        public async Task<DougnutChartDto?> GetMeetingMinutesCounts(string userId)
        {
            var chartData = new DougnutChartDto();
            //Chart Values
            var initialMeetingsMinutesCount =
                await _mmsUnitOfWork.Meetings.CountAsync(x =>
                (x.Createdby == userId || x.MeetingAttendees.Any(x => x.UserId == userId)) &&
                 x.StatusId >= (int)MeetingStatusDbEnum.PendingInitialMeetingMinutesApproval);
            var finalMeetingsMinutesCount = 
                await _mmsUnitOfWork.Meetings.CountAsync(x => 
                (x.Createdby == userId || x.MeetingAttendees.Any(x => x.UserId == userId)) &&
                x.StatusId >= (int)MeetingStatusDbEnum.PendingFinalMeetingMinutesSign);

            chartData.Values.Add(initialMeetingsMinutesCount);
            chartData.Values.Add(finalMeetingsMinutesCount);
            return chartData;
        }
        public async Task<BarChartDto?> GetMeetingCountForStatuses(string userId, LanguageDbEnum language)
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

            var statusCounts = await _mmsUnitOfWork.Meetings.GetCountsByStatusForUserAsync(userId, statusesToInclude);

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

        // New Dashboard APIs

        public async Task<DashboardStatsDto> GetDashboardStats(string userId)
        {
            var stats = new DashboardStatsDto();

            // Meetings stats
            stats.TotalMeetings = await _mmsUnitOfWork.Meetings.CountAsync(x =>
                x.Createdby == userId || x.MeetingAttendees.Any(a => a.UserId == userId));

            stats.UpcomingMeetings = await _mmsUnitOfWork.Meetings.CountAsync(x =>
                (x.Createdby == userId || x.MeetingAttendees.Any(a => a.UserId == userId)) &&
                x.Date >= DateTime.Today &&
                x.StatusId != (int)MeetingStatusDbEnum.Canceled);

            stats.CompletedMeetings = await _mmsUnitOfWork.Meetings.CountAsync(x =>
                (x.Createdby == userId || x.MeetingAttendees.Any(a => a.UserId == userId)) &&
                x.StatusId == (int)MeetingStatusDbEnum.FinalMeetingMinutesSigned);

            // Tasks stats
            stats.PendingTasks = await _mmsUnitOfWork.Tasks.CountAsync(x =>
                x.UserId == userId &&
                x.StatusId == (int)TaskStatusDbEnum.PendingApproval);

            stats.CompletedTasks = await _mmsUnitOfWork.Tasks.CountAsync(x =>
                x.UserId == userId &&
                x.StatusId == (int)TaskStatusDbEnum.Approved);

            // Overdue tasks (tasks where CreatedDate + DueDate days < today)
            var today = DateTime.Today;
            stats.OverdueTask = await _mmsUnitOfWork.Tasks.CountAsync(x =>
                x.UserId == userId &&
                x.StatusId == (int)TaskStatusDbEnum.PendingApproval &&
                x.CreatedDate.AddDays(x.DueDate) < today);

            // Recommendations stats
            stats.TotalRecommendations = await _mmsUnitOfWork.MeetingAgendaRecommendations.CountAsync(x =>
                x.Owner == userId);

            stats.PendingRecommendations = await _mmsUnitOfWork.MeetingAgendaRecommendations.CountAsync(x =>
                x.Owner == userId &&
                x.StatusId == (int)MeetingAgendaRecommendationStatusDbEnum.InProgess);

            return stats;
        }

        public async Task<List<UpcomingMeetingDto>> GetUpcomingMeetings(string userId, int count = PaginationConstants.DefaultListCount)
        {
            var today = DateTime.Today;

            // PERFORMANCE FIX: Apply ordering at database level and fetch exact count needed with projection
            var meetings = await _mmsUnitOfWork.Meetings.GetUpcomingMeetingsAsync(
                userId,
                today,
                count,
                m => new UpcomingMeetingDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Date = m.Date,
                    Time = m.StartTime,
                    Location = m.Location,
                    CouncilName = m.Committee != null ? (m.Committee.NameAr ?? m.Committee.NameEn) : null,
                    Status = m.Status != null ? m.Status.NameEn : "Unknown"
                });

            return meetings ?? new List<UpcomingMeetingDto>();
        }

        public async Task<List<RecentActivityDto>> GetRecentActivities(string userId, int count = PaginationConstants.DefaultListCount)
        {
            var activities = new List<RecentActivityDto>();
            var halfCount = (count / 2) + 1; // Get slightly more to ensure we have enough after merging

            // PERFORMANCE FIX: Apply ordering and Take at database level, use projection
            // Get recent meetings (created or attended) - ordered and limited at DB level
            var meetingActivities = await _mmsUnitOfWork.Meetings.GetRecentActivitiesAsync(
                userId,
                halfCount,
                m => new RecentActivityDto
                {
                    Id = m.Id,
                    Type = "meeting",
                    Title = m.Title,
                    Description = m.Notes,
                    Timestamp = m.CreatedDate ?? DateTime.Now,
                    UserId = m.Createdby
                });

            activities.AddRange(meetingActivities);

            // PERFORMANCE FIX: Apply ordering and Take at database level, use projection
            // Get recent tasks - ordered and limited at DB level
            var taskActivities = await _mmsUnitOfWork.Tasks.GetRecentActivitiesAsync(
                userId,
                halfCount,
                t => new RecentActivityDto
                {
                    Id = t.Id,
                    Type = "task",
                    Title = t.Meeting != null ? t.Meeting.Title : "Task",
                    Timestamp = t.CreatedDate,
                    UserId = t.UserId
                });

            activities.AddRange(taskActivities);

            // Sort by timestamp and take top count (now only merging small sets)
            return activities
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToList();
        }

        public async Task<UserDashboardSummaryDto> GetUserDashboardSummary(string userId)
        {
            var summary = new UserDashboardSummaryDto();

            summary.MyMeetingsCount = await _mmsUnitOfWork.Meetings.CountAsync(x =>
                x.Createdby == userId || x.MeetingAttendees.Any(a => a.UserId == userId));

            summary.MyTasksCount = await _mmsUnitOfWork.Tasks.CountAsync(x => x.UserId == userId);

            summary.MyPendingTasksCount = await _mmsUnitOfWork.Tasks.CountAsync(x =>
                x.UserId == userId &&
                x.StatusId == (int)TaskStatusDbEnum.PendingApproval);

            return summary;
        }
    }
}
