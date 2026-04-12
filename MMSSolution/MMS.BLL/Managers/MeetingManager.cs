using MMS.BLL.Storage;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Meetings;
using Intalio.Tools.Common.Extensions.FileExtensions;
using Intalio.Tools.Common.Storage;
using MMS.DTO;
using MMS.BLL.Constants;
using Microsoft.Extensions.Configuration;
using Task = System.Threading.Tasks.Task;
using Intalio.Tools.Common.Objects;
using Intalio.Tools.Common.FileKit;
using Path = System.IO.Path;
using System.Text;
using System.Linq.Expressions;
using MMS.BLL.Common.Helpers;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Runtime.CompilerServices;
using System.Globalization;
using Pipelines.Sockets.Unofficial.Arenas;
using Intalio.Tools.Common.Extensions.StringExtensions;
using MMS.DTO.Settings;
using Intalio.Tools.Common.Teams;



namespace MMS.BLL.Managers
{
    public class MeetingManager
    {
        // Well-known attendee index positions (order-based role assignment)
        private const int ChairmanIndex = 0;
        private const int SecretaryIndex = 1;

        // Attendee role constants
        private const string RoleChairman = "chairman";
        private const string RoleSecretary = "secretary";
        private const string RoleMember = "member";

        private readonly IMapper _mapper;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;
        private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;
        private readonly StorageManager _storageManager;
        private readonly StorageFactory _storageFactory;
        private readonly int _totalCountForAutoComplete;
        private readonly IProcessUnitOfWork _processUnitOfWork;
        private readonly IFilterHelper _filterHelper;
        private readonly EmailNotificationManager _emailNotificationManager;
        private readonly MomTemplateManager _momTemplateManager;
        private readonly TeamsService _teamsService;

        private readonly MmsNotificationService _notify;

        public MeetingManager(
            IConfiguration configuration,
            IMapper mapper,
            StorageManager storageManager,
            StorageFactory storageFactory,
            IMMSUnitOfWork mmsUnitOfWork,
            IUserManagementUnitOfWork userManagementUnitOfWork,
            IProcessUnitOfWork processUnitOfWork,
            EmailNotificationManager emailNotificationManager,
            IFilterHelper filterHelper,
            MomTemplateManager momTemplateManager,
            MmsNotificationService notify)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
            _storageManager = storageManager;
            _storageFactory = storageFactory;
            _totalCountForAutoComplete = configuration.GetValue<int>(AppSettingsConstants.TotalCountForAutoComplete);
            _userManagementUnitOfWork = userManagementUnitOfWork;
            _processUnitOfWork = processUnitOfWork;
            _filterHelper = filterHelper;
            _emailNotificationManager = emailNotificationManager;
            _momTemplateManager = momTemplateManager;
            _notify = notify;

            // Initialize Teams service
            var teamsSettings = configuration.GetSection(AppSettingsConstants.TeamsIntegrationSectionName).Get<TeamsIntegrationSettings>() ?? new();
            _teamsService = new TeamsService(teamsSettings);
        }

