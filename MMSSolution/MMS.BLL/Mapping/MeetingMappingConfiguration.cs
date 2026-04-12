using System.Linq;
using Mapster;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Meetings;
using MMS.DTO.Reports;
using Task = MMS.DAL.Models.MMS.Task;

namespace MMS.BLL.Mapping
{
    internal class MeetingMappingConfiguration : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {

			config.NewConfig<Meeting, MeetingPostDto>()
				  .Map(dest => dest.TypeId, src => src.MeetingTypeId)
				  .Map(dest => dest.Url, src => src.MeetingUrl);

			config.NewConfig<(Meeting meeting, LanguageDbEnum Language), MeetingListItemDto>()
                  .Map(dest => dest.Id, src => src.meeting.Id)
                  .Map(dest => dest.ReferenceNumber, src => src.meeting.ReferenceNumber)
                  .Map(dest => dest.StatusId, src => src.meeting.StatusId)
                  .Map(dest => dest.Title, src => src.meeting.Title)
                  .Map(dest => dest.Date, src => src.meeting.Date)
                  .Map(dest => dest.StartTime, src => src.meeting.StartTime)
                  .Map(dest => dest.EndTime, src => src.meeting.EndTime)
                  .Map(dest => dest.Location, src => src.meeting.Location)
                  .Map(dest => dest.Notes, src => src.meeting.Notes)
                  .Map(dest => dest.StatusName, src => src.Language == LanguageDbEnum.Arabic ? src.meeting.Status.NameAr : src.meeting.Status.NameEn)
                  .Map(dest => dest.CommitteeName, src => src.Language == LanguageDbEnum.Arabic ? src.meeting.Committee != null ? src.meeting.Committee.NameAr : string.Empty : src.meeting.Committee != null ? src.meeting.Committee.NameEn : string.Empty)
                  .Map(dest => dest.CreatedById, src => src.meeting.Createdby)
                  .Map(dest => dest.CreatedByName, src => src.meeting.CreatedbyNavigation != null ? (src.Language == LanguageDbEnum.Arabic ? src.meeting.CreatedbyNavigation.FullnameAr : src.meeting.CreatedbyNavigation.FullnameEn) : string.Empty)
                  .Map(dest => dest.ApprovedCount, src => src.meeting.Tasks != null ? src.meeting.Tasks.Count(t => t.StatusId == (int)TaskStatusDbEnum.Approved) : 0)
                  .Map(dest => dest.TotalApprovalCount, src => src.meeting.Tasks != null ? src.meeting.Tasks.Count() : 0);

            config.NewConfig<(MeetingAgendaListItemDto meetingAgendaListItemDto, string userId), MeetingAgendum>()
                  .Map(dest => dest.CommitteeDutyId, src => src.meetingAgendaListItemDto.CommitteeDutyId)
                  .Map(dest => dest.Title, src => src.meetingAgendaListItemDto.Title)
                  .Map(dest => dest.Duration, src => src.meetingAgendaListItemDto.Duration)
                  .Map(dest => dest.VotingTypeId, src => src.meetingAgendaListItemDto.VotingTypeId)
                  .Map(dest => dest.CreatedDate, src => DateTime.Now)
                  .Map(dest => dest.AgendaTopics, src => src.meetingAgendaListItemDto.AgendaTopics)
                  .Map(dest => dest.CreatedBy, src => src.userId);
			config.NewConfig<(MeetingAgendaPostDto meetingAgendaPostDto, string userId), MeetingAgendum>()
				  .Map(dest => dest.CommitteeDutyId, src => src.meetingAgendaPostDto.CommitteeDutyId)
				  .Map(dest => dest.Title, src => src.meetingAgendaPostDto.Title)
				  .Map(dest => dest.Duration, src => src.meetingAgendaPostDto.Duration)
				  .Map(dest => dest.VotingTypeId, src => src.meetingAgendaPostDto.VotingTypeId)
				  .Map(dest => dest.CreatedDate, src => DateTime.Now)
				  .Map(dest => dest.AgendaTopics, src => src.meetingAgendaPostDto.AgendaTopics)
				  .Map(dest => dest.CreatedBy, src => src.userId);

			config.NewConfig<(MeetingAgendum meetingAgenda, LanguageDbEnum Language), MeetingAgendaListItemDto>()
                  .Map(dest => dest.Id, src => src.meetingAgenda.Id)
                  .Map(dest => dest.MeetingId, src => src.meetingAgenda.MeetingId)
                  .Map(dest => dest.Title, src => src.meetingAgenda.Title)
                  .Map(dest => dest.Duration, src => src.meetingAgenda.Duration)
                  .Map(dest => dest.Voting, src => src.Language == LanguageDbEnum.Arabic ? src.meetingAgenda.VotingType.NameAr : src.meetingAgenda.VotingType.NameAr)
                  .Map(dest => dest.VotingType, src => src.meetingAgenda.VotingType )
                  .Map(dest => dest.VotingTypeId, src => src.meetingAgenda.VotingTypeId )
                  .Map(dest => dest.AgendaTopics, src => src.meetingAgenda.AgendaTopics);

