using DocumentFormat.OpenXml.Spreadsheet;
using MapsterMapper;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Meetings;
using System.Net.Http.Headers;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class MeetingAgendaManager
    {
        private readonly IMapper _mapper;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;
        private readonly MmsNotificationService _notify;
        public MeetingAgendaManager(IMapper mapper, IMMSUnitOfWork mmsUnitOfWork, IUserManagementUnitOfWork userManagementUnitOfWork, MmsNotificationService notify)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
            _notify = notify;
        }
        public async Task<List<MeetingUserVoteDto>> AddMeetingAgendaVote(string userId, MeetingUserVotePostDto meetingUserVotePostDto, LanguageDbEnum language = LanguageDbEnum.Arabic)
        {
            // Check if user already voted on this agenda
            var existingVote = await _mmsUnitOfWork.MeetingUserVotes.GetAsync(
                x => x.UserId == userId && x.MeetingAgendaId == meetingUserVotePostDto.MeetingAgendaId);

            if (existingVote != null)
            {
                // Update existing vote
                existingVote.VottingOptionId = meetingUserVotePostDto.VotingOptionId;
                existingVote.CreatedDate = DateTime.Now;
                _mmsUnitOfWork.MeetingUserVotes.Update(existingVote);
            }
            else
            {
                // Create new vote
                var meetingUserVote = new MeetingUserVote()
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now,
                    VottingOptionId = meetingUserVotePostDto.VotingOptionId,
                    MeetingAgendaId = meetingUserVotePostDto.MeetingAgendaId
                };
                await _mmsUnitOfWork.MeetingUserVotes.AddAsync(meetingUserVote);
            }

            await _mmsUnitOfWork.SaveChangesAsync();

            // Notify meeting owner about the vote
            var ownerId = await _mmsUnitOfWork.MeetingAgendas.GetMeetingOwner(meetingUserVotePostDto.MeetingAgendaId);
            _ = _notify.VoteCast(ownerId, userId);

            var votes = await _mmsUnitOfWork.MeetingUserVotes.ListIncludeMeetingAgendaAndUserAndVottingOptionAsync(x => x.MeetingAgendaId == meetingUserVotePostDto.MeetingAgendaId);
            return votes.Select(x => _mapper.Map<MeetingUserVoteDto>((x, language))).ToList();
        }
        public async Task<bool> AddMeetingAgendaNote(string userId, MeetingAgendaNotePostDto meetingAgendaNotePostDto)
        {
            var meetingAgendaNote = new MeetingAgendaNote()
            {
                UserId = userId,
                CreatedAt = DateTime.Now,
                Text = meetingAgendaNotePostDto.Text,
                MeetingAgendaId = meetingAgendaNotePostDto.MeetingAgendaId,
                IsPublic = meetingAgendaNotePostDto.IsPublic
            };
            await _mmsUnitOfWork.MeetingAgendaNotes.AddAsync(meetingAgendaNote);
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }
        public async Task<List<MeetingAgendaNoteListItemDto>> ListMeetingAgendaNotes(string userId, int meetingAgendaId, LanguageDbEnum language)
        {
            var notes = await _mmsUnitOfWork.MeetingAgendaNotes.ListAsyncIncludeUser(x => x.MeetingAgendaId == meetingAgendaId && (x.UserId == userId || (x.IsPublic)));
            return notes.Select(x => _mapper.Map<MeetingAgendaNoteListItemDto>((x, language))).ToList();
        }
        public async Task<bool> UpdateMeetingAgendaNote(MeetingAgendaNotePutDto meetingAgendaNotePutDto)
        {
            var meetingAgendaNote = await _mmsUnitOfWork.MeetingAgendaNotes.GetAsync(x => x.Id == meetingAgendaNotePutDto.Id);
            meetingAgendaNote.Text = meetingAgendaNotePutDto.Text;
            meetingAgendaNote.IsPublic = meetingAgendaNotePutDto.IsPublic;

            _mmsUnitOfWork.MeetingAgendaNotes.Update(meetingAgendaNote);
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteMeetingAgendaNote(int meetingAgendaNoteId)
        {
            var meetingAgendaNote = await _mmsUnitOfWork.MeetingAgendaNotes.GetAsync(x => x.Id == meetingAgendaNoteId);
            _mmsUnitOfWork.MeetingAgendaNotes.Remove(meetingAgendaNote);
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddMeetingAgendaRecommendation(string userId, MeetingAgendaRecommendationPostDto meetingAgendaRecommendationPostDto)
        {
            var meetingAgendaRecommendation = new MeetingAgendaRecommendation()
            {
                CreateBy = userId,
                CreatedAt = DateTime.Now,
                Text = meetingAgendaRecommendationPostDto.Text,
                Description = meetingAgendaRecommendationPostDto.Description,
                MeetingAgendaId = meetingAgendaRecommendationPostDto.MeetingAgendaId,
                Owner = meetingAgendaRecommendationPostDto.Owner,
                StatusId = (int)MeetingAgendaRecommendationStatusDbEnum.Draft,
                Percentage = 0,
                DueDate = meetingAgendaRecommendationPostDto.DueDate,
                PriorityId = meetingAgendaRecommendationPostDto.PriorityId,
                OwnerStructureId = meetingAgendaRecommendationPostDto.OwnerStructureId
            };
            await _mmsUnitOfWork.MeetingAgendaRecommendations.AddAsync(meetingAgendaRecommendation);
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }
        public async Task<List<MeetingAgendaRecommendationListItemDto>> ListMeetingAgendaRecommendations(int meetingAgendaId, LanguageDbEnum language)
        {
            var recommendations = await _mmsUnitOfWork.MeetingAgendaRecommendations.ListAsyncIncludeUserAndStatus(x => x.MeetingAgendaId == meetingAgendaId);
            return recommendations.Select(x => _mapper.Map<MeetingAgendaRecommendationListItemDto>((x, language))).ToList();
        }
        public async Task<bool> UpdateMeetingAgendaRecommendation(MeetingAgendaRecommendationPutDto meetingAgendaRecommendationPutDto)
        {
            var meetingAgendaRecommendation = await _mmsUnitOfWork.MeetingAgendaRecommendations.GetAsync(x => x.Id == meetingAgendaRecommendationPutDto.Id);
            meetingAgendaRecommendation.Text = meetingAgendaRecommendationPutDto.Text;
            meetingAgendaRecommendation.Description = meetingAgendaRecommendationPutDto.Description;
            meetingAgendaRecommendation.Owner = meetingAgendaRecommendationPutDto.Owner;
            meetingAgendaRecommendation.DueDate = meetingAgendaRecommendationPutDto.DueDate;
            meetingAgendaRecommendation.PriorityId = meetingAgendaRecommendationPutDto.PriorityId;
            meetingAgendaRecommendation.OwnerStructureId = meetingAgendaRecommendationPutDto.OwnerStructureId;

            _mmsUnitOfWork.MeetingAgendaRecommendations.Update(meetingAgendaRecommendation);
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteMeetingAgendaRecommendation(int meetingAgendaRecommendationId)
        {
            var meetingAgendaRecommendation = await _mmsUnitOfWork.MeetingAgendaRecommendations.GetAsync(x => x.Id == meetingAgendaRecommendationId);
            _mmsUnitOfWork.MeetingAgendaRecommendations.Remove(meetingAgendaRecommendation);
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<List<MeetingAgendaVotingResultsListItemDto>> ListMeetingAgendaVotingResultsAsync(int agendaId, LanguageDbEnum language)
        {

            var votes = await _mmsUnitOfWork.MeetingUserVotes.ListIncludeMeetingAgendaAndUserAndVottingOptionAsync(x => x.MeetingAgendaId == agendaId);
            var retVal = votes.Select(x => _mapper.Map<MeetingAgendaVotingResultsListItemDto>((x, language))).ToList();
            return retVal;
        }

        /// <summary>
        /// Checks if user is a meeting owner (creator, super admin, or has committee permissions)
        /// </summary>
        public async Task<bool> IsMeetingOwner(string userId, int meetingAgendaId)
        {
            // Check if user is the meeting creator
            var creatorId = await _mmsUnitOfWork.MeetingAgendas.GetMeetingOwner(meetingAgendaId);
            if (creatorId == userId) return true;

            // Check if user is SuperAdmin
            var isSuperAdmin = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(p =>
                p.UserId == userId && p.PermissionId == (int)PermissionDbEnum.SuperAdmin);
            if (isSuperAdmin) return true;

            // Check committee permissions for committee meetings
            var hasCommitteePermission = await _mmsUnitOfWork.MeetingAgendas.AnyAsync(x =>
                x.Id == meetingAgendaId &&
                x.Meeting.IsCommittee == true &&
                x.Meeting.Committee != null &&
                x.Meeting.Committee.CommitteePermissions.Any(p =>
                    p.UserId == userId &&
                    (p.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetings ||
                     p.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetingMinutes)));
            if (hasCommitteePermission) return true;

            // Check if user is committee classification admin
            var agenda = await _mmsUnitOfWork.MeetingAgendas.GetAsync(x => x.Id == meetingAgendaId);
            if (agenda != null)
            {
                var meeting = await _mmsUnitOfWork.Meetings.Find(agenda.MeetingId);
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
            }

            return false;
        }

        public async Task<bool> canEditRecommendation(int id, string userId)
        {
            // Check if user created the recommendation
            var isCreator = await _mmsUnitOfWork.MeetingAgendaRecommendations.AnyAsync(x => x.Id == id && x.CreateBy == userId);
            if (isCreator) return true;

            // Also allow meeting owners to edit recommendations
            var recommendation = await _mmsUnitOfWork.MeetingAgendaRecommendations.GetAsync(x => x.Id == id);
            if (recommendation != null)
            {
                return await IsMeetingOwner(userId, recommendation.MeetingAgendaId);
            }
            return false;
        }

        /// <summary>
        /// Checks if user is a member of the meeting (attendee, owner, or has committee permissions)
        /// </summary>
        public async Task<bool> isMeetingMember(int meetingAgendaId, string userId)
        {
            // Check if user is meeting owner
            var isOwner = await IsMeetingOwner(userId, meetingAgendaId);
            if (isOwner) return true;

            // Check if user is an attendee
            var isAttendee = await _mmsUnitOfWork.MeetingAgendas.AnyAsync(x =>
                x.Id == meetingAgendaId &&
                x.Meeting.MeetingAttendees.Any(a => a.UserId == userId));
            if (isAttendee) return true;

            // Check committee permissions for viewing meetings
            var hasCommitteePermission = await _mmsUnitOfWork.MeetingAgendas.AnyAsync(x =>
                x.Id == meetingAgendaId &&
                x.Meeting.IsCommittee == true &&
                x.Meeting.Committee != null &&
                x.Meeting.Committee.CommitteePermissions.Any(p =>
                    p.UserId == userId &&
                    p.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetings));

            return hasCommitteePermission;
        }

        public async Task<bool> canEditNote(int noteId, string userId)
        {
            return await _mmsUnitOfWork.MeetingAgendaNotes.AnyAsync(x => x.Id == noteId && x.UserId == userId);
        }

        public async Task<int?> GetMeetingIdByAgendaId(int meetingAgendaId)
        {
            var agenda = await _mmsUnitOfWork.MeetingAgendas.GetAsync(x => x.Id == meetingAgendaId);
            return agenda?.MeetingId;
        }

        public async Task<int?> GetMeetingIdByNoteId(int noteId)
        {
            var note = await _mmsUnitOfWork.MeetingAgendaNotes.GetAsyncIncludeAgenda(x => x.Id == noteId);
            return note?.MeetingAgenda?.MeetingId;
        }

        public async Task<(int? MeetingId, int? AgendaId, bool IsPublic)> GetNoteInfoForSignalR(int noteId)
        {
            var note = await _mmsUnitOfWork.MeetingAgendaNotes.GetAsyncIncludeAgenda(x => x.Id == noteId);
            if (note == null) return (null, null, false);
            return (note.MeetingAgenda?.MeetingId, note.MeetingAgendaId, note.IsPublic);
        }

        #region Agenda Summary

        public async Task<string?> GetAgendaSummaryAsync(int meetingAgendaId)
        {
            var agenda = await _mmsUnitOfWork.MeetingAgendas.GetIncludeSummaryAsync(x => x.Id == meetingAgendaId);
            var summary = agenda?.MeetingAgendaSummaries?.OrderByDescending(s => s.CreatedDate).FirstOrDefault();
            return summary?.Text;
        }

        public async Task SaveAgendaSummaryAsync(string userId, int meetingAgendaId, string summary)
        {
            var agenda = await _mmsUnitOfWork.MeetingAgendas.GetIncludeSummaryAsync(x => x.Id == meetingAgendaId);
            if (agenda != null)
            {
                var existingSummary = agenda.MeetingAgendaSummaries?.OrderByDescending(s => s.CreatedDate).FirstOrDefault();
                if (existingSummary != null)
                {
                    existingSummary.Text = summary;
                    await _mmsUnitOfWork.SaveChangesAsync();
                }
                else
                {
                    agenda.MeetingAgendaSummaries.Add(new MeetingAgendaSummary
                    {
                        Text = summary,
                        CreatedBy = userId,
                        MeetingAgendaId = meetingAgendaId
                    });
                    await _mmsUnitOfWork.SaveChangesAsync();
                }
            }
        }

        #endregion
    }
}