        public async Task<MeetingPostDto?> GetMeetingAsync(int meetingId, string userId, LanguageDbEnum language)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAndAssociatedAsync(x => x.Id == meetingId);
            if (meeting != null)
            {
                var meetingAgendas = await _mmsUnitOfWork.MeetingAgendas.ListIncludeTopicsAndVotingTypeAsync(x => x.MeetingId == meeting.Id);

                var meetingDto = _mapper.Map<MeetingPostDto>(meeting);
                if (meeting.IsCommittee.GetValueOrDefault() && meeting.CommitteeId != null && meeting.Committee != null)
                {
                    meetingDto.CommitteeName = language == LanguageDbEnum.Arabic ? meeting.Committee.NameAr : meeting.Committee.NameEn;
                    meetingDto.CommitteeTypeId = meeting.Committee.TypeId;
                }
                if (meeting.CouncilSession != null)
                {
                    meetingDto.CouncilSessionName = language == LanguageDbEnum.Arabic ? meeting.CouncilSession.NameAr : meeting.CouncilSession.NameEn;
                }
                meetingDto.ReadOnly = !(meeting.Createdby == userId && (meeting.StatusId != (int)MeetingStatusDbEnum.Draft || meeting.StatusId != (int)MeetingStatusDbEnum.PendingMeetingApproval));
                if (meetingAgendas != null)
                {
                    meetingDto.MeetingAgendas = meetingAgendas.Select(x => _mapper.Map<MeetingAgendaListItemDto>((x, language))).ToList();
                }
                if (meetingAgendas != null)
                {
                    meetingDto.MeetingAttendees = meeting.MeetingAttendees.Select(x => _mapper.Map<MeetingAttendeePostDto>((x, language))).ToList();
                }
                meetingDto.AssociatedMeetings = meeting.Associateds.Select(_mapper.Map<AssociatedMeetingDto>).ToList();

                return meetingDto;
            }
            return null;
        }

        public async Task<MeetingDto?> GetMeetingIncludeRecommendations(int meetingId, string userId, LanguageDbEnum language)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAgendasRecommendationsAsync(x => x.Id == meetingId);
            if (meeting != null)
            {
                var meetingDto = _mapper.Map<MeetingDto>(meeting);
                if (meeting.IsCommittee.GetValueOrDefault() && meeting.CommitteeId != null && meeting.Committee != null)
                {
                    meetingDto.CommitteeName = language == LanguageDbEnum.Arabic ? meeting.Committee.NameAr : meeting.Committee.NameEn;
                }
                /*if (meetingAgendas != null)
				{
					meetingDto.MeetingAgendas = meetingAgendas.Select(x => _mapper.Map<MeetingAgendaDto>((x, language))).ToList();
				}*/
                meetingDto.MeetingAttendees = meeting.MeetingAttendees.Select(x => _mapper.Map<MeetingAttendeePostDto>((x, language))).ToList();


                return meetingDto;
            }
            return null;
        }
        public async Task<LiveMeetingDto?> GetLiveMeetingDetails(int meetingId, string userId, LanguageDbEnum language)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAndAssociatedAsync(x => x.Id == meetingId);
            if (meeting != null)
            {
                var meetingAgendas = await _mmsUnitOfWork.MeetingAgendas.ListIncludeVotingAndDutyAndTopicsAsync(x => x.MeetingId == meeting.Id);
                List<int> agendaIds = meetingAgendas.Select(x => x.Id).ToList();


                var liveMeetingDto = _mapper.Map<LiveMeetingDto>(meeting);
                if (meeting.IsCommittee.GetValueOrDefault() && meeting.CommitteeId != null && meeting.Committee != null)
                {
                    liveMeetingDto.CommitteeName = language == LanguageDbEnum.Arabic ? meeting.Committee.NameAr : meeting.Committee.NameEn;
                }
                if (meeting.CouncilSession != null)
                {
                    liveMeetingDto.CouncilSessionName = language == LanguageDbEnum.Arabic ? meeting.CouncilSession.NameAr : meeting.CouncilSession.NameEn;
                }
                if (meeting.Status != null)
                {
                    liveMeetingDto.StatusName = language == LanguageDbEnum.Arabic ? meeting.Status.NameAr : meeting.Status.NameEn;
                }
                if (meetingAgendas != null)
                {
                    var meetingOwner = meeting.Createdby == userId;
                    bool startNext = false;
                    liveMeetingDto.MeetingAgendas = meetingAgendas.Select(x => _mapper.Map<LiveMeetingAgendaDto>((x, language))).ToList();
                    for (int i = 0; i < meetingAgendas.Count; i++)
                    {
                        var agenda = liveMeetingDto.MeetingAgendas[i];
                        if (agenda.ActualStartDate == null)
                        {//not started
                            if (startNext)
                            {
                                startNext = false;
                                await StartMeetingAgenda(agenda.Id.GetValueOrDefault());
                                agenda.IsRunning = true;
                            }
                            agenda.RemainingSeconds = agenda.Duration * 60;

                        }
                        else if (agenda.ActualEndDate == null) //started but not ended
                        {
                            var srartTime = agenda.ActualStartDate.GetValueOrDefault();
                            TimeSpan diff = DateTime.Now - srartTime;
                            if (agenda.Paused.GetValueOrDefault() && agenda.LastPausedDate != null)
                            {
                                DateTime lastPaused = agenda.LastPausedDate.GetValueOrDefault();
                                diff = lastPaused - srartTime;
                            }
                            int completedSeconds = (int)Math.Floor(diff.TotalSeconds);
                            if (agenda.PauseDuration.HasValue)
                            {
                                completedSeconds -= agenda.PauseDuration.Value;
                            }
                            agenda.RemainingSeconds = agenda.Duration * 60 - completedSeconds;
                            if (agenda.RemainingSeconds < 0)
                            {
                                startNext = true;
                                var completedAgenda = await EndMeetingAgenda(agenda.Id.GetValueOrDefault());
                                agenda.ActualEndDate = completedAgenda.ActualEndDate;
                                agenda.RemainingSeconds = 0;
                            }
                            else
                            {
                                agenda.IsRunning = true;
                            }
                        }
                        else //ended 
                        {
                            agenda.RemainingSeconds = 0;
                        }

                    }
                }

                liveMeetingDto.MeetingAttendees = meeting.MeetingAttendees.Select(x => _mapper.Map<MeetingAttendeePostDto>((x, language))).ToList();
                return liveMeetingDto;
            }
            return null;
        }

        private async Task<MeetingAgendum> StartMeetingAgenda(int id)
        {
            var agenda = await _mmsUnitOfWork.MeetingAgendas.Find(id);
            if (agenda != null)
            {
                agenda.ActualStartDate = DateTime.Now;
                if (agenda.Paused.GetValueOrDefault())
                {
                    agenda.Paused = false;
                }
                await _mmsUnitOfWork.SaveChangesAsync();
                return agenda;
            }
            return null;
        }
        private async Task<MeetingAgendum> EndMeetingAgenda(int id)
        {
            var agenda = await _mmsUnitOfWork.MeetingAgendas.Find(id);
            if (agenda != null)
            {
                agenda.ActualEndDate = DateTime.Now;
                await _mmsUnitOfWork.SaveChangesAsync();
                return agenda;
            }
            return null;
        }

        public async Task<GenericPaginationListDto<MeetingListItemDto>?> ListUserDraftMeetingsAsync(string userId, int page, int pageSize, LanguageDbEnum language)
        {
            var meetings = await _mmsUnitOfWork.Meetings.ListIncludeCommitteeAndStatusAsync(x => x.Createdby == userId && x.StatusId == (int)MeetingStatusDbEnum.Draft, page, pageSize);
            var count = await _mmsUnitOfWork.Meetings.CountAsync(x => x.Createdby == userId && x.StatusId == (int)MeetingStatusDbEnum.Draft);
            if (meetings != null)
            {
                var data = meetings.Select(x => _mapper.Map<MeetingListItemDto>((x, language))).OrderByDescending(x => x.Id).ToList();
                return new GenericPaginationListDto<MeetingListItemDto>(count, data);
            }
            return new GenericPaginationListDto<MeetingListItemDto>(0, new List<MeetingListItemDto>());
        }

        public async Task<GenericPaginationListDto<MeetingListItemDto>?> ListUsertMeetingsAsync(SearchMeetingDto searchMeetingDto, string userId, int page, int pageSize, LanguageDbEnum language)
        {
            Expression<Func<Meeting, bool>> filter = x => x.Createdby == userId || x.MeetingAttendees.Any(a => a.UserId == userId);
            if (!searchMeetingDto.IncludeDrafts)
            {
                Expression<Func<Meeting, bool>> draftCondition = x => x.StatusId != (int)MeetingStatusDbEnum.Draft;
                filter = _filterHelper.Combine(filter, draftCondition);
            }
            if (searchMeetingDto.StatusId != null)
            {
                Expression<Func<Meeting, bool>> statusCondition = x => x.StatusId == searchMeetingDto.StatusId;
                filter = _filterHelper.Combine(filter, statusCondition);
            }
            if (searchMeetingDto.MeetingId != null)
            {
                Expression<Func<Meeting, bool>> MeetingIdCondition = x => x.Id == searchMeetingDto.MeetingId;
                filter = _filterHelper.Combine(filter, MeetingIdCondition);
            }
            if (searchMeetingDto.Title != null)
            {
                Expression<Func<Meeting, bool>> TitleCondition = x => x.Title.Contains(searchMeetingDto.Title);
                filter = _filterHelper.Combine(filter, TitleCondition);
            }
            if (searchMeetingDto.Location != null)
            {
                Expression<Func<Meeting, bool>> LocationCondition = x => x.Location.Contains(searchMeetingDto.Location);
                filter = _filterHelper.Combine(filter, LocationCondition);
            }
            if (searchMeetingDto.NoComitteeRelated)
            {
                Expression<Func<Meeting, bool>> CommitteeIdCondition = x => x.CommitteeId == null;
                filter = _filterHelper.Combine(filter, CommitteeIdCondition);
            }
            else if (searchMeetingDto.CommitteeId != null)
            {
                Expression<Func<Meeting, bool>> CommitteeIdCondition = x => x.CommitteeId == searchMeetingDto.CommitteeId;
                filter = _filterHelper.Combine(filter, CommitteeIdCondition);
            }
            if (searchMeetingDto.FromDate != null)
            {
                Expression<Func<Meeting, bool>> FromDateCondition = x => x.Date >= searchMeetingDto.FromDate;
                filter = _filterHelper.Combine(filter, FromDateCondition);
            }
            if (searchMeetingDto.ToDate != null)
            {
                Expression<Func<Meeting, bool>> ToDateCondition = x => x.Date <= searchMeetingDto.ToDate;
                filter = _filterHelper.Combine(filter, ToDateCondition);
            }
            var meetings = await _mmsUnitOfWork.Meetings.ListIncludeCommitteeAndStatusAsync(filter, page, pageSize);
            var count = await _mmsUnitOfWork.Meetings.CountAsync(filter);
            if (meetings != null)
            {
                var data = meetings.Select(x => _mapper.Map<MeetingListItemDto>((x, language))).ToList();
                return new GenericPaginationListDto<MeetingListItemDto>(count, data);

            }
            return new GenericPaginationListDto<MeetingListItemDto>(0, new List<MeetingListItemDto>());
        }
        public async Task<List<CalenderMeetingDto>> ListUserMeetingsForCalenderAsync(string userId, SearchCalenderMeetingDto searchMeetingDto)
        {
            var meetings = await _mmsUnitOfWork.Meetings.ListAsync(x =>
                            x.StatusId != (int)MeetingStatusDbEnum.Draft &&
                            x.StatusId != (int)MeetingStatusDbEnum.Canceled &&
                             x.Date >= searchMeetingDto.StartDate &&
                             x.Date <= searchMeetingDto.EndDate &&
                             (x.Createdby == userId || x.MeetingAttendees.Any(x => x.UserId == userId)));
            if (meetings != null)
            {
                return meetings.Select(x => _mapper.Map<CalenderMeetingDto>(x)).ToList();
            }
            return new List<CalenderMeetingDto>();
        }
        public async Task<int> SaveMeetingAsync(MeetingPostDto meeting, IFormFileCollection files, string userId)
        {
            Meeting meetingNew = new Meeting
            {
                CommitteeId = meeting.CommitteeId,
                Createdby = userId,
                Date = meeting.Date ?? DateTime.Now,
                StartTime = meeting.StartTime ?? string.Empty,
                EndTime = meeting.EndTime ?? string.Empty,
                IsCommittee = meeting.IsCommittee,
                Location = meeting.Location,
                Notes = meeting.Notes,
                CreatedDate = DateTime.Now,
                Title = meeting.Title ?? string.Empty,
                MeetingTypeId = (int)MeetingTypeDbEnum.Table,
                ReferenceNumber = await GenerateReferenceNumber(meeting.CommitteeId),
                StatusId = (int)MeetingStatusDbEnum.Draft,
            };

            if (meeting.MeetingAttendees != null && meeting.MeetingAttendees.Any())
            {
                meeting.MeetingAttendees.ForEach(ma => meetingNew.MeetingAttendees.Add(new MeetingAttendee
                {
                    JobTitle = ma.JobTitle,
                    NeedsApproval = ma.NeedsApproval,
                    UserId = ma.UserId
                }));
            }

            if (meeting.MeetingAgendas != null && meeting.MeetingAgendas.Any())
            {
                meeting.MeetingAgendas.ForEach(ma => meetingNew.MeetingAgenda.Add(_mapper.Map<MeetingAgendum>((ma, userId))));
            }

            if (meeting.AssociatedMeetings != null && meeting.AssociatedMeetings.Any())
            {
                var associatedMeetings = await _mmsUnitOfWork.Meetings.ListAsync(x => meeting.AssociatedMeetings.Select(x => x.Id).Contains(x.Id));
                meetingNew.Associateds = associatedMeetings.ToList();
            }

            await _mmsUnitOfWork.Meetings.AddAsync(meetingNew);
            await _mmsUnitOfWork.SaveChangesAsync();

            if (files.Count > 0)
            {
                await this.AddAttachments(meetingNew.Id, files, userId);
            }
            return meetingNew.Id;
        }
        private async Task sendMeetingToAttendees(int? meetingId)
        {
            if (meetingId != null)
            {
                var attendees = await _mmsUnitOfWork.MeetingAttendees.ListAsync(x => x.MeetingId == meetingId);
                List<DAL.Models.MMS.Task> tasks = new List<DAL.Models.MMS.Task>();
                foreach (var attendee in attendees.Where(x => x.NeedsApproval == true))
                {
                    DAL.Models.MMS.Task task = new DAL.Models.MMS.Task
                    {
                        MeetingId = meetingId ?? default,
                        CreatedDate = DateTime.Now,
                        UserId = attendee.UserId,
                        DueDate = 3,
                        StatusId = (int)TaskStatusDbEnum.PendingApproval,
                        TypeId = (int)TaskTypeDbEnum.MeetingApproval
                    };
                    tasks.Add(task);
                }

                var meeting = await _mmsUnitOfWork.Meetings.GetAsync(x => x.Id == meetingId);
                if (meeting != null)
                {
                    meeting.StatusId = (int)MeetingStatusDbEnum.PendingMeetingApproval;
                }
                await _mmsUnitOfWork.Tasks.AddRangeAsync(tasks);
            }
        }
        public async Task SendMeetingAsync(int? meetingId)
        {
            await sendMeetingToAttendees(meetingId);
            await _mmsUnitOfWork.SaveChangesAsync();

            // Notify assigned task users
            if (meetingId.HasValue)
            {
                var meeting = await _mmsUnitOfWork.Meetings.GetAsync(x => x.Id == meetingId.Value);
                var tasks = await _mmsUnitOfWork.Tasks.ListAsync(x => x.MeetingId == meetingId.Value && x.StatusId == (int)TaskStatusDbEnum.PendingApproval);
                foreach (var t in tasks)
                {
                    _ = _notify.MeetingTaskAssigned(t.UserId, meeting?.Title ?? "", meeting?.Createdby ?? "");
                }
            }
        }

        public async Task ApproveMeetingAsync(int? meetingId)
        {
            await sendMeetingToAttendees(meetingId);
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAsync(x => x.Id == meetingId);
            if (meeting != null)
            {
                meeting.StatusId = (int)MeetingStatusDbEnum.Approved;
                var meetingTasks = await _mmsUnitOfWork.Tasks.ListAsync(x => x.MeetingId == meetingId);
                // Mark all approval tasks as approved (bypassing the approval workflow)
                foreach (var task in meetingTasks)
                {
                    task.StatusId = (int)TaskStatusDbEnum.Approved;
                    task.CompletedDate = DateTime.Now;
                }

                // Create Teams meeting if online meeting is enabled
                if (meeting.IsOnlineMeeting == true && _teamsService.IsEnabled)
                {
                    await CreateTeamsMeetingAsync(meeting);
                }

                await _mmsUnitOfWork.SaveChangesAsync();

                // Send Outlook calendar invite to attendees (with Teams link if created)
                _emailNotificationManager.SendMeetingCalendarInvite(meeting);
            }
        }

        /// <summary>
        /// Creates a Teams online meeting for the given meeting
        /// </summary>
        private async Task CreateTeamsMeetingAsync(Meeting meeting)
        {
            try
            {
                // Parse meeting times
                var meetingDate = meeting.Date.Date;
                var startTimeParts = meeting.StartTime?.Split(':') ?? new[] { "09", "00" };
                var endTimeParts = meeting.EndTime?.Split(':') ?? new[] { "10", "00" };

                var startTime = meetingDate.AddHours(int.Parse(startTimeParts[0])).AddMinutes(int.Parse(startTimeParts[1]));
                var endTime = meetingDate.AddHours(int.Parse(endTimeParts[0])).AddMinutes(int.Parse(endTimeParts[1]));

                // Get attendee emails
                var attendeeEmails = meeting.MeetingAttendees
                    .Where(a => a.User != null && !string.IsNullOrEmpty(a.User.Email))
                    .Select(a => a.User.Email)
                    .ToList();

                // Create Teams meeting
                var result = await _teamsService.CreateOnlineMeetingAsync(
                    meeting.Title ?? "Meeting",
                    startTime,
                    endTime,
                    attendeeEmails);

                if (result != null && result.Success)
                {
                    meeting.MeetingUrl = result.JoinUrl;
                    meeting.OnlineMeetingId = result.MeetingId;
                    meeting.OnlineMeetingPasscode = result.VideoTeleconferenceId;
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the approval
                Console.WriteLine($"Failed to create Teams meeting: {ex.Message}");
            }
        }

        public async Task DeleteMeetingAsync(int meetingId)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetAsync(x => x.Id == meetingId);
            if (meeting != null)
            {
                await this.RemoveMeetingAttendees(meeting.Id);
                _mmsUnitOfWork.Meetings.Remove(meeting);
                await _mmsUnitOfWork.SaveChangesAsync();
            }
        }


        #region private methods 
        private async Task<string> GenerateReferenceNumber(int? committeeId)
        {
            int counter = await _mmsUnitOfWork.Meetings.CountAsync();
            if (committeeId != null)
            {
                return $"MTG-{DateTime.Now.Year}-{committeeId}-{counter}";
            }
            return $"MTG-{DateTime.Now.Year}-{counter}";
        }

        private async Task RemoveMeetingAttendees(int? meetingId)
        {
            var attendees = await _mmsUnitOfWork.MeetingAttendees.ListAsync(x => x.MeetingId == meetingId);
            if (attendees != null)
            {
                _mmsUnitOfWork.MeetingAttendees.RemoveRange(attendees);
                await _mmsUnitOfWork.SaveChangesAsync();
            }
        }

        private async Task RemoveAssociatedMeetings(int? meetingId)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeAssociatedAsync(x => x.Id == meetingId);
            if (meeting != null)
            {
                meeting.Associateds.Clear();
                await _mmsUnitOfWork.SaveChangesAsync();
            }
        }

        private async Task RemoveAttachments(int? meetingId, List<AttachmentListItemDto>? existingAttachments)
        {
            var existingAttachmentIds = existingAttachments != null ? existingAttachments.Select(x => x.Id) : new List<int>();
            var attachmentsToDelete = await _mmsUnitOfWork.Attachments.ListAsync(x =>
            x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.Meeting &&
            !existingAttachmentIds.Contains(x.Id));
            if (attachmentsToDelete != null)
            {
                _mmsUnitOfWork.Attachments.RemoveRange(attachmentsToDelete);
                await _mmsUnitOfWork.SaveChangesAsync();
            }
        }


        private async Task RemoveAgendasAndRelatedTopicsAndAttachments(int? meetingId, List<AttachmentListItemDto>? existingAttachments)
        {
            var existingAttachmentIds = existingAttachments != null ? existingAttachments.Select(x => x.Id) : new List<int>();


            var existingAgendas = await _mmsUnitOfWork.MeetingAgendas.ListAsync(x => x.MeetingId == meetingId);
            if (existingAgendas != null)
            {

                var ids = existingAgendas.Select(x => x.Id).ToList();

                var attachmentsToDelete = await _mmsUnitOfWork.Attachments.ListAsync(x =>
                x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.MeetingAgenda &&
                 ids.Contains(x.RecordId) &&
                !existingAttachmentIds.Contains(x.Id));

                var agendaTopics = await _mmsUnitOfWork.AgendaTopics.ListAsync(x =>
                ids.Contains(x.MeetingAgendaId));

                if (attachmentsToDelete != null) _mmsUnitOfWork.Attachments.RemoveRange(attachmentsToDelete);
                if (agendaTopics != null) _mmsUnitOfWork.AgendaTopics.RemoveRange(agendaTopics);
                _mmsUnitOfWork.MeetingAgendas.RemoveRange(existingAgendas);

                await _mmsUnitOfWork.SaveChangesAsync();
            }
        }

        private async Task AddAttachments(int meetingId, IFormFileCollection files, string userId)
        {
            var attachmentsToAdd = new List<Attachment>();
            string meetingDirectory = StorageFactory.GetMeetingDirectory(meetingId);
            for (int i = 0; i < files.Count; i++)
            {
                string fileRelativeUrl = $"{meetingDirectory}{Guid.NewGuid()}";
                attachmentsToAdd.Add(new Attachment
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    FileName = files[i].FileName,
                    FileRelativeUrl = fileRelativeUrl,
                    FileSize = files[i].ToBytes().Length,
                    RecordId = meetingId,
                    RecordTypeId = (int)AttachmentRecordTypeDbEnum.Meeting,
                    Title = files[i].FileName,
                    Version = 1,
                });


            }
            await _mmsUnitOfWork.Attachments.AddRangeAsync(attachmentsToAdd);
            await _mmsUnitOfWork.SaveChangesAsync();
            for (int i = 0; i < attachmentsToAdd.Count; i++)
            {
                Attachment attachment = attachmentsToAdd[i];
                await _storageManager.SaveToStorage(files[i].ToBytes(), attachment.Id, attachment.FileRelativeUrl);

            }
        }
        #endregion

        public async Task<List<ListItemDto>> ListMeetingsAsync(string userId)
        {
            var meetings = await _mmsUnitOfWork.Meetings.ListAsync(x =>
                x.StatusId != (int)MeetingStatusDbEnum.Draft &&
                (x.MeetingAttendees.Any(x => x.UserId == userId) || x.Createdby == userId));
            return meetings.Select(_mapper.Map<ListItemDto>).ToList();
        }

        public async Task<List<AssociatedMeetingDto>> ListMeetingsForAutoCompleteAsync(string search, string userId)
        {
            var meetings = await _mmsUnitOfWork.Meetings.ListAsync(x =>
                x.StatusId != (int)MeetingStatusDbEnum.Draft &&
                (x.Createdby == userId || x.MeetingAttendees.Any(a => a.UserId == userId)) &&
                (x.Title.Contains(search) || x.ReferenceNumber.Contains(search) || x.Location.Contains(search) || x.Notes.Contains(search)));
            var retVal = meetings.Take(_totalCountForAutoComplete).ToList();
            return retVal.Select(x => _mapper.Map<AssociatedMeetingDto>(x)).ToList();
        }

        public async Task<bool> HasAccessToUpdate(string userId, int? meetingId)
        {
            return await _mmsUnitOfWork.Meetings.AnyAsync(x => x.Id == meetingId && x.Createdby == userId && x.StatusId == (int)MeetingStatusDbEnum.Draft || x.StatusId == (int)MeetingStatusDbEnum.PendingMeetingApproval);
        }
        public async Task<bool> IsMeetingOwner(string userId, int? meetingId)
        {
            return await _mmsUnitOfWork.Meetings.AnyAsync(x => x.Id == meetingId && x.Createdby == userId);
        }
        public async Task<List<MeetingAttendeeListItemDto>?> ListMeetingAttendeesAsync(int meetingId, LanguageDbEnum language)
        {
            var attendees = await _mmsUnitOfWork.MeetingAttendees.ListAsync(x => x.MeetingId == meetingId);
            var users = await _userManagementUnitOfWork.Users.ListAsync(x => attendees.Select(x => x.UserId).Contains(x.Id));
            var tasks = await _mmsUnitOfWork.Tasks.ListAsync(x => x.MeetingId == meetingId);
            return attendees.Select(x => new MeetingAttendeeListItemDto
            {
                UserId = x.UserId,
                Approved = x.NeedsApproval ? tasks.FirstOrDefault(x => x.UserId == x.UserId)?.StatusId == (int)TaskStatusDbEnum.Approved : false,
                FullName = language == LanguageDbEnum.Arabic ? users.FirstOrDefault(u => u.Id == x.UserId)?.FullnameAr : users.FirstOrDefault(u => u.Id == x.UserId)?.FullnameEn,
                JobTitle = x.JobTitle,
                NeedsApproval = x.NeedsApproval,
                Attended = x.Attended
            }).ToList();
        }

        public async Task<List<MeetingAgendaListItemDto>?> ListMeetingAgendasAsync(int meetingId, LanguageDbEnum language)
        {
            var meetingAgendas = await _mmsUnitOfWork.MeetingAgendas.ListIncludeVotingAndDutyAndTopicsAsync(x => x.MeetingId == meetingId);
            if (meetingAgendas == null) { return null; }
            return meetingAgendas.Select(x => _mapper.Map<MeetingAgendaListItemDto>((x, language))).ToList();
        }

        #region Live Meeting Methods
        public async Task<(bool success, List<LiveMeetingAgendaDto> agendaItems, MeetingStatusDbEnum? newStatus, int? newAgendaStartedId)> StartNextMeetingAgenda(int meetingId, LanguageDbEnum language)
        {
            var meetingAgendas = await _mmsUnitOfWork.MeetingAgendas.ListIncludeVotingAndDutyAndTopicsAsyncWithTrack(x => x.MeetingId == meetingId);
            if (meetingAgendas.Count() == 0)
            {
                return (false, new List<LiveMeetingAgendaDto>(), null, null);
            }
            else
            {
                MeetingStatusDbEnum? newStatus = null;
                bool firstAgenda = true;
                int? newAgendaStartedId = null;
                foreach (var agenda in meetingAgendas)
                {
                    if (agenda.ActualStartDate != null && agenda.ActualEndDate == null)//current running agenda
                    {
                        agenda.ActualEndDate = DateTime.Now;
                    }
                    else if (agenda.ActualStartDate == null && agenda.ActualEndDate == null)
                    {
                        agenda.ActualStartDate = DateTime.Now;
                        newAgendaStartedId = agenda.Id;
                        agenda.Paused = false;
                        if (firstAgenda)
                        {
                            var meeting = await _mmsUnitOfWork.Meetings.Find(meetingId);
                            meeting.StatusId = (int)MeetingStatusDbEnum.Started;
                            newStatus = MeetingStatusDbEnum.Started;
                        }
                        break;
                    }
                }
                if (newAgendaStartedId == null)//last agenda ended and no agenda started
                {
                    var meeting = await _mmsUnitOfWork.Meetings.Find(meetingId);
                    meeting.StatusId = (int)MeetingStatusDbEnum.Finished;
                    newStatus = MeetingStatusDbEnum.Finished;

                }
                await _mmsUnitOfWork.SaveChangesAsync();
                var agendas = meetingAgendas.Select(x => _mapper.Map<LiveMeetingAgendaDto>((x, language))).ToList();
                if (agendas != null)
                {
                    for (int i = 0; i < agendas.Count; i++)
                    {
                        if (agendas[i].ActualStartDate == null)//not started
                        {
                            agendas[i].RemainingSeconds = agendas[i].Duration * 60;

                        }
                        else if (agendas[i].ActualEndDate == null) //started but not ended
                        {
                            var srartTime = agendas[i].ActualStartDate.GetValueOrDefault();
                            TimeSpan diff = DateTime.Now - srartTime;
                            if (agendas[i].Paused.GetValueOrDefault() && agendas[i].LastPausedDate != null)
                            {
                                DateTime lastPaused = agendas[i].LastPausedDate.GetValueOrDefault();
                                diff = lastPaused - srartTime;
                            }
                            int completedSeconds = (int)Math.Floor(diff.TotalSeconds);
                            if (agendas[i].PauseDuration.HasValue)
                            {
                                completedSeconds -= agendas[i].PauseDuration.GetValueOrDefault();
                            }
                            agendas[i].RemainingSeconds = agendas[i].Duration * 60 - completedSeconds;
                            if (agendas[i].RemainingSeconds < 0)
                            {
                                var completedAgenda = await EndMeetingAgenda(agendas[i].Id.GetValueOrDefault());
                                agendas[i].ActualEndDate = completedAgenda.ActualEndDate;
                                agendas[i].RemainingSeconds = 0;
                            }
                            else
                            {
                                agendas[i].IsRunning = true;
                            }
                        }
                        else //ended 
                        {
                            agendas[i].RemainingSeconds = 0;
                        }

                    }
                }
                else
                {
                    agendas = new List<LiveMeetingAgendaDto>();
                }
                return (true, agendaItems: agendas, newStatus, newAgendaStartedId);
            }
        }
        public async Task<(bool success, List<LiveMeetingAgendaDto> agendaItems)> EndMeeting(int meetingId, LanguageDbEnum language)
        {
            var meetingAgendas = await _mmsUnitOfWork.MeetingAgendas.ListIncludeVotingAndDutyAndTopicsAsyncWithTrack(x => x.MeetingId == meetingId);
            if (meetingAgendas.Count() == 0)
            {
                return (false, new List<LiveMeetingAgendaDto>());
            }
            else
            {
                foreach (var agenda in meetingAgendas)
                {
                    if (agenda.ActualStartDate == null)
                    {
                        agenda.ActualStartDate = DateTime.Now;
                    }
                    if (agenda.ActualEndDate == null)
                    {
                        agenda.ActualEndDate = DateTime.Now;
                    }
                    agenda.Paused = false;

                }
                var meeting = await _mmsUnitOfWork.Meetings.Find(meetingId);
                meeting.StatusId = (int)MeetingStatusDbEnum.Finished;
                await _mmsUnitOfWork.SaveChangesAsync();
                var agendas = meetingAgendas.Select(x => _mapper.Map<LiveMeetingAgendaDto>((x, language))).ToList();
                for (int i = 0; i < agendas.Count; i++)
                {
                    agendas[i].RemainingSeconds = 0;
                }
                return (true, agendaItems: agendas);
            }
        }

        public async Task<(bool success, List<LiveMeetingAgendaDto> agendaItems)> PauseMeetingAgenda(int meetingId, LanguageDbEnum language)
        {
            var meetingAgendas = await _mmsUnitOfWork.MeetingAgendas.ListIncludeVotingAndDutyAndTopicsAsyncWithTrack(x => x.MeetingId == meetingId);
            if (meetingAgendas.Count() == 0)
            {
                return (false, new List<LiveMeetingAgendaDto>());
            }
            else
            {
                foreach (var agenda in meetingAgendas)
                {
                    if (agenda.ActualStartDate != null && agenda.ActualEndDate == null)//current running agenda
                    {
                        if (!agenda.Paused.GetValueOrDefault())
                        {
                            agenda.Paused = true;
                            agenda.LastPausedDate = DateTime.Now;
                        }
                        else
                        {
                            agenda.Paused = false;
                            agenda.PauseDuration = agenda.PauseDuration.GetValueOrDefault() + ((int)(DateTime.Now - agenda.LastPausedDate).GetValueOrDefault().TotalSeconds);

                        }

                    }
                }
                await _mmsUnitOfWork.SaveChangesAsync();
                var agendas = meetingAgendas.Select(x => _mapper.Map<LiveMeetingAgendaDto>((x, language))).ToList();
                await PrepareAgenda(agendas);
                return (true, agendaItems: agendas);
            }
        }

        private async Task PrepareAgenda(List<LiveMeetingAgendaDto> agendas)
        {
            bool startNext = false;
            for (int i = 0; i < agendas.Count; i++)
            {
                if (agendas[i].ActualStartDate == null)
                {//not started
                    if (startNext)
                    {
                        startNext = false;
                        await StartMeetingAgenda(agendas[i].Id.GetValueOrDefault());
                        agendas[i].IsRunning = true;
                    }
                    agendas[i].RemainingSeconds = agendas[i].Duration * 60;

                }
                else if (agendas[i].ActualEndDate == null) //started but not ended
                {
                    var srartTime = agendas[i].ActualStartDate.GetValueOrDefault();
                    TimeSpan diff = DateTime.Now - srartTime;
                    if (agendas[i].Paused.GetValueOrDefault() && agendas[i].LastPausedDate != null)
                    {
                        DateTime lastPaused = agendas[i].LastPausedDate.GetValueOrDefault();
                        diff = lastPaused - srartTime;
                    }
                    int completedSeconds = (int)Math.Floor(diff.TotalSeconds);
                    if (agendas[i].PauseDuration.HasValue)
                    {
                        completedSeconds -= agendas[i].PauseDuration.GetValueOrDefault();
                    }
                    agendas[i].RemainingSeconds = agendas[i].Duration * 60 - completedSeconds;
                    if (agendas[i].RemainingSeconds < 0)
                    {
                        startNext = true;
                        var completedAgenda = await EndMeetingAgenda(agendas[i].Id.GetValueOrDefault());
                        agendas[i].ActualEndDate = completedAgenda.ActualEndDate;
                        agendas[i].RemainingSeconds = 0;
                    }
                    else
                    {
                        agendas[i].IsRunning = true;
                    }
                }
                else //ended 
                {
                    agendas[i].RemainingSeconds = 0;
                }

            }
        }

        public async Task<bool> CancelMeeting(int meetingId)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAsync(x => x.Id == meetingId);
            if (meeting == null)
                return false;

            meeting.StatusId = (int)MeetingStatusDbEnum.Canceled;
            await _mmsUnitOfWork.SaveChangesAsync();

            // Send Outlook calendar cancellation to attendees
            _emailNotificationManager.SendMeetingCalendarCancellation(meeting, 1);

            return true;
        }

        public async Task<List<string>> ListMeetingAttendeesIds(int meetingId)
        {
            var attendees = await _mmsUnitOfWork.MeetingAttendees.ListAsync(x => x.MeetingId == meetingId);
            return attendees.Select(x => x.UserId).ToList();
        }



        public async Task<bool> SendMeetingMinutesTaskForApproval(MeetingMinutesTaskDto initialMeetingMinutesTaskDto, TaskTypeDbEnum taskType)
        {
            var attachment = await _mmsUnitOfWork.Attachments.GetLastAsync(x =>
            x.RecordId == initialMeetingMinutesTaskDto.MeetingId &&
            x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes);
            var runnungTasks = await _mmsUnitOfWork.Tasks.ListWithTrackAsync(x =>
                    x.MeetingId == initialMeetingMinutesTaskDto.MeetingId &&
                    x.TypeId == (int)TaskTypeDbEnum.InitialMeetingMinutesApproval &&
                    x.StatusId == (int)TaskStatusDbEnum.PendingApproval);
            foreach (var task in runnungTasks)
            {
                task.StatusId = (int)TaskStatusDbEnum.Cancelled;
            }
            await _mmsUnitOfWork.Tasks.AddRangeAsync(
                initialMeetingMinutesTaskDto.UsersIds.Select
                (x => new DAL.Models.MMS.Task()
                {
                    MeetingId = initialMeetingMinutesTaskDto.MeetingId,
                    UserId = x,
                    TypeId = (int)taskType,
                    CreatedDate = DateTime.Now,
                    StatusId = (int)TaskStatusDbEnum.PendingApproval,
                    DueDate = initialMeetingMinutesTaskDto.DueDate,
                    Claimed = false,
                    AttachmentId = attachment.Id
                })
                );
            await SendInitialMeetingMinutesNotification(initialMeetingMinutesTaskDto.MeetingId, initialMeetingMinutesTaskDto.UsersIds);
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }

        private async Task SendInitialMeetingMinutesNotification(int meetingId, List<string> usersIds)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeUsersAsync(x => x.Id == meetingId);
            if (meeting != null)
            {
                await _emailNotificationManager.InitialMeetingMinutesApprovalEmail(meeting, usersIds);
            }
        }

        public async Task<bool> SendFinalMeetingMinutesTaskForSign(MeetingMinutesTaskDto initialMeetingMinutesTaskDto)
        {
            var attachment = await _mmsUnitOfWork.Attachments.GetAsync(x =>
                    x.RecordId == initialMeetingMinutesTaskDto.MeetingId &&
                    x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes);
            if (attachment == null) return false;

            await _mmsUnitOfWork.Tasks.AddRangeAsync(
            initialMeetingMinutesTaskDto.UsersIds.Select
            (x => new DAL.Models.MMS.Task()
            {
                MeetingId = initialMeetingMinutesTaskDto.MeetingId,
                UserId = x,
                TypeId = (int)TaskTypeDbEnum.SignFinalMeetingMinutes,
                CreatedDate = DateTime.Now,
                StatusId = (int)TaskStatusDbEnum.PendingApproval,
                DueDate = initialMeetingMinutesTaskDto.DueDate,
                Claimed = false,
                AttachmentId = attachment.Id
            })
            );


            var tasksAdded = await _mmsUnitOfWork.SaveChangesAsync() > 0;
            if (tasksAdded)
            {
                var meeting = await _mmsUnitOfWork.Meetings.GetIncludeUsersAsync(x => x.Id == initialMeetingMinutesTaskDto.MeetingId);
                if (meeting != null)
                {
                    await _emailNotificationManager.FinalMeetingMinutesApprovalEmail(meeting, initialMeetingMinutesTaskDto.UsersIds);
                }
            }
            return tasksAdded;
        }
        public async Task<List<string>> GetInitialMeetingMinutesUsers(int MeetingId)
        {
            var attachment = await _mmsUnitOfWork.Attachments.GetLastAsync(x =>
                x.RecordId == MeetingId &&
                x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes);
            return (await _mmsUnitOfWork.Tasks.ListAsync(x => x.AttachmentId == attachment.Id && x.TypeId == (int)TaskTypeDbEnum.InitialMeetingMinutesApproval)).Select(x => x.UserId).ToList();

        }
        public async Task<List<string>> GetFinalMeetingMinutesUsers(int MeetingId)
        {
            return (await _mmsUnitOfWork.Tasks.ListAsync(x => x.MeetingId == MeetingId && x.TypeId == (int)TaskTypeDbEnum.SignFinalMeetingMinutes)).Select(x => x.UserId).ToList();

        }
        public async Task<AttachmentListItemDto> GetMeetingMinutesFile(int meetingId)
        {
            var attachment = await _processUnitOfWork.Attachments.GetLastAsync(x => x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes);

            if (attachment != null)
            {
                return _mapper.Map<AttachmentListItemDto>(attachment);
            }
            return null;

        }

        public async Task<(AttachmentListItemDto attachment, MeetingStatusDbEnum? newStatus)> GenerateMeetingMinutesFile(int meetingId, string userId, LanguageDbEnum language)
        {
            MeetingStatusDbEnum? newStatus = null;
            var attachment = await _processUnitOfWork.Attachments.GetLastAsync(x => x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes);

            int versionNo = 1;
            if (attachment != null)
            {
                versionNo = attachment.Version + 1;
            }
            var meeting = await GetMeetingIncludeRecommendations(meetingId, userId, language);
            var owner = meeting.MeetingAttendees.FirstOrDefault(x => x.UserId == meeting.Createdby)?.Name;
            var bytes = await _storageManager.GetInitialMeetingMinutesTemplate();
            if (bytes != null)
            {
                var bookMarks = PrepareBookmarksForMeetingMinutes(meeting, owner ?? "");
                bytes = FileWord.ApplyBookmarks(bytes, bookMarks);
            }

            string fileDirectory = StorageFactory.GetMeetingMinutesDirectory(meetingId);
            string fileRelativeUrl = $"{fileDirectory}{Guid.NewGuid()}";


            var newAttachment = new Attachment
            {
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                FileName = $"MeetingMinutes{meetingId}-V{versionNo}.docx",
                FileRelativeUrl = fileRelativeUrl,
                FileSize = bytes.Length,
                RecordId = meetingId,
                RecordTypeId = (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes,
                Title = Path.GetFileNameWithoutExtension($"MeetingMinutes-{meetingId}"),
                Version = versionNo,
            };
            if (versionNo == 1)
            {
                var meetingRecord = await _mmsUnitOfWork.Meetings.Find(meetingId);
                newStatus = MeetingStatusDbEnum.PendingInitialMeetingMinutesApproval;
                meetingRecord.StatusId = (int)newStatus;
            }

            await _processUnitOfWork.Attachments.AddAsync(newAttachment);
            await _processUnitOfWork.SaveChangesAsync();
            await _storageManager.SaveToStorage(bytes, newAttachment.Id, fileRelativeUrl);


            return (_mapper.Map<AttachmentListItemDto>(attachment), newStatus);



        }
        public async Task<(AttachmentListItemDto attachment, MeetingStatusDbEnum? newStatus)> UploadInitialMeetingMinutes(int meetingId, string userId, IFormFileCollection files, LanguageDbEnum language)
        {
            MeetingStatusDbEnum? newStatus = null;
            var meetingMinutesFile = files[0];
            var fileExtension = Path.GetExtension(meetingMinutesFile.FileName);
            var attachment = await _processUnitOfWork.Attachments.GetLastAsync(x => x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes);
            int versionNo = 1;
            if (attachment != null)
            {
                versionNo = attachment.Version + 1;
            }
            string fileDirectory = StorageFactory.GetMeetingMinutesDirectory(meetingId);
            string fileRelativeUrl = $"{fileDirectory}{Guid.NewGuid()}";

            var bytes = meetingMinutesFile.ToBytes();


            var newAttachment = new Attachment
            {
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                FileName = $"MeetingMinutes{meetingId}-V{versionNo}.{fileExtension}",
                FileRelativeUrl = fileRelativeUrl,
                FileSize = bytes.Length,
                RecordId = meetingId,
                RecordTypeId = (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes,
                Title = Path.GetFileNameWithoutExtension($"MeetingMinutes-{meetingId}-V{versionNo}"),
                Version = versionNo,
            };
            if (versionNo == 1)
            {
                var meetingRecord = await _mmsUnitOfWork.Meetings.Find(meetingId);
                newStatus = MeetingStatusDbEnum.PendingInitialMeetingMinutesApproval;
                meetingRecord.StatusId = (int)newStatus;
            }

            await _processUnitOfWork.Attachments.AddAsync(newAttachment);
            await _processUnitOfWork.SaveChangesAsync();
            await _storageManager.SaveToStorage(bytes, newAttachment.Id, fileRelativeUrl);


            return (_mapper.Map<AttachmentListItemDto>(attachment), newStatus);


        }

        public async Task<AttachmentListItemDto> ReGenerateMeetingMinutesFile(int meetingId, string userId, LanguageDbEnum language)
        {
            MeetingStatusDbEnum? newStatus = null;
            var attachment = await _processUnitOfWork.Attachments.GetLastAsync(x => x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes);
            var meeting = await GetMeetingIncludeRecommendations(meetingId, userId, language);
            var owner = meeting.MeetingAttendees.FirstOrDefault(x => x.UserId == meeting.Createdby)?.Name;
            byte[]? bytes = await _storageManager.GetInitialMeetingMinutesTemplate();
            if (bytes != null)
            {
                var bookMarks = PrepareBookmarksForMeetingMinutes(meeting, owner ?? "");
                bytes = FileWord.ApplyBookmarks(bytes, bookMarks);
            }
            string fileDirectory = StorageFactory.GetMeetingMinutesDirectory(meetingId);
            string fileRelativeUrl = $"{fileDirectory}{Guid.NewGuid()}";
            var newAttachment = new Attachment
            {
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                FileName = $"MeetingMinutes{meetingId}-V{attachment.Version + 1}.docx",
                FileRelativeUrl = fileRelativeUrl,
                FileSize = bytes.Length,
                RecordId = meetingId,
                RecordTypeId = (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes,
                Title = Path.GetFileNameWithoutExtension($"MeetingMinutes-{meetingId}-V{attachment.Version + 1}"),
                Version = attachment.Version + 1,
            };
            var meetingRecord = await _mmsUnitOfWork.Meetings.GetAsync(x => x.Id == meetingId);
            newStatus = MeetingStatusDbEnum.PendingInitialMeetingMinutesApproval;
            meetingRecord.StatusId = (int)newStatus;
            await _processUnitOfWork.Attachments.AddAsync(newAttachment);
            await _processUnitOfWork.SaveChangesAsync();
            await _storageManager.SaveToStorage(bytes, newAttachment.Id, fileRelativeUrl);

            return (_mapper.Map<AttachmentListItemDto>(newAttachment));

        }
        public async Task<AttachmentListItemDto> GetFinalMeetingMinutesFile(int meetingId)
        {
            var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes);
            if (attachment != null)
            {
                return _mapper.Map<AttachmentListItemDto>(attachment);

            }
            return null;
        }

        public async Task<(AttachmentListItemDto attachment, MeetingStatusDbEnum? newStatus)> GenerateFinalMeetingMinutesFile(int meetingId, string userId, LanguageDbEnum language)
        {
            MeetingStatusDbEnum? newStatus = null;

            // Get meeting with full data
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAndAssociatedAsync(x => x.Id == meetingId);
            if (meeting == null)
            {
                return (null, null);
            }

            // Get agendas with all related data (notes, recommendations, votes)
            var agendas = await _mmsUnitOfWork.MeetingAgendas.ListIncludeAllForMinutesAsync(x => x.MeetingId == meetingId);

            // Get meeting summary
            var meetingSummary = await GetMeetingSummaryAsync(meetingId);

            // Get user info for the generator
            var user = await _userManagementUnitOfWork.Users.Find(userId);

            // Build the MinutesDto
            var minutesDto = BuildMeetingMinutesDto(meeting, agendas, meetingSummary, user, language, false);

            // Get version number for Final MOM
            var existingAttachment = await _processUnitOfWork.Attachments.GetLastAsync(
                x => x.RecordId == meetingId &&
                x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes);

            int versionNo = existingAttachment != null ? existingAttachment.Version + 1 : 1;
            minutesDto.Version = versionNo;
            minutesDto.VersionLabel = $"{versionNo}.0";

            // Get Final MOM template for this meeting's branch
            var branchId = meeting.Committee?.BranchId;
            var template = await _momTemplateManager.GetDefaultTemplateAsync(branchId, MomTemplateTypeDbEnum.Final);

            // Generate PDF with template config and HTML template
            byte[] pdfBytes = MeetingMinutesPdfGenerator.GeneratePdf(minutesDto, template?.Config, template?.HtmlTemplate, true);

            // Save to storage
            string fileDirectory = StorageFactory.GetFinalMeetingMinutesDirectory(meetingId);
            string fileRelativeUrl = $"{fileDirectory}{Guid.NewGuid()}";
            string fileName = $"FinalMeetingMinutes-{meetingId}-V{versionNo}.pdf";

            var newAttachment = new Attachment
            {
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                FileName = fileName,
                FileRelativeUrl = fileRelativeUrl,
                FileSize = pdfBytes.Length,
                RecordId = meetingId,
                RecordTypeId = (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes,
                Title = $"Final Meeting Minutes - {meeting.Title}",
                Version = versionNo,
            };

            // Update meeting status if first version
            if (versionNo == 1)
            {
                var meetingRecord = await _mmsUnitOfWork.Meetings.Find(meetingId);
                newStatus = MeetingStatusDbEnum.PendingFinalMeetingMinutesSign;
                meetingRecord.StatusId = (int)newStatus;
                await _mmsUnitOfWork.SaveChangesAsync();
            }

            await _processUnitOfWork.Attachments.AddAsync(newAttachment);
            await _processUnitOfWork.SaveChangesAsync();
            await _storageManager.SaveToStorage(pdfBytes, newAttachment.Id, fileRelativeUrl);

            return (_mapper.Map<AttachmentListItemDto>(newAttachment), newStatus);
        }

        public async Task<(AttachmentListItemDto attachment, MeetingStatusDbEnum? newStatus)> UploadFinalMeetingMinutes(int meetingId, string userId, IFormFileCollection files, LanguageDbEnum language)
        {
            MeetingStatusDbEnum? newStatus = null;
            var meetingMinutesFile = files[0];
            var attachment = await _processUnitOfWork.Attachments.GetAsync(x => x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes);
            if (attachment == null)
            {
                var fileExtension = Path.GetExtension(meetingMinutesFile.FileName);
                string fileDirectory = StorageFactory.GetFinalMeetingMinutesDirectory(meetingId);
                string fileRelativeUrl = $"{fileDirectory}{Guid.NewGuid()}";

                var bytes = meetingMinutesFile.ToBytes();


                attachment = new Attachment
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    FileName = $"MeetingMinutes{meetingId}.{fileExtension}",
                    FileRelativeUrl = fileRelativeUrl,
                    FileSize = bytes.Length,
                    RecordId = meetingId,
                    RecordTypeId = (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes,
                    Title = $"FinalMeetingMinutes-{meetingId}",
                    Version = 1,
                };

                var meetingRecord = await _mmsUnitOfWork.Meetings.Find(meetingId);
                newStatus = MeetingStatusDbEnum.PendingFinalMeetingMinutesSign;
                meetingRecord.StatusId = (int)newStatus;

                await _processUnitOfWork.Attachments.AddAsync(attachment);
                await _processUnitOfWork.SaveChangesAsync();
                await _storageManager.SaveToStorage(bytes, attachment.Id, fileRelativeUrl);


            }
            return (_mapper.Map<AttachmentListItemDto>(attachment), newStatus);


        }

        private List<FileBookmark> PrepareBookmarksForMeetingMinutes(MeetingDto meeting, string userName)
        {
            var arabicCulture = new CultureInfo("ar-SA");
            arabicCulture.DateTimeFormat.Calendar = new GregorianCalendar();
            var bookmarks = new List<FileBookmark>
    {
        new("MeetingNo", " "+meeting.Id.ToString()+" "),
        new("MeetingSubject", meeting.Title),
        new("MeetingDate", meeting.Date.Value.ToString("d MMMM yyyy", arabicCulture )),
        new("MeetingTime", $"من: {meeting.StartTime.FormatTo12Hour()} - الي: {meeting.EndTime.FormatTo12Hour()}"),
        new("MeetingLocation", meeting.Location),
        new("MeetingAtendees", $@"
            <table style='border-collapse: collapse;color:black; width: 100%;margig:-5px; direction: rtl; text-align: center; font-size: 16px;'>
                <thead>
                    <tr>
                        <th style='border: 0.1px solid #C085C0; padding: 5px; '>الاسم</th>
                        <th style='border: 0.1px solid #C085C0; padding: 5px;'>المسمى الوظيفي</th>
                        <th style='border: 0.1px solid #C085C0; padding: 5px;'>حاضر</th>
                    </tr>
                </thead>
                <tbody>
                    {string.Join("", meeting.MeetingAttendees.Select(x => $@"
                        <tr>
                            <td style='border: 0.1px solid #C085C0;color:black; padding: 5px;'>{x.Name}</td>
                            <td style='border: 0.1px solid #C085C0;color:black; padding: 5px;'>{x.JobTitle}</td>
                            <td style='border: 0.1px solid #C085C0;color:black; padding: 5px; text-align: center;'>
                                {(x.Attended ? "&#10003;" : "&#10005;")}
                            </td>
                        </tr>
                    "))}
                </tbody>
            </table>"),
        new("MeetingRecomendation", $@"
            <table style='border-collapse:color:black; collapse; width: 100%; direction: rtl; text-align: center; font-size: 16px;'>
                <thead>
                    <tr>
                        <th style='border: 0.1px solid #C085C0;color:black;  padding: 5px;'>#</th>

                        <th style='border: 0.1px solid #C085C0;color:black; padding: 5px;'>التوصية</th>
                        <th style='border: 0.1px solid #C085C0;color:black; padding: 5px;'>المسؤول</th>
                        <th style='border: 0.1px solid #C085C0;color:black; padding: 5px;'>التاريخ </th>

                    </tr>
                </thead>
                <tbody>
                    {string.Join("", meeting.MeetingAgenda.SelectMany(x=>x.MeetingAgendaRecommendations).Select((recommendation,index) => $@"
                        <tr>
                            <td style='border: 0.1px solid #C085C0;color:black; padding: 5px;'>{index+1}</td>
                            <td style='border: 0.1px solid #C085C0;color:black; padding: 5px;'>{recommendation.Text}</td>
                            <td style='border: 0.1px solid #C085C0;color:black; padding: 5px;'>{meeting.MeetingAttendees.FirstOrDefault(x => x.UserId == recommendation.Owner)?.Name ?? "N/A"}</td>
                            <td style='border: 0.1px solid #C085C0;color:black; padding: 5px;'>{recommendation.DueDate?.ToString("d MMMM yyyy", arabicCulture ) ?? "N/A"}</td>

                        </tr>
                    "))}
                </tbody>
            </table>"),

        new("MeetingMinutesDate", DateTime.Now.ToString("d MMMM yyyy", arabicCulture )),
        new("MeetingMinutesCreater", userName),
       new("MeetingAgenda", $@"
      <table style='border-collapse: collapse; width: 100%; direction: rtl; text-align: center; font-size: 16px;'>
          <thead>
              <tr>
                  <th style='border: 0.1px solid #C085C0; padding: 5px; width: 85%;'>جدول الاعمال</th>
                  <th style='border: 0.1px solid #C085C0; padding: 5px; width: 15%;'>المدة</th>
              </tr>
          </thead>
          <tbody>
              {string.Join("", meeting.MeetingAgenda.Select(agenda => $@"
                  <tr>
                      <td style='border: 0.1px solid #C085C0; padding: 5px; width: 85%;'>{agenda.Title}</td>
                      <td style='border: 0.1px solid #C085C0; padding: 5px; width: 15%;'>{agenda.Duration.ToString() ?? "-"}</td>
                  </tr>
              ")) }
          </tbody>
      </table>")
    };
            return bookmarks;
        }

        private string GenerateAgendaHtmlForMeetingMinutes(MeetingDto meeting)
        {
            var sb = new StringBuilder();
            sb.Append("<div class='main-agendas'>");

            foreach (var agenda in meeting.MeetingAgenda)
            {
                // Agenda title
                sb.AppendFormat("<p>&nbsp;- {0}.</p>", agenda.Title);


            }

            sb.Append("</div>");
            sb.Append("</body></html>");
            return sb.ToString();
        }



        public async Task<bool> ApproveInitialMeetingMinutes(int meetingId)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetAsync(x => x.Id == meetingId);
            meeting.StatusId = (int)MeetingStatusDbEnum.InitialMeetingMinutesApproved;
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<List<MeetingAttendeePostDto>> UpdateUserAttend(UserAttendPutDto userAttendPutDto, LanguageDbEnum language)
        {
            var attendees = await _mmsUnitOfWork.MeetingAttendees.ListIncludeUserAsync(x => x.MeetingId == userAttendPutDto.MeetingId);
            foreach (var attendee in attendees)
            {
                if (attendee.UserId == userAttendPutDto.UserId)
                {
                    attendee.Attended = userAttendPutDto.Attended;
                    break;
                }
            }
            await _mmsUnitOfWork.SaveChangesAsync();
            return attendees.Select(x => _mapper.Map<MeetingAttendeePostDto>((x, language))).ToList();

        }

        public async Task<bool> ApproveFinalMeetingMinutes(int meetingId)
        {
            var signed = await CheckFinalMeetingSigned(meetingId);
            if (!signed)
            {
                return false;
            }
            var meeting = await _mmsUnitOfWork.Meetings.GetAsync(x => x.Id == meetingId);
            meeting.StatusId = (int)MeetingStatusDbEnum.FinalMeetingMinutesSigned;

            // Activate recommendations status (set to InProgress)
            bool updated = await _mmsUnitOfWork.MeetingAgendaRecommendations.ActivateRecommendationsStatus(meetingId);

            // Create tasks for recommendation owners
            await CreateRecommendationTasksAsync(meetingId, meeting);

            await _mmsUnitOfWork.SaveChangesAsync();
            return true;
        }

        private async System.Threading.Tasks.Task CreateRecommendationTasksAsync(int meetingId, Meeting meeting)
        {
            // Get all non-draft recommendations for this meeting
            var recommendations = await _mmsUnitOfWork.MeetingAgendaRecommendations.ListByMeetingIdAsync(meetingId);

            if (!recommendations.Any())
            {
                return;
            }

            // Group by owner to avoid duplicate tasks for the same user
            var ownerGroups = recommendations.GroupBy(r => r.Owner);

            foreach (var group in ownerGroups)
            {
                var ownerId = group.Key;

                // Check if task already exists for this user and meeting
                var existingTask = await _mmsUnitOfWork.Tasks.AnyAsync(t =>
                    t.UserId == ownerId &&
                    t.MeetingId == meetingId &&
                    t.TypeId == (int)TaskTypeDbEnum.RecommendationFollowUp);

                if (!existingTask)
                {
                    var task = new DAL.Models.MMS.Task
                    {
                        UserId = ownerId,
                        MeetingId = meetingId,
                        TypeId = (int)TaskTypeDbEnum.RecommendationFollowUp,
                        StatusId = (int)TaskStatusDbEnum.PendingApproval,
                        CreatedDate = DateTime.Now
                    };
                    await _mmsUnitOfWork.Tasks.AddAsync(task);

                    _ = _notify.RecommendationAssigned(ownerId, meeting?.Title ?? "", meeting?.Createdby ?? "");
                }
            }
        }
        private async Task<bool> CheckFinalMeetingSigned(int meetingId)
        {
            var attachment = await _processUnitOfWork.Attachments.GetAsyncIncludeSignatures(x => x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes);
            var signCount = attachment.AttachmentsSignatures.Count();
            var usersToSignCount = await _mmsUnitOfWork.Tasks.CountAsync(x => x.TypeId == (int)TaskTypeDbEnum.SignFinalMeetingMinutes && x.MeetingId == meetingId);
            return signCount >= usersToSignCount;
        }
        public async Task<List<AttachmentListItemDto>> GetInitialMeetingMinutesVersions(int meetingId)
        {
            var attachments = await _processUnitOfWork.Attachments.ListAsync(x => x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes);
            return attachments.Select(attachment => _mapper.Map<AttachmentListItemDto>(attachment)).ToList();
        }
        public async Task<List<AttachmentListItemDto>> GetFinalMeetingMinutesVersions(int meetingId)
        {
            var attachments = await _processUnitOfWork.Attachments.ListAsync(x => x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes);
            return attachments.Select(attachment => _mapper.Map<AttachmentListItemDto>(attachment)).ToList();
        }

        public async Task<List<AttachmentListItemDto>> listMeetingAttachmentsAsync(int meetingId)
        {
            var meetingAgendas = await _mmsUnitOfWork.MeetingAgendas.ListIncludeVotingAndDutyAndTopicsAsync(x => x.MeetingId == meetingId);
            List<int> agendaIds = meetingAgendas.Select(x => x.Id).ToList();
            var meetingAttachments = await _mmsUnitOfWork.Attachments.ListAsync(x =>
                (x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.Meeting) ||
                (agendaIds.Contains(x.RecordId) && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.MeetingAgenda));
            var retVal = meetingAttachments.Select(x => _mapper.Map<AttachmentListItemDto>(x)).ToList();
            return retVal;
        }
        #endregion

        #region  Add Meeting New Apis Part
        public async Task<MeetingInfoDto> AddMeetingInfo(MeetingInfoPostDto meeting, string UserId, LanguageDbEnum language)
        {
            if (meeting.CommitteeId.GetValueOrDefault() != 0)
            {
                var hasAccess = await _mmsUnitOfWork.CommitteePermissions.AnyAsync(x =>
                    x.CommitteeId == meeting.CommitteeId &&
                    x.UserId == UserId &&
                    x.PermissionId == (int)CommitteePermissionDbEnum.CommitteeAddMeeting);
                if (!hasAccess)
                {
                    return null;
                }
            }

            Meeting meetingNew = new Meeting
            {
                CommitteeId = meeting.CommitteeId.GetValueOrDefault() == 0 ? null : meeting.CommitteeId,
                CouncilSessionId = meeting.CommitteeId.GetValueOrDefault() == 0 ? null : meeting.CouncilSessionId,
                Createdby = UserId,
                Date = meeting.Date,
                StartTime = meeting.StartTime,
                EndTime = meeting.EndTime,
                IsCommittee = meeting.IsCommittee,
                Location = meeting.Location,
                Notes = meeting.Notes,
                CreatedDate = DateTime.Now,
                Title = meeting.Title,
                MeetingUrl = meeting.Url,
                MeetingTypeId = meeting.TypeId,
                ReferenceNumber = await GenerateReferenceNumber(meeting.CommitteeId),
                StatusId = (int)MeetingStatusDbEnum.Draft,
            };
            if (meeting.IsCommittee && meeting.CommitteeId.GetValueOrDefault() != 0)
            {
                var users = await _mmsUnitOfWork.UserCommittee.ListIncludeCommitteeRoleAsync(x => x.CommitteeId == meeting.CommitteeId && x.Active);
                foreach (var user in users)
                {
                    meetingNew.MeetingAttendees.Add(new MeetingAttendee()
                    {
                        UserId = user.UserId,
                        NeedsApproval = false,
                        JobTitle = language == LanguageDbEnum.English ? user.CommitteeRole.NameEn : user.CommitteeRole.NameAr,
                        Attended = false
                    });

                }
            }
            await _mmsUnitOfWork.Meetings.AddAsync(meetingNew);
            await _mmsUnitOfWork.SaveChangesAsync();
            var newMeetingModel = await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAsync(x => x.Id == meetingNew.Id);
            var meetingInfoDto = _mapper.Map<MeetingInfoDto>(newMeetingModel);
            for (int i = 0; i < meetingInfoDto.MeetingAttendees.Count; i++)
            {
                var user = newMeetingModel.MeetingAttendees.ElementAt(i).User;
                meetingInfoDto.MeetingAttendees[i].Name = language == LanguageDbEnum.Arabic ? user.FullnameAr : user.FullnameEn;
            }
            return meetingInfoDto;


        }

        public async Task<MeetingInfoDto> UpdateMeetingInfo(MeetingInfoPostDto meetingInfoPostDto, LanguageDbEnum language)
        {
            var existingMeeting = await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAsync(x => x.Id == meetingInfoPostDto.Id);
            if (meetingInfoPostDto.IsCommittee && meetingInfoPostDto.CommitteeId != existingMeeting.CommitteeId)
            {
                if (meetingInfoPostDto.CommitteeId.GetValueOrDefault() != 0)
                {
                    var hasAccess = await _mmsUnitOfWork.CommitteePermissions.AnyAsync(x =>
                        x.CommitteeId == meetingInfoPostDto.CommitteeId &&
                        x.UserId == existingMeeting.Createdby &&
                        x.PermissionId == (int)CommitteePermissionDbEnum.CommitteeAddMeeting);
                    if (!hasAccess)
                    {
                        return null;
                    }
                }
                var users = await _mmsUnitOfWork.UserCommittee.ListIncludeCommitteeRoleAsync(x => x.CommitteeId == meetingInfoPostDto.CommitteeId && x.Active);
                foreach (var user in users)
                {
                    if (!existingMeeting.MeetingAttendees.Any(x => x.UserId == user.UserId))
                    {
                        existingMeeting.MeetingAttendees.Add(new MeetingAttendee()
                        {
                            UserId = user.UserId,
                            NeedsApproval = false,
                            JobTitle = language == LanguageDbEnum.English ? user.CommitteeRole.NameEn : user.CommitteeRole.NameAr,
                            Attended = false
                        });
                    }

                }
            }
            if (existingMeeting != null)
            {

                existingMeeting.CommitteeId = meetingInfoPostDto.CommitteeId.GetValueOrDefault() == 0 ? null : meetingInfoPostDto.CommitteeId;
                existingMeeting.CouncilSessionId = meetingInfoPostDto.CommitteeId.GetValueOrDefault() == 0 ? null : meetingInfoPostDto.CouncilSessionId;
                existingMeeting.Date = meetingInfoPostDto.Date;
                existingMeeting.StartTime = meetingInfoPostDto.StartTime ?? string.Empty;
                existingMeeting.EndTime = meetingInfoPostDto.EndTime ?? string.Empty;
                existingMeeting.IsCommittee = meetingInfoPostDto.IsCommittee;
                existingMeeting.Notes = meetingInfoPostDto.Notes;
                existingMeeting.Location = meetingInfoPostDto.Location;
                existingMeeting.Title = meetingInfoPostDto.Title;
            }
            await _mmsUnitOfWork.SaveChangesAsync();

            // Send Outlook calendar update if meeting is already approved
            if (existingMeeting.StatusId == (int)MeetingStatusDbEnum.Approved)
            {
                _emailNotificationManager.SendMeetingCalendarUpdate(existingMeeting, 1);
            }

            // Use the already loaded entity instead of querying again
            var meetingInfoDto = _mapper.Map<MeetingInfoDto>(existingMeeting);
            for (int i = 0; i < meetingInfoDto.MeetingAttendees.Count; i++)
            {
                var user = existingMeeting.MeetingAttendees.ElementAt(i).User;
                meetingInfoDto.MeetingAttendees[i].Name = language == LanguageDbEnum.Arabic ? user.FullnameAr : user.FullnameEn;
            }
            return meetingInfoDto;

        }

        public async Task<MeetingInfoDto> GetMeetingInfo(int meetingId, LanguageDbEnum language)
        {
            var existingMeeting = await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAsync(x => x.Id == meetingId);
            return _mapper.Map<MeetingInfoDto>(existingMeeting);
        }

        /// <summary>
        /// Gets a meeting with its attendees for Outlook calendar integration
        /// </summary>
        public async Task<Meeting?> GetMeetingWithAttendeesAsync(int meetingId)
        {
            return await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAsync(x => x.Id == meetingId);
        }
        public async Task<List<MeetingAttendeePostDto>> AddMeetingAttendee(int MeetingId, MeetingAttendeePostDto attendee, LanguageDbEnum language)
        {
            var exist = await _mmsUnitOfWork.MeetingAttendees.AnyAsync(x => x.MeetingId == MeetingId && x.UserId == attendee.UserId);
            if (exist)
            {
                return null;
            }
            var Attendee = new MeetingAttendee()
            {
                MeetingId = MeetingId,
                JobTitle = attendee.JobTitle,
                UserId = attendee.UserId,
                NeedsApproval = attendee.NeedsApproval,
            };
            await _mmsUnitOfWork.MeetingAttendees.AddAsync(Attendee);
            await _mmsUnitOfWork.SaveChangesAsync();
            var attendees = await _mmsUnitOfWork.MeetingAttendees.ListIncludeUserAsync(x => x.MeetingId == MeetingId);
            return attendees.Select(x => _mapper.Map<MeetingAttendeePostDto>((x, language))).ToList();
        }

        public async Task<List<MeetingAttendeePostDto>> UpdateMeetingAttendee(int meetingId, MeetingAttendeePostDto attendee, LanguageDbEnum language)
        {
            var oldAttendee = await _mmsUnitOfWork.MeetingAttendees.GetAsync(x => x.MeetingId == meetingId && x.UserId == attendee.UserId);
            if (oldAttendee.JobTitle != attendee.JobTitle)
            {
                oldAttendee.JobTitle = attendee.JobTitle;
            }
            if (oldAttendee.NeedsApproval != attendee.NeedsApproval)
            {
                oldAttendee.NeedsApproval = attendee.NeedsApproval;
            }
            await _mmsUnitOfWork.SaveChangesAsync();
            var attendees = await _mmsUnitOfWork.MeetingAttendees.ListIncludeUserAsync(x => x.MeetingId == meetingId);
            return attendees.Select(x => _mapper.Map<MeetingAttendeePostDto>((x, language))).ToList();

        }
        public async Task<List<MeetingAttendeePostDto>> DeleteMeetingAttendee(int MeetingId, string userId, LanguageDbEnum language)
        {
            var attendee = await _mmsUnitOfWork.MeetingAttendees.GetAsync(x => x.MeetingId == MeetingId && x.UserId == userId);
            _mmsUnitOfWork.MeetingAttendees.Remove(attendee);
            await _mmsUnitOfWork.SaveChangesAsync();
            var attendees = await _mmsUnitOfWork.MeetingAttendees.ListIncludeUserAsync(x => x.MeetingId == MeetingId);
            return attendees.Select(x => _mapper.Map<MeetingAttendeePostDto>((x, language))).ToList();


        }

        public async Task<List<MeetingAgendaListItemDto>?> AddMeetingAgenda(MeetingAgendaPostDto meetingAgendaPostDto, int MeetingId, string userId, LanguageDbEnum language)
        {
            var newAgenda = _mapper.Map<MeetingAgendum>((meetingAgendaPostDto, userId));
            newAgenda.MeetingId = MeetingId;
            await _mmsUnitOfWork.MeetingAgendas.AddAsync(newAgenda);
            await _mmsUnitOfWork.SaveChangesAsync();
            var agendaList = await _mmsUnitOfWork.MeetingAgendas.ListIncludeTopicsAndVotingTypeAsync(x => x.MeetingId == MeetingId);

            return agendaList.Select(x => _mapper.Map<MeetingAgendaListItemDto>((x, language))).ToList();

        }

        public async Task<List<MeetingAgendaListItemDto>> DeleteMeetingAgenda(int agendaId, int MeetingId, string userId, LanguageDbEnum language)
        {
            var agenda = await _mmsUnitOfWork.MeetingAgendas.GetAsync(x => x.Id == agendaId);
            var topics = await _mmsUnitOfWork.AgendaTopics.ListWithTrackAsync(x => x.MeetingAgendaId == agendaId);
            _mmsUnitOfWork.AgendaTopics.RemoveRange(topics);
            _mmsUnitOfWork.MeetingAgendas.Remove(agenda);
            await _mmsUnitOfWork.SaveChangesAsync();
            var agendaList = await _mmsUnitOfWork.MeetingAgendas.ListIncludeTopicsAndVotingTypeAsync(x => x.MeetingId == MeetingId);

            return agendaList.Select(x => _mapper.Map<MeetingAgendaListItemDto>((x, language))).ToList();

        }

        public async Task<List<MeetingAgendaListItemDto>?> UpdateMeetingAgenda(MeetingAgendaPostDto meetingAgendaPostDto, int MeetingId, string userId, LanguageDbEnum language)
        {
            var agenda = await _mmsUnitOfWork.MeetingAgendas.GetAsync(x => x.Id == meetingAgendaPostDto.Id);
            agenda.Duration = meetingAgendaPostDto.Duration;
            agenda.Title = meetingAgendaPostDto.Title;
            agenda.VotingTypeId = meetingAgendaPostDto.VotingTypeId;
            agenda.AgendaTopics.Clear();
            foreach (var topic in meetingAgendaPostDto.AgendaTopics)
            {
                agenda.AgendaTopics.Add(new AgendaTopic() { Id = 0, Text = topic.Text ?? "" });
            }
            _mmsUnitOfWork.MeetingAgendas.Update(agenda);
            await _mmsUnitOfWork.SaveChangesAsync();
            var agendaList = await _mmsUnitOfWork.MeetingAgendas.ListIncludeTopicsAndVotingTypeAsync(x => x.MeetingId == MeetingId);

            return agendaList.Select(x => _mapper.Map<MeetingAgendaListItemDto>((x, language))).ToList();
        }

        public async Task<List<AttachmentListItemDto>?> DeleteMeetingAttachment(int attachmentId, int meetingId, LanguageDbEnum language)
        {
            var attachment = await _mmsUnitOfWork.Attachments.GetAsync(x => x.Id == attachmentId);
            _mmsUnitOfWork.Attachments.Remove(attachment);
            await _mmsUnitOfWork.SaveChangesAsync();

            var agendas = (await _mmsUnitOfWork.MeetingAgendas.ListAsync(x => x.MeetingId == meetingId));
            var agendaIds = agendas.Select(x => x.Id);
            var attachments = await _mmsUnitOfWork.Attachments.ListIncludePrivacyAndType(x =>
            x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.Meeting ||
            (agendaIds.Contains(x.RecordId) && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.MeetingAgenda));
            return attachments.Select(x => _mapper.Map<AttachmentListItemDto>((x, language))).ToList();
        }

        public async Task<List<AttachmentListItemDto>?> GetMeetingAttachments(int meetingId, string userId, LanguageDbEnum language)
        {
            var isMeetingOwner = await IsMeetingOwner(userId, meetingId);

            List<int> agendaIds = (await _mmsUnitOfWork.MeetingAgendas.ListAsync(x => x.MeetingId == meetingId)).Select(x => x.Id).ToList();
            var meetingAttachments = await _mmsUnitOfWork.Attachments.ListIncludePrivacyAndType(x =>
                (x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.Meeting) ||
                (agendaIds.Contains(x.RecordId) && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.MeetingAgenda));
            if (isMeetingOwner)
            {
                return meetingAttachments.Select(x => _mapper.Map<AttachmentListItemDto>((x, language))).ToList();
            }
            var committeeId = (await _mmsUnitOfWork.Meetings.Find(meetingId)).CommitteeId;
            var hasCommitteePermission = await _mmsUnitOfWork.CommitteePermissions.AnyAsync(x =>
                    x.UserId == userId &&
                    x.CommitteeId == committeeId &&
                    x.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetingAttachments);


            if (committeeId != null)
            {
                hasCommitteePermission = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(p => p.UserId == userId && p.PermissionId == (int)PermissionDbEnum.SuperAdmin);

                if (!hasCommitteePermission)
                {
                    var classificationId = (await _mmsUnitOfWork.Committees.Find(committeeId.GetValueOrDefault())).CommitteeClassificationId;
                    if (classificationId != null)
                    {
                        hasCommitteePermission = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(x => x.UserId == userId && x.Permission.MapId == classificationId);

                    }
                }

            }


            if (!hasCommitteePermission)
            {
                if (committeeId.GetValueOrDefault() > 0)
                {
                    var userCommittee = await _mmsUnitOfWork.UserCommittee.GetAsync(x => x.CommitteeId == committeeId && x.UserId == userId);
                    var userPrivacy = userCommittee?.PrivacyId;
                    userPrivacy ??= (short)PrivacyDbEnum.Normal;// attendees from outside the committee view only the normal leve
                    meetingAttachments = meetingAttachments.Where(x => x.PrivacyId <= userPrivacy).ToList();
                }

            }
            return meetingAttachments.Select(x => _mapper.Map<AttachmentListItemDto>((x, language))).ToList();
        }
        public async Task<List<AttachmentListItemDto>> GetLiveMeetingAttachments(int meetingId, string userId)
        {
            var committeeId = (await _mmsUnitOfWork.Meetings.Find(meetingId)).CommitteeId;
            List<int> agendaIds = (await _mmsUnitOfWork.MeetingAgendas.ListAsync(x => x.MeetingId == meetingId)).Select(x => x.Id).ToList();
            var meetingAttachments = await _mmsUnitOfWork.Attachments.ListIncludePrivacyAndType(x =>
                (x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.Meeting) ||
                (agendaIds.Contains(x.RecordId) && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.MeetingAgenda));
            if (committeeId.GetValueOrDefault() > 0)
            {
                var userPrivacy = (await _mmsUnitOfWork.UserCommittee.GetAsync(x => x.CommitteeId == committeeId && x.UserId == userId)).PrivacyId;
                meetingAttachments = meetingAttachments.Where(x => x.PrivacyId <= userPrivacy).ToList();
            }
            return meetingAttachments.Select(_mapper.Map<AttachmentListItemDto>).ToList();

        }
        public async Task<List<AttachmentListItemDto>?> UpdateMeetingAttachment(AttachmentPutDto attachmentPutDto, int meetingId, LanguageDbEnum language)
        {
            var attachment = await _mmsUnitOfWork.Attachments.Find(attachmentPutDto.id);

            attachment.PrivacyId = attachmentPutDto.PrivacyId;
            attachment.RecordId = attachmentPutDto.RecordId;
            attachment.RecordTypeId = attachmentPutDto.RecordTypeId;
            _mmsUnitOfWork.Attachments.Update(attachment);
            await _mmsUnitOfWork.SaveChangesAsync();

            var agendas = (await _mmsUnitOfWork.MeetingAgendas.ListAsync(x => x.MeetingId == meetingId));
            var agendaIds = agendas.Select(x => x.Id);
            var attachments = await _mmsUnitOfWork.Attachments.ListIncludePrivacyAndType(x =>
            x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.Meeting ||
            (agendaIds.Contains(x.RecordId) && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.MeetingAgenda));
            return attachments.Select(x => _mapper.Map<AttachmentListItemDto>((x, language))).ToList();
        }

        public async Task<List<AttachmentListItemDto>?> AddMeetingAttachments(int meetingId, IFormFileCollection files, string userId, short privacyId, int? agendaId, LanguageDbEnum language)
        {
            var attachmentsToAdd = new List<Attachment>();
            string meetingDirectory = StorageFactory.GetMeetingDirectory(meetingId);
            for (int i = 0; i < files.Count; i++)
            {
                string fileRelativeUrl = $"{meetingDirectory}{Guid.NewGuid()}";
                attachmentsToAdd.Add(new Attachment
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    FileName = files[i].FileName,
                    FileRelativeUrl = fileRelativeUrl,
                    FileSize = files[i].ToBytes().Length,
                    RecordId = agendaId == null ? meetingId : agendaId.Value,
                    RecordTypeId = agendaId == null ? (int)AttachmentRecordTypeDbEnum.Meeting : (int)AttachmentRecordTypeDbEnum.MeetingAgenda,
                    Title = files[i].FileName,
                    Version = 1,
                    PrivacyId = privacyId
                });

            }
            await _mmsUnitOfWork.Attachments.AddRangeAsync(attachmentsToAdd);
            await _mmsUnitOfWork.SaveChangesAsync();
            for (int i = 0; i < attachmentsToAdd.Count; i++)
            {
                Attachment attachment = attachmentsToAdd[i];
                await _storageManager.SaveToStorage(files[i].ToBytes(), attachment.Id, attachment.FileRelativeUrl);
            }
            var agendas = (await _mmsUnitOfWork.MeetingAgendas.ListAsync(x => x.MeetingId == meetingId));
            var agendaIds = agendas.Select(x => x.Id);
            var attachments = await _mmsUnitOfWork.Attachments.ListIncludePrivacyAndType(x =>
            x.RecordId == meetingId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.Meeting ||
            (agendaIds.Contains(x.RecordId) && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.MeetingAgenda));
            return attachments.Select(x => _mapper.Map<AttachmentListItemDto>((x, language))).ToList();
        }
        public async Task<List<AssociatedMeetingDto>> ListAssociatedMeeting(int meetingId)
        {
            var meetings = await _mmsUnitOfWork.Meetings.GetIncludeAssociatedAsync(x => x.Id == meetingId);
            return meetings.Associateds.Where(x => x.Id != meetingId).Select(x => _mapper.Map<AssociatedMeetingDto>(x)).ToList();

        }
        public async Task<List<AssociatedMeetingDto>?> DeleteAssociatedMeeting(int meetingId, int AssociatedId)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeAssociatedAsync(x => x.Id == meetingId);
            if (meeting.Associateds.Any(x => x.Id == AssociatedId))
            {
                meeting.Associateds.Remove(await _mmsUnitOfWork.Meetings.GetAsync(x => x.Id == AssociatedId));
                await _mmsUnitOfWork.SaveChangesAsync();
            }
            return meeting.Associateds.Select(x => _mapper.Map<AssociatedMeetingDto>(x)).ToList();
        }

        public async Task<List<AssociatedMeetingDto>?> AddAssociatedMeeting(int meetingId, int AssociatedId)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeAssociatedAsync(x => x.Id == meetingId);
            if (!meeting.Associateds.Any(x => x.Id == AssociatedId))
            {
                meeting.Associateds.Add(await _mmsUnitOfWork.Meetings.GetAsync(x => x.Id == AssociatedId));
                await _mmsUnitOfWork.SaveChangesAsync();

            }
            return meeting.Associateds.Select(x => _mapper.Map<AssociatedMeetingDto>(x)).ToList();
        }

        public async Task<List<MeetingUserApprovalDto>?> GetMeetingApprovals(int meetingId, LanguageDbEnum language)
        {
            var tasks = await _mmsUnitOfWork.Tasks.ListIncludeUserAndStatusAsync(x => x.MeetingId == meetingId && x.TypeId == (int)TaskTypeDbEnum.MeetingApproval);
            return tasks.Select(x => _mapper.Map<MeetingUserApprovalDto>((x, language))).ToList();
        }

        public async Task<List<MeetingUserApprovalDto>?> GetMeetingMinutesApprovals(int attachmentId, LanguageDbEnum language)
        {
            var tasks = await _mmsUnitOfWork.Tasks.ListIncludeUserAndStatusAsync(x => x.AttachmentId == attachmentId && x.TypeId == (int)TaskTypeDbEnum.InitialMeetingMinutesApproval);
            return tasks.Select(x => _mapper.Map<MeetingUserApprovalDto>((x, language))).ToList();
        }
        public async Task<List<MeetingUserApprovalDto>?> GetMeetingMinutesSignatures(int meetingId, LanguageDbEnum language)
        {
            var tasks = await _mmsUnitOfWork.Tasks.ListIncludeUserAndStatusAsync(x => x.MeetingId == meetingId && x.TypeId == (int)TaskTypeDbEnum.SignFinalMeetingMinutes);
            return tasks.Select(x => _mapper.Map<MeetingUserApprovalDto>((x, language))).ToList();
        }





        #endregion

        #region Meeting Summary
        public async Task<string> GetMeetingSummaryAsync(int meetingId)
        {

            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeSummaryAsync(x => x.Id == meetingId);
            if (meeting?.MeetingSummary != null)
                return meeting?.MeetingSummary.Text;
            else return string.Empty;
        }

        public async Task<string> GetMeetingNameAsync(int meetingId)
        {

            var meeting = await _mmsUnitOfWork.Meetings.GetAsync(x => x.Id == meetingId);
            if (meeting != null)
                return meeting?.Title;
            else return string.Empty;
        }

        public async Task SaveMeetingSummaryAsync(string userId, int meetingId, string summary)
        {

            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeSummaryAsync(x => x.Id == meetingId);
            if (meeting != null)
            {
                if (meeting?.MeetingSummary != null)
                {
                    meeting.MeetingSummary.Text = summary;
                    await _mmsUnitOfWork.SaveChangesAsync();
                }
                else
                {
                    meeting.MeetingSummary = new MeetingSummary()
                    {
                        Text = summary,
                        CreatedBy = userId,
                    };
                    await _mmsUnitOfWork.SaveChangesAsync();
                }
            }

        }
        #endregion
        public async Task<List<MeetingAgendaVotingReportDto>> ListMeetingAgendaVotingReportAsync(int meetingId, LanguageDbEnum language)
        {
            var meetingAgendas = await _mmsUnitOfWork.MeetingAgendas.ListAsync(x => x.MeetingId == meetingId && x.VotingType.Id != (int)VotingTypeDbEnum.WithoutVoting);
            var meetingAgendaIds = meetingAgendas?.Select(x => x.Id).ToList();
            List<MeetingAgendaVotingReportDto> retVal = new();

            foreach (var agendaId in meetingAgendaIds)
            {
                var votes = await _mmsUnitOfWork.MeetingUserVotes
                    .ListIncludeMeetingAgendaAndVotingOptionAsync(x => x.MeetingAgendaId == agendaId);

                var allVotingOptions = await _mmsUnitOfWork.VotingTypeOptions.ListAsync();

                var voteResults = votes
                    .GroupBy(mv => new { mv.MeetingAgendaId, mv.MeetingAgenda.Title, mv.MeetingAgenda.VotingTypeId })
                    .Select(g => new MeetingAgendaVotingReportDto
                    {
                        Id = g.Key.MeetingAgendaId,
                        Title = g.Key.Title,
                        Votes = allVotingOptions.Where(o => o.VotingTypeId == g.Key.VotingTypeId)
                            .Select(option => new
                            {
                                Name = language == LanguageDbEnum.Arabic ? option.NameAr : option.NameEn,
                                Count = g.Count(mv => mv.VottingOptionId == option.Id)
                            }).ToDictionary(
                                vote => vote.Name,
                                vote => vote.Count > 0 ? vote.Count : 0),
                        TotalVotes = g.Count(),
                    }).ToList();

                if (voteResults != null)
                {
                    retVal.AddRange(voteResults);
                }
            }
            return retVal;
        }

        public async Task<bool> CanViewMeetingMinutes(string userId, int meetingId)
        {
            var isMeetingOwner = await IsMeetingOwner(userId, meetingId);
            var isSuperAdmin = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(p => p.UserId == userId && p.PermissionId == (int)PermissionDbEnum.SuperAdmin);

            bool hasTask = false, hasCommitteePermission = false;
            if (!isMeetingOwner && !isSuperAdmin)
            {
                hasTask = await _mmsUnitOfWork.Tasks.AnyAsync(x =>
                    x.MeetingId == meetingId &&
                    userId == x.UserId &&
                    x.TypeId == (int)TaskTypeDbEnum.InitialMeetingMinutesApproval);
                if (!hasTask)
                {
                    hasCommitteePermission = await _mmsUnitOfWork.Meetings.AnyAsync(x => x.Id == meetingId && x.IsCommittee == true && x.Committee != null &&
                                    x.Committee.CommitteePermissions.Any(p => p.UserId == userId && p.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetingMinutes));
                }
            }
            return isMeetingOwner || isSuperAdmin || hasTask || hasCommitteePermission;
        }
        public async Task<bool> CanViewFinalwMeetingMinutes(string userId, int meetingId)
        {
            var isMeetingOwner = await IsMeetingOwner(userId, meetingId);
            var isSuperAdmin = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(p => p.UserId == userId && p.PermissionId == (int)PermissionDbEnum.SuperAdmin);//to enhance check is committee
            bool hasTask = false, hasCommitteePermission = false;
            if (!isMeetingOwner && !isSuperAdmin)
            {
                hasTask = await _mmsUnitOfWork.Tasks.AnyAsync(x =>
                    x.MeetingId == meetingId &&
                    userId == x.UserId &&
                    x.TypeId == (int)TaskTypeDbEnum.SignFinalMeetingMinutes);
                if (!hasTask)
                {
                    hasCommitteePermission = await _mmsUnitOfWork.Meetings.AnyAsync(x => x.Id == meetingId && x.IsCommittee == true && x.Committee != null &&
                                    x.Committee.CommitteePermissions.Any(p => p.UserId == userId && p.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetingMinutes));
                }
            }
            return isMeetingOwner || isSuperAdmin || hasTask || hasCommitteePermission;
        }

        public async Task<MeetingInvitationMailDto> GetMeetingInfoForInvitation(int meetingId)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAgendasRecommendationsAsync(x => x.Id == meetingId);
            var meetingDto = _mapper.Map<MeetingInvitationMailDto>(meeting);
            return meetingDto;
        }

        public async Task<List<MeetingAttendee>> GetMeetingInfoForInvitationAttendeesFullInfo(int meetingId)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeUsersAsync(x => x.Id == meetingId);
            return meeting.MeetingAttendees.ToList();
        }

        public async Task<bool> IsMeetingMember(string userId, int meetingId)
        {
            // Check if user is meeting owner
            var hasAccess = await IsMeetingOwner(userId, meetingId);
            if (hasAccess) return true;

            // Check if user is SuperAdmin
            var isSuperAdmin = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(p =>
                p.UserId == userId && p.PermissionId == (int)PermissionDbEnum.SuperAdmin);
            if (isSuperAdmin) return true;

            // Check if user is an attendee
            hasAccess = await _mmsUnitOfWork.MeetingAttendees.AnyAsync(x => x.MeetingId == meetingId && x.UserId == userId);
            if (hasAccess) return true;

            // Check committee permissions
            hasAccess = await _mmsUnitOfWork.Meetings.AnyAsync(x => x.Id == meetingId && x.IsCommittee == true && x.Committee != null &&
                                x.Committee.CommitteePermissions.Any(p => p.UserId == userId && p.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetings));
            if (hasAccess) return true;

            // Check committee classification admin
            var meeting = await _mmsUnitOfWork.Meetings.Find(meetingId);
            if (meeting?.CommitteeId != null)
            {
                var committee = await _mmsUnitOfWork.Committees.Find(meeting.CommitteeId.Value);
                if (committee?.CommitteeClassificationId != null)
                {
                    var isClassificationAdmin = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(x =>
                        x.UserId == userId && x.Permission.MapId == committee.CommitteeClassificationId);
                    if (isClassificationAdmin) return true;
                }
            }

            return false;
        }
        public async Task<bool> HasMeetingAttachmentAccess(string userId, int meetingId)
        {
            var hasAccess = await IsMeetingOwner(userId, meetingId);
            if (!hasAccess)
            {
                hasAccess = await _mmsUnitOfWork.MeetingAttendees.AnyAsync(x => x.MeetingId == meetingId && x.UserId == userId);
                if (hasAccess) return true;
                hasAccess = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(p => p.UserId == userId && p.PermissionId == (int)PermissionDbEnum.SuperAdmin);
                if (hasAccess) return true;

                var committeeId = (await _mmsUnitOfWork.Meetings.Find(meetingId)).CommitteeId;
                if (committeeId != null)
                {
                    var classificationId = (await _mmsUnitOfWork.Committees.Find(committeeId.GetValueOrDefault())).CommitteeClassificationId;
                    bool isCommittClassificationAdmin = false;
                    if (classificationId != null)
                    {
                        isCommittClassificationAdmin = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(x => x.UserId == userId && x.Permission.MapId == classificationId);
                        if (isCommittClassificationAdmin) return true;
                    }
                    return await _mmsUnitOfWork.Committees.AnyAsync(x => x.Id == committeeId &&
                    x.CommitteePermissions.Any(p => p.UserId == userId && p.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetingAttachments));

                }
            }
            return hasAccess;
        }
        public async Task<bool> HasMeetingAVotingResultsAccess(string userId, int meetingId)
        {
            var hasAccess = await IsMeetingOwner(userId, meetingId);
            if (!hasAccess)
            {
                return await _mmsUnitOfWork.Meetings.AnyAsync(x => x.Id == meetingId && x.IsCommittee == true && x.Committee != null &&
                                    x.Committee.CommitteePermissions.Any(p => p.UserId == userId && p.PermissionId == (int)CommitteePermissionDbEnum.VotingResults));
            }
            return true;
        }
        public async Task<bool> CanViewMeetingMinutesApprovals(string userId, int attachmentId)
        {
            var attachment = await _mmsUnitOfWork.Attachments.Find(attachmentId);

            if (attachment == null || attachment.RecordTypeId != (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes) return false;
            return await IsMeetingOwner(userId, attachment.RecordId);
        }

        #region PDF-based Minutes of Meeting Generation

        /// <summary>
        /// Generates a PDF-based Minutes of Meeting document.
        /// This method uses direct PDF generation (Aspose.PDF) instead of Word templates,
        /// providing better control over layout, RTL support, and consistent output.
        /// </summary>
        public async Task<GenerateMeetingMinutesResponseDto> GeneratePdfMeetingMinutes(
            GenerateMeetingMinutesRequestDto request,
            string userId,
            LanguageDbEnum language)
        {
            try
            {
                // Get meeting with full data
                var meeting = await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAndAssociatedAsync(x => x.Id == request.MeetingId);
                if (meeting == null)
                {
                    return new GenerateMeetingMinutesResponseDto { Success = false, Message = "Meeting not found" };
                }

                // Get agendas with all related data (notes, recommendations, votes)
                var agendas = await _mmsUnitOfWork.MeetingAgendas.ListIncludeAllForMinutesAsync(x => x.MeetingId == request.MeetingId);

                // Get meeting summary
                var meetingSummary = await GetMeetingSummaryAsync(request.MeetingId);

                // Get user info for the generator
                var user = await _userManagementUnitOfWork.Users.Find(userId);

                // Build the MinutesDto
                var minutesDto = BuildMeetingMinutesDto(meeting, agendas, meetingSummary, user, language, request.IncludePrivateNotes);

                // Get version number
                var existingAttachment = await _processUnitOfWork.Attachments.GetLastAsync(
                    x => x.RecordId == request.MeetingId &&
                    x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes);

                int versionNo = existingAttachment != null ? existingAttachment.Version + 1 : 1;
                minutesDto.Version = versionNo;
                minutesDto.VersionLabel = $"{versionNo}.0";

                // Get MOM template for this meeting's branch
                var branchId = meeting.Committee?.BranchId;
                var template = await _momTemplateManager.GetDefaultTemplateAsync(branchId, MomTemplateTypeDbEnum.Initial);

                // Generate PDF with template config and HTML template
                byte[] pdfBytes = MeetingMinutesPdfGenerator.GeneratePdf(minutesDto, template?.Config, template?.HtmlTemplate, request.IncludeVoterNames);

                // Save to storage
                string fileDirectory = StorageFactory.GetMeetingMinutesDirectory(request.MeetingId);
                string fileRelativeUrl = $"{fileDirectory}{Guid.NewGuid()}";
                string fileName = $"MeetingMinutes-{request.MeetingId}-V{versionNo}.pdf";

                var newAttachment = new Attachment
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    FileName = fileName,
                    FileRelativeUrl = fileRelativeUrl,
                    FileSize = pdfBytes.Length,
                    RecordId = request.MeetingId,
                    RecordTypeId = (int)AttachmentRecordTypeDbEnum.InitialMeetingMinutes,
                    Title = $"Meeting Minutes - {meeting.Title}",
                    Version = versionNo,
                };

                // Update meeting status if first version
                MeetingStatusDbEnum? newStatus = null;
                if (versionNo == 1)
                {
                    var meetingRecord = await _mmsUnitOfWork.Meetings.Find(request.MeetingId);
                    newStatus = MeetingStatusDbEnum.PendingInitialMeetingMinutesApproval;
                    meetingRecord.StatusId = (int)newStatus;
                    await _mmsUnitOfWork.SaveChangesAsync();
                }

                await _processUnitOfWork.Attachments.AddAsync(newAttachment);
                await _processUnitOfWork.SaveChangesAsync();
                await _storageManager.SaveToStorage(pdfBytes, newAttachment.Id, fileRelativeUrl);

                return new GenerateMeetingMinutesResponseDto
                {
                    Success = true,
                    AttachmentId = newAttachment.Id,
                    FileName = fileName,
                    Version = versionNo,
                    Message = "Meeting minutes generated successfully",
                    NewMeetingStatusId = newStatus.HasValue ? (int)newStatus.Value : null
                };
            }
            catch (Exception)
            {
                return new GenerateMeetingMinutesResponseDto
                {
                    Success = false,
                    Message = "Failed to generate meeting minutes"
                };
            }
        }

        /// <summary>
        /// Builds the MeetingMinutesDto from database entities
        /// </summary>
        private MeetingMinutesDto BuildMeetingMinutesDto(
            Meeting meeting,
            List<MeetingAgendum>? agendas,
            string? meetingSummary,
            User? generatedByUser,
            LanguageDbEnum language,
            bool includePrivateNotes)
        {
            var arabicCulture = new CultureInfo("ar-SA");
            arabicCulture.DateTimeFormat.Calendar = new GregorianCalendar();

            var dto = new MeetingMinutesDto
            {
                MeetingId = meeting.Id,
                MeetingNumber = meeting.Id.ToString(),
                ReferenceNumber = meeting.ReferenceNumber ?? "",
                Title = meeting.Title,

                CommitteeName = meeting.Committee != null
                    ? (language == LanguageDbEnum.Arabic ? meeting.Committee.NameAr : meeting.Committee.NameEn)
                    : null,
                CouncilSessionName = meeting.CouncilSession != null
                    ? (language == LanguageDbEnum.Arabic ? meeting.CouncilSession.NameAr : meeting.CouncilSession.NameEn)
                    : null,

                Date = meeting.Date,
                StartTime = meeting.StartTime,
                EndTime = meeting.EndTime,

                Location = meeting.Location,
                MeetingUrl = meeting.MeetingUrl,

                MeetingSummary = meetingSummary,

                GeneratedAt = DateTime.Now,
                GeneratedBy = language == LanguageDbEnum.Arabic
                    ? (generatedByUser?.FullnameAr ?? generatedByUser?.FullnameEn ?? "النظام")
                    : (generatedByUser?.FullnameEn ?? generatedByUser?.FullnameAr ?? "System"),
                Language = language == LanguageDbEnum.Arabic ? "ar" : "en"
            };

            // Calculate duration
            if (TimeSpan.TryParse(meeting.StartTime, out var startTime) && TimeSpan.TryParse(meeting.EndTime, out var endTime))
            {
                var duration = endTime - startTime;
                int hours = (int)duration.TotalHours;
                int minutes = duration.Minutes;
                dto.ActualDuration = language == LanguageDbEnum.Arabic
                    ? (hours > 0 ? $"{hours} ساعة {minutes} دقيقة" : $"{minutes} دقيقة")
                    : (hours > 0 ? $"{hours}h {minutes}m" : $"{minutes}m");
            }

            // Map attendees
            dto.Attendees = meeting.MeetingAttendees.Select((att, index) => new MinutesAttendeeDto
            {
                Id = att.Id,
                UserId = att.UserId,
                Name = language == LanguageDbEnum.Arabic
                    ? (att.User?.FullnameAr ?? att.User?.FullnameEn ?? "N/A")
                    : (att.User?.FullnameEn ?? att.User?.FullnameAr ?? "N/A"),
                JobTitle = att.JobTitle,
                Role = index == ChairmanIndex ? RoleChairman : (index == SecretaryIndex ? RoleSecretary : RoleMember),
                Attended = att.Attended,
                AttendedAt = null // MeetingAttendee doesn't track attendance time
            }).ToList();

            dto.TotalAttendees = dto.Attendees.Count;
            dto.PresentCount = dto.Attendees.Count(a => a.Attended);
            dto.AbsentCount = dto.TotalAttendees - dto.PresentCount;
            dto.QuorumMet = dto.PresentCount >= Math.Ceiling(dto.TotalAttendees / 2.0);

            // Set chairman and secretary names and titles
            var chairman = dto.Attendees.FirstOrDefault(a => a.Role == RoleChairman);
            var secretary = dto.Attendees.FirstOrDefault(a => a.Role == RoleSecretary);
            dto.ChairmanName = chairman?.Name;
            dto.SecretaryName = secretary?.Name;
            dto.ChairmanTitle = language == LanguageDbEnum.Arabic ? "رئيس الاجتماع" : "Chairman";
            dto.SecretaryTitle = language == LanguageDbEnum.Arabic ? "أمين السر" : "Secretary";

            // Map agenda items
            if (agendas != null)
            {
                dto.AgendaItems = agendas.Select((agenda, index) => BuildAgendaItemDto(
                    agenda, index + 1, dto.Attendees, language, includePrivateNotes)).ToList();
            }

            return dto;
        }

        /// <summary>
        /// Builds a single agenda item DTO with notes, voting results, and recommendations
        /// </summary>
        private MinutesAgendaItemDto BuildAgendaItemDto(
            MeetingAgendum agenda,
            int index,
            List<MinutesAttendeeDto> attendees,
            LanguageDbEnum language,
            bool includePrivateNotes)
        {
            var arabicCulture = new CultureInfo("ar-SA");
            arabicCulture.DateTimeFormat.Calendar = new GregorianCalendar();

            var item = new MinutesAgendaItemDto
            {
                Index = index,
                Id = agenda.Id,
                Title = agenda.Title,
                Description = agenda.Note,
                PlannedDuration = agenda.Duration,
                StartedAt = agenda.ActualStartDate,
                EndedAt = agenda.ActualEndDate
            };

            // Calculate actual duration
            if (agenda.ActualStartDate.HasValue && agenda.ActualEndDate.HasValue)
            {
                var diff = agenda.ActualEndDate.Value - agenda.ActualStartDate.Value;
                if (agenda.PauseDuration.HasValue)
                {
                    diff = diff.Subtract(TimeSpan.FromSeconds(agenda.PauseDuration.Value));
                }
                item.ActualDuration = (int)diff.TotalMinutes;
            }

            // Get summary from MeetingAgendaSummaries
            var latestSummary = agenda.MeetingAgendaSummaries?.OrderByDescending(s => s.Id).FirstOrDefault();
            item.Summary = latestSummary?.Text;

            // Map notes (filter by public/private based on request)
            item.DiscussionNotes = agenda.MeetingAgendaNotes?
                .Where(n => includePrivateNotes || n.IsPublic)
                .Select(n => new MinutesNoteDto
                {
                    Id = n.Id,
                    Text = n.Text ?? "",
                    IsPublic = n.IsPublic,
                    AuthorName = language == LanguageDbEnum.Arabic
                        ? (n.User?.FullnameAr ?? n.User?.FullnameEn ?? "N/A")
                        : (n.User?.FullnameEn ?? n.User?.FullnameAr ?? "N/A"),
                    CreatedAt = n.CreatedAt
                }).ToList() ?? new List<MinutesNoteDto>();

            // Map voting results
            var votingOptions = agenda.VotingType?.VotingOptions?.ToList();
            var votes = agenda.MeetingUserVotes?.ToList();

            // Check if this agenda has voting (VotingTypeId != WithoutVoting means it has voting)
            item.HasVoting = agenda.VotingTypeId != (int)VotingTypeDbEnum.WithoutVoting && votingOptions != null && votingOptions.Any();

            if (item.HasVoting && votingOptions != null && votes != null)
            {
                item.VotingResults = new MinutesVotingResultsDto
                {
                    VotingType = language == LanguageDbEnum.Arabic
                        ? agenda.VotingType?.NameAr
                        : agenda.VotingType?.NameEn,
                    TotalVoters = votes.Count,
                    Options = votingOptions.Select(opt =>
                    {
                        var optionVotes = votes.Where(v => v.VottingOptionId == opt.Id).ToList();
                        return new MinutesVotingOptionDto
                        {
                            Id = opt.Id,
                            Name = opt.NameEn ?? opt.NameAr ?? "",
                            NameAr = opt.NameAr ?? opt.NameEn ?? "",
                            VoteCount = optionVotes.Count,
                            Percentage = votes.Count > 0
                                ? (int)Math.Round((double)optionVotes.Count / votes.Count * 100)
                                : 0,
                            Voters = optionVotes.Select(v => language == LanguageDbEnum.Arabic
                                ? (v.User?.FullnameAr ?? v.User?.FullnameEn ?? "N/A")
                                : (v.User?.FullnameEn ?? v.User?.FullnameAr ?? "N/A")).ToList()
                        };
                    }).ToList()
                };

                // Determine outcome (option with most votes)
                var winningOption = item.VotingResults.Options
                    .OrderByDescending(o => o.VoteCount)
                    .FirstOrDefault();
                item.VotingResults.Outcome = language == LanguageDbEnum.Arabic
                    ? (winningOption?.NameAr ?? winningOption?.Name ?? "N/A")
                    : (winningOption?.Name ?? winningOption?.NameAr ?? "N/A");
            }

            // Map recommendations
            item.Recommendations = agenda.MeetingAgendaRecommendations?
                .Select(r => new MinutesRecommendationDto
                {
                    Id = r.Id,
                    Text = r.Text,
                    OwnerId = r.Owner,
                    OwnerName = language == LanguageDbEnum.Arabic
                        ? (r.OwnerNavigation?.FullnameAr ?? r.OwnerNavigation?.FullnameEn ?? "N/A")
                        : (r.OwnerNavigation?.FullnameEn ?? r.OwnerNavigation?.FullnameAr ?? "N/A"),
                    DueDate = r.DueDate,
                    Priority = r.PriorityId?.ToString()
                }).ToList() ?? new List<MinutesRecommendationDto>();

            return item;
        }

        /// <summary>
        /// Gets meeting minutes data for preview (without saving)
        /// </summary>
        public async Task<MeetingMinutesDto?> GetMeetingMinutesPreview(int meetingId, string userId, LanguageDbEnum language, bool includePrivateNotes = false)
        {
            var meeting = await _mmsUnitOfWork.Meetings.GetIncludeAttendeesAndAssociatedAsync(x => x.Id == meetingId);
            if (meeting == null) return null;

            var agendas = await _mmsUnitOfWork.MeetingAgendas.ListIncludeAllForMinutesAsync(x => x.MeetingId == meetingId);
            var meetingSummary = await GetMeetingSummaryAsync(meetingId);
            var user = await _userManagementUnitOfWork.Users.Find(userId);

            return BuildMeetingMinutesDto(meeting, agendas, meetingSummary, user, language, includePrivateNotes);
        }

        #endregion

		#region Meeting Tasks

		public async Task<GenericPaginationListDto<MMS.DTO.Tasks.MeetingTaskListItemDto>?> ListMeetingTasksAsync(string userId, int page, int pageSize, LanguageDbEnum language)
		{
			System.Linq.Expressions.Expression<Func<DAL.Models.MMS.Task, bool>> filter =
				x => x.UserId == userId && x.StatusId == (int)TaskStatusDbEnum.PendingApproval;
			var totalTasks = await _mmsUnitOfWork.Tasks.CountAsync(filter);
			var tasks = await _mmsUnitOfWork.Tasks.ListIncludeAllAsync(
				page, pageSize, filter: filter, orderBy: x => x.Id, true);

			var retval = tasks.Select(x => _mapper.Map<MMS.DTO.Tasks.MeetingTaskListItemDto>((x, language))).ToList();
			return new GenericPaginationListDto<MMS.DTO.Tasks.MeetingTaskListItemDto>(totalTasks, retval);
		}

		public async Task<bool> ClaimMeetingTaskAsync(int taskId, string userId)
		{
			var task = await _mmsUnitOfWork.Tasks.GetAsync(x => x.Id == taskId && x.UserId == userId);
			if (task != null && (task.Claimed == null || task.Claimed == false))
			{
				task.Claimed = true;
				task.ClaimedDate = DateTime.UtcNow;
				await _mmsUnitOfWork.SaveChangesAsync();
				return true;
			}
			return false;
		}

		public async Task<(bool success, string message)> ApproveMeetingTaskAsync(int taskId, string userId, bool approved, string note)
		{
			var task = await _mmsUnitOfWork.Tasks.GetAsync(x => x.Id == taskId);
			if (task == null) return (false, "Task not found");

			// For sign tasks, verify the user has actually signed before approving
			if (approved && task.TypeId == (int)TaskTypeDbEnum.SignFinalMeetingMinutes)
			{
				var signed = await CheckMeetingMinutesSignedAsync(task, userId);
				if (!signed)
					return (false, "Minutes must be signed before approval");
			}

			task.StatusId = approved ? (int)TaskStatusDbEnum.Approved : (int)TaskStatusDbEnum.Rejected;
			task.CompletedDate = DateTime.Now;

			var meetingNote = new MeetingNote
			{
				CreatedBy = userId,
				MeetingId = task.MeetingId,
				CreatedDate = DateTime.Now,
				Text = note,
				TaskId = taskId
			};
			await _mmsUnitOfWork.MeetingNotes.AddAsync(meetingNote);
			await _mmsUnitOfWork.SaveChangesAsync();

			// Check if all required approvals are complete for MeetingApproval tasks
			if (approved && task.TypeId == (int)TaskTypeDbEnum.MeetingApproval)
			{
				await CheckAndUpdateMeetingApprovalStatusAsync(task.MeetingId);
			}

			return (true, "Updated");
		}

		public async Task<bool> IsTaskOwnerAsync(string userId, int taskId)
		{
			return await _mmsUnitOfWork.Tasks.AnyAsync(x => x.Id == taskId && x.UserId == userId);
		}

		public async Task<bool> IsTaskCompletedAsync(int taskId)
		{
			var task = await _mmsUnitOfWork.Tasks.Find(taskId);
			if (task == null) return false;
			return task.StatusId == (int)TaskStatusDbEnum.Approved || task.StatusId == (int)TaskStatusDbEnum.Rejected;
		}

		private async Task CheckAndUpdateMeetingApprovalStatusAsync(int meetingId)
		{
			var attendeesNeedingApproval = await _mmsUnitOfWork.MeetingAttendees.ListAsync(
				x => x.MeetingId == meetingId && x.NeedsApproval);

			if (!attendeesNeedingApproval.Any()) return;

			var attendeeUserIds = attendeesNeedingApproval.Select(a => a.UserId).ToList();
			var approvalTasks = await _mmsUnitOfWork.Tasks.ListAsync(
				x => x.MeetingId == meetingId
				&& x.TypeId == (int)TaskTypeDbEnum.MeetingApproval
				&& attendeeUserIds.Contains(x.UserId));

			if (approvalTasks.All(t => t.StatusId == (int)TaskStatusDbEnum.Approved))
			{
				var meeting = await _mmsUnitOfWork.Meetings.GetAsync(x => x.Id == meetingId);
				if (meeting != null && meeting.StatusId == (int)MeetingStatusDbEnum.PendingMeetingApproval)
				{
					meeting.StatusId = (int)MeetingStatusDbEnum.Approved;
					await _mmsUnitOfWork.SaveChangesAsync();
				}
			}
		}

		private async Task<bool> CheckMeetingMinutesSignedAsync(DAL.Models.MMS.Task task, string userId)
		{
			var attachments = await _mmsUnitOfWork.Attachments.ListAsync(x =>
				x.RecordId == task.MeetingId
				&& x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.FinalMeetingMinutes);

			if (!attachments.Any()) return false;

			var attachmentIds = attachments.Select(a => a.Id).ToList();
			return await _processUnitOfWork.AttachmentsSignatures.AnyAsync(x =>
				attachmentIds.Contains(x.AttachmentId) && x.UserId == userId);
		}

		#endregion
    }
}