			/*config.NewConfig<MeetingAgendum, MeetingAgendaDto>()
				  .Map(dest => dest.MeetingAgendaRecommendations, src => src.MeetingAgendaRecommendations);*/

			config.NewConfig<(MeetingAgendum meetingAgenda, LanguageDbEnum Language), LiveMeetingAgendaDto>()
				  .Map(dest => dest.Id, src => src.meetingAgenda.Id)
				  .Map(dest => dest.MeetingId, src => src.meetingAgenda.MeetingId)
				  .Map(dest => dest.Title, src => src.meetingAgenda.Title)
				  .Map(dest => dest.Duration, src => src.meetingAgenda.Duration)
				  .Map(dest => dest.Voting, src => src.meetingAgenda.VotingTypeId)
				  .Map(dest => dest.Voting, src => src.Language == LanguageDbEnum.Arabic ? src.meetingAgenda.VotingType.NameAr : src.meetingAgenda.VotingType.NameAr)
				  .Map(dest => dest.Duty, src => src.meetingAgenda.CommitteeDuty != null ? src.meetingAgenda.CommitteeDuty.Title : string.Empty)
				  .Map(dest => dest.ActualStartDate, src => src.meetingAgenda.ActualStartDate)
				  .Map(dest => dest.ActualEndDate, src => src.meetingAgenda.ActualEndDate)
				  .Map(dest => dest.Paused, src => src.meetingAgenda.Paused)
				  .Map(dest => dest.PauseDuration, src => src.meetingAgenda.PauseDuration)
				  .Map(dest => dest.VotingType, src => src.meetingAgenda.VotingType)
				  .Map(dest => dest.MeetingUserVotes, src => src.meetingAgenda.MeetingUserVotes.Select(v => new MeetingUserVoteDto
				  {
					  Id = v.Id,
					  UserId = v.UserId,
					  UserName = src.Language == LanguageDbEnum.Arabic ? v.User.FullnameAr : v.User.FullnameEn,
					  VottingOptionId = v.VottingOptionId,
					  SelectedOptionName = src.Language == LanguageDbEnum.Arabic ? v.VottingOption.NameAr : v.VottingOption.NameEn,
					  CreatedDate = v.CreatedDate,
					  MeetingAgendaId = v.MeetingAgendaId
				  }).ToList())
				  .Map(dest => dest.VotingTypeId, src => src.meetingAgenda.VotingTypeId)
				  .Map(dest => dest.LastPausedDate, src => src.meetingAgenda.LastPausedDate)
				  .Map(dest => dest.AgendaTopics, src => src.meetingAgenda.AgendaTopics);

			config.NewConfig<(MeetingAgendum meetingAgendum, LanguageDbEnum Language), MeetingAgendaPostDto>()
                  .Map(dest => dest.Id, src => src.meetingAgendum.Id)
                  .Map(dest => dest.Title, src => src.meetingAgendum.Title)
                  .Map(dest => dest.Duration, src => src.meetingAgendum.Duration)
                  .Map(dest => dest.CommitteeDutyId, src => src.meetingAgendum.CommitteeDutyId)
                  .Map(dest => dest.VotingTypeId, src => src.meetingAgendum.VotingTypeId)
                  .Map(dest => dest.AgendaTopics, src => src.meetingAgendum.AgendaTopics)
                  .Map(dest => dest.Voting, src => src.Language == LanguageDbEnum.Arabic ? src.meetingAgendum.VotingType.NameAr : src.meetingAgendum.VotingType.NameAr)
                  .Map(dest => dest.Duty, src => src.meetingAgendum.CommitteeDuty != null ? src.meetingAgendum.CommitteeDuty.Title : string.Empty);

            config.NewConfig<Meeting, ListItemDto>()
                 .Map(dest => dest.Id, src => src.Id)
                 .Map(dest => dest.Name, src => src.Title);

            config.NewConfig<Meeting, AssociatedMeetingDto>()
                 .Map(dest => dest.Id, src => src.Id)
                 .Map(dest => dest.Name, src => src.Title)
                 .Map(dest => dest.ReferenceNumber, src => src.ReferenceNumber)
                 .Map(dest => dest.Date, src => src.Date);

              config.NewConfig<(MeetingAttendee Attendee, LanguageDbEnum Language), MeetingAttendeePostDto>()
                 .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.Attendee.User.FullnameAr : src.Attendee.User.FullnameEn)
                 .Map(dest => dest.UserId, src =>  src.Attendee.UserId)
                 .Map(dest => dest.NeedsApproval, src =>  src.Attendee.NeedsApproval)
                 .Map(dest => dest.Attended, src =>  src.Attendee.Attended)
                 .Map(dest => dest.HasProfilePicture, src =>  src.Attendee.User.HasProfilePicture)
                 .Map(dest => dest.JobTitle, src =>  src.Attendee.JobTitle);
			config.NewConfig<(MeetingAgendaNote Note, LanguageDbEnum Language), MeetingAgendaNoteListItemDto>()
				 .Map(dest => dest.CreatedByName, src => src.Language == LanguageDbEnum.Arabic ? src.Note.User.FullnameAr : src.Note.User.FullnameEn)
				 .Map(dest => dest.CreatedBy, src => src.Note.User.Id)
				 .Map(dest => dest.IsPublic, src => src.Note.IsPublic)
				 .Map(dest => dest.Text, src => src.Note.Text)
				 .Map(dest => dest.Id, src => src.Note.Id)
				 .Map(dest => dest.CreatedAt, src => src.Note.CreatedAt);

			config.NewConfig<(MeetingAgendaRecommendation Recommendation, LanguageDbEnum Language), MeetingAgendaRecommendationListItemDto>()
				 .Map(dest => dest.CreatedByName, src => src.Language == LanguageDbEnum.Arabic ? src.Recommendation.CreateByNavigation.FullnameAr : src.Recommendation.CreateByNavigation.FullnameEn)
				 .Map(dest => dest.CreatedBy, src => src.Recommendation.CreateBy)
				 .Map(dest => dest.CreatedAt, src => src.Recommendation.CreatedAt)
				 .Map(dest => dest.DueDate, src => src.Recommendation.DueDate)
				 .Map(dest => dest.Owner, src => src.Recommendation.Owner)
				 .Map(dest => dest.OwnerName, src => src.Language == LanguageDbEnum.Arabic ? src.Recommendation.OwnerNavigation.FullnameAr : src.Recommendation.OwnerNavigation.FullnameEn)
				 .Map(dest => dest.StatusId, src => src.Recommendation.StatusId)
				 .Map(dest => dest.Text, src => src.Recommendation.Text)
				 .Map(dest => dest.Status, src => src.Language == LanguageDbEnum.Arabic ? src.Recommendation.Status.NameAr:src.Recommendation.Status.NameEn)
				 .Map(dest => dest.Percentage, src => src.Recommendation.Percentage)
				 .Map(dest => dest.Id, src => src.Recommendation.Id)
				 .Map(dest => dest.PriorityId, src => src.Recommendation.PriorityId)
				 .Map(dest => dest.PriorityName, src => src.Recommendation.Priority != null ? (src.Language == LanguageDbEnum.Arabic ? src.Recommendation.Priority.NameAr : src.Recommendation.Priority.NameEn) : null)
				 .Map(dest => dest.OwnerStructureId, src => src.Recommendation.OwnerStructureId)
				 .Map(dest => dest.OwnerStructureName, src => (string?)null)
				 .Map(dest => dest.Description, src => src.Recommendation.Description);

			config.NewConfig<(MeetingAgendaRecommendation Recommendation, LanguageDbEnum Language), MeetingAgendaRecommendationFollowUpListItemDto>()
				 .Map(dest => dest.CreatedByName, src => src.Language == LanguageDbEnum.Arabic ? src.Recommendation.CreateByNavigation.FullnameAr : src.Recommendation.CreateByNavigation.FullnameEn)
				 .Map(dest => dest.CreatedBy, src => src.Recommendation.CreateBy)
				 .Map(dest => dest.CreatedAt, src => src.Recommendation.CreatedAt)
				 .Map(dest => dest.DueDate, src => src.Recommendation.DueDate)
				 .Map(dest => dest.Owner, src => src.Recommendation.Owner)
				 .Map(dest => dest.OwnerName, src => src.Language == LanguageDbEnum.Arabic ? src.Recommendation.OwnerNavigation.FullnameAr : src.Recommendation.OwnerNavigation.FullnameEn)
				 .Map(dest => dest.StatusId, src => src.Recommendation.StatusId)
				 .Map(dest => dest.Text, src => src.Recommendation.Text)
				 .Map(dest => dest.Status, src => src.Language == LanguageDbEnum.Arabic ? src.Recommendation.Status.NameAr : src.Recommendation.Status.NameEn)
				 .Map(dest => dest.Percentage, src => src.Recommendation.Percentage)
				 .Map(dest => dest.MeetingReferenceNo, src => src.Recommendation.MeetingAgenda.Meeting.ReferenceNumber)
				 .Map(dest => dest.MeetingId, src => src.Recommendation.MeetingAgenda.MeetingId)
				 .Map(dest => dest.CanEdit, src => src.Recommendation.StatusId == (int)MeetingAgendaRecommendationStatusDbEnum.InProgess)
				 .Map(dest => dest.Id, src => src.Recommendation.Id);

            config.NewConfig<(MeetingAttendee attendee, LanguageDbEnum Language), AttendanceReportDto>()
                 .Map(dest => dest.UserName, src => src.Language == LanguageDbEnum.Arabic ? src.attendee.User.FullnameAr : src.attendee.User.FullnameEn)
                 .Map(dest => dest.UserId, src => src.attendee.UserId)
                 .Map(dest => dest.MeetingDate, src => src.attendee.Meeting.Date)
                 .Map(dest => dest.Title, src => src.attendee.Meeting.Title)
                 .Map(dest => dest.Attended, src => src.attendee.Attended)
                 .Map(dest => dest.MeetingId, src => src.attendee.MeetingId)
                 .Map(dest => dest.CommitteeName, src => src.attendee.Meeting.Committee==null?"": src.Language == LanguageDbEnum.Arabic ? src.attendee.Meeting.Committee.NameAr : src.attendee.Meeting.Committee.NameAr);

			config.NewConfig<Meeting, MeetingInfoDto>()
			   .Map(dest => dest.Date, src => src.Date.ToString("yyyy-MM-dd"));

            config.NewConfig<(Task task, LanguageDbEnum Language), MeetingUserApprovalDto>()
				.Map(dest => dest.Id, src => src.task.Id)
				.Map(dest => dest.UserId, src => src.task.UserId)
				.Map(dest => dest.UserName, src => src.Language == LanguageDbEnum.Arabic ? src.task.User.FullnameAr : src.task.User.FullnameEn)
				.Map(dest => dest.StatusId, src => src.task.StatusId)
				.Map(dest => dest.StatusName, src => src.Language == LanguageDbEnum.Arabic ? src.task.Status.NameAr : src.task.Status.NameEn)
				.Map(dest => dest.ApproveDate, src => src.task.CompletedDate)
				.Map(dest => dest.Comment, src => src.task.MeetingNotes != null && src.task.MeetingNotes.Any()
					? src.task.MeetingNotes.OrderByDescending(n => n.CreatedDate).First().Text
					: null)
				.Map(dest => dest.AttachmentId, src => src.task.AttachmentId)
				.Map(dest => dest.Version, src => src.task.Attachment != null ? (int?)src.task.Attachment.Version : null);

            config.NewConfig<(MeetingUserVote userVotes, LanguageDbEnum Language), MeetingAgendaVotingResultsListItemDto>()
             .Map(dest => dest.UserFullName, src => src.Language == LanguageDbEnum.Arabic ? src.userVotes.User.FullnameAr : src.userVotes.User.FullnameEn)
             .Map(dest => dest.CreatedDate, src => src.userVotes.CreatedDate)
             .Map(dest => dest.SelectedOption, src => src.Language == LanguageDbEnum.Arabic ? src.userVotes.VottingOption.NameAr : src.userVotes.VottingOption.NameEn)
             .Map(dest => dest.AgendaTitle, src => src.userVotes.MeetingAgenda.Title);

			config.NewConfig<(MeetingUserVote vote, LanguageDbEnum Language), MeetingUserVoteDto>()
				 .Map(dest => dest.Id, src => src.vote.Id)
				 .Map(dest => dest.UserId, src => src.vote.UserId)
				 .Map(dest => dest.UserName, src => src.Language == LanguageDbEnum.Arabic ? src.vote.User.FullnameAr : src.vote.User.FullnameEn)
				 .Map(dest => dest.VottingOptionId, src => src.vote.VottingOptionId)
				 .Map(dest => dest.SelectedOptionName, src => src.Language == LanguageDbEnum.Arabic ? src.vote.VottingOption.NameAr : src.vote.VottingOption.NameEn)
				 .Map(dest => dest.CreatedDate, src => src.vote.CreatedDate)
				 .Map(dest => dest.MeetingAgendaId, src => src.vote.MeetingAgendaId);

			config.NewConfig<Meeting, MeetingInvitationMailDto>()
			   .Map(dest => dest.AttendeesEmails, src => src.MeetingAttendees.Select(x => x.User.Email))
			   .Map(dest => dest.MeetingAgenda, src => src.MeetingAgenda)
			   .Map(dest => dest.CreatedByName, src => src.CreatedbyNavigation.FullnameAr);
		}
	}
}
