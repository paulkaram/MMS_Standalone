using DocumentFormat.OpenXml.Wordprocessing;
using Intalio.Tools.Common.Extensions.FileExtensions;
using Intalio.Tools.Common.Storage;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using MMS.BLL.Common.Helpers;
using MMS.BLL.Storage;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Meetings;
using System.Linq.Expressions;

namespace MMS.BLL.Managers
{
    public class MeetingAgendaRecommendationsManager
    {
        private readonly IMapper _mapper;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;
        private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;
        private readonly IFilterHelper _filterHelper;
        private readonly IStorage _storage;
		private readonly StorageManager _storageManager;

		public MeetingAgendaRecommendationsManager(IMapper mapper,
            IMMSUnitOfWork mmsUnitOfWork,
            StorageFactory storageFactory,
			StorageManager storageManager,
            IUserManagementUnitOfWork userManagementUnitOfWork,
            IFilterHelper filterHelper)
        {
            _mapper = mapper;
            _storage = storageFactory.GetStorage();
            _mmsUnitOfWork = mmsUnitOfWork;
            _filterHelper = filterHelper;
            _storageManager = storageManager;
            _userManagementUnitOfWork = userManagementUnitOfWork;

        }



        public async Task<GenericPaginationListDto<MeetingAgendaRecommendationFollowUpListItemDto>> ListMeetingAgendaRecommendations(SearchRecommendationsDto searchRecommendationsDto, string userId, LanguageDbEnum language, int Page , int PageSize )
        {
            Expression<Func<MeetingAgendaRecommendation, bool>> filter = x => x.StatusId != (int)MeetingAgendaRecommendationStatusDbEnum.Draft && (x.CreateBy == userId || x.Owner == userId);


            if (!string.IsNullOrEmpty(searchRecommendationsDto.MeetingReferenceNo))
            {
                Expression<Func<MeetingAgendaRecommendation, bool>> MeetingRefCondition = x => x.MeetingAgenda.Meeting.ReferenceNumber.Contains(searchRecommendationsDto.MeetingReferenceNo);
                filter = _filterHelper.Combine(filter, MeetingRefCondition);
            }
            if (!string.IsNullOrEmpty(searchRecommendationsDto.Title))
            {
                Expression<Func<MeetingAgendaRecommendation, bool>> TitleCondition = x => x.Text.Contains(searchRecommendationsDto.Title);
                filter = _filterHelper.Combine(filter, TitleCondition);
            }
            if (searchRecommendationsDto.FromDate != null)
            {
                Expression<Func<MeetingAgendaRecommendation, bool>> FromDateCondition = x => x.CreatedAt >= searchRecommendationsDto.FromDate;
                filter = _filterHelper.Combine(filter, FromDateCondition);
            }
            if (searchRecommendationsDto.ToDate != null)
            {
                Expression<Func<MeetingAgendaRecommendation, bool>> ToDateCondition = x => x.CreatedAt <= searchRecommendationsDto.ToDate;
                filter = _filterHelper.Combine(filter, ToDateCondition);
            }
            var recommendations = await _mmsUnitOfWork.MeetingAgendaRecommendations.
                ListAsyncIncludeUserAndMeetingAndStatus(filter, Page, PageSize);
            var data= recommendations.Select(x => _mapper.Map<MeetingAgendaRecommendationFollowUpListItemDto>((x, language))).OrderByDescending(x => x.Id).ToList();
			var count = await _mmsUnitOfWork.MeetingAgendaRecommendations.CountAsync(filter);

			return new GenericPaginationListDto<MeetingAgendaRecommendationFollowUpListItemDto>(count, data);
			
			
		}

        public async Task<GenericPaginationListDto<MeetingAgendaRecommendationFollowUpListItemDto>?> ListMeetingAgendaRecommendationsForHub(string? email, string? username, SearchRecommendationsDto searchRecommendationsDto, LanguageDbEnum language, int page, int pageSize)
        {
            var user = await _userManagementUnitOfWork.Users.GetAsync(x => x.Email == email || x.Username == username);
            if (user == null)
            {
                return new GenericPaginationListDto<MeetingAgendaRecommendationFollowUpListItemDto>(0, new List<MeetingAgendaRecommendationFollowUpListItemDto>());
            }
            return await ListMeetingAgendaRecommendations(searchRecommendationsDto, user.Id, language, page, pageSize);
        }
        public async Task<GenericPaginationListDto<RecommendationNoteListItemDto>> ListRecommendationNotes(int recommendationId, int Page, int PageSize)
        {
            var count = await _mmsUnitOfWork.RecommendationNotes.CountAsync(x => x.RecommendationId == recommendationId);
            var notes = await _mmsUnitOfWork.RecommendationNotes.ListWithPaginationAsync(
                  x => x.RecommendationId == recommendationId, Page: Page, PageSize: PageSize);
            var data = notes.Select(x => _mapper.Map<RecommendationNoteListItemDto>(x)).ToList();
            return new GenericPaginationListDto<RecommendationNoteListItemDto>(count, data);
        }
        public async Task<bool> AddRecommendationNote(RecommendationNoteListItemDto recommendationNoteListItemDto)
        {
            var RecomendationNote = _mapper.Map<RecommendationNote>(recommendationNoteListItemDto);
            RecomendationNote.CreatedAt = DateTime.Now;
            await _mmsUnitOfWork.RecommendationNotes.AddAsync(RecomendationNote);
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }
        public async Task<bool> UpdateRecommendationNote(RecommendationNoteListItemDto recommendationNoteListItemDto)
        {
            var RecomendationNote = await _mmsUnitOfWork.RecommendationNotes.Find(recommendationNoteListItemDto.Id);
            if(RecomendationNote != null&& RecomendationNote.Text!= recommendationNoteListItemDto.Text)
            {
				RecomendationNote.Text = recommendationNoteListItemDto.Text;
			}
			return await _mmsUnitOfWork.SaveChangesAsync() > 0;

		}
		public async Task<bool> DeleteRecommendationNote(int recommendationNoteId)
        {
            var RecomendationNote = await _mmsUnitOfWork.RecommendationNotes.GetAsync(x => x.Id == recommendationNoteId);
            _mmsUnitOfWork.RecommendationNotes.Remove(RecomendationNote);
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddRecommendationAttachments(int recommendationId, IFormFileCollection files, string userId)
        {
            var attachmentsToAdd = new List<Attachment>();
            string recommendationDirectory = StorageFactory.GetRecommendationDirectory(recommendationId);
            for (int i = 0; i < files.Count; i++)
            {
                string fileRelativeUrl = $"{recommendationDirectory}{Guid.NewGuid()}";
                attachmentsToAdd.Add(new Attachment
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    FileName = files[i].FileName,
                    FileRelativeUrl = fileRelativeUrl,
                    FileSize = files[i].ToBytes().Length,
                    RecordId = recommendationId,
                    RecordTypeId = (int)AttachmentRecordTypeDbEnum.AgendaRecommendation,
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
            return true;
		}
        public async Task<bool> DeleteRecommendationAttachment(int AttachmentId)
        {
            var attachment = await _mmsUnitOfWork.Attachments.GetAsync(x => x.Id == AttachmentId);
            _mmsUnitOfWork.Attachments.Remove(attachment);

            return await _mmsUnitOfWork.SaveChangesAsync() > 0; ;
        }

        public async Task<GenericPaginationListDto<AttachmentListItemDto>?> ListRecommendationAttachments(int recommendationId, int page, int pageSize)
        {
            var count = await _mmsUnitOfWork.Attachments.CountAsync(x => x.RecordId == recommendationId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.AgendaRecommendation);
            var attachments = await _mmsUnitOfWork.Attachments.ListWithPaginationAsync(x => x.RecordId == recommendationId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.AgendaRecommendation, Page: page, PageSize: pageSize);

            var data = attachments.Select(_mapper.Map<AttachmentListItemDto>).ToList();
            return new GenericPaginationListDto<AttachmentListItemDto>(count, data);


        }

        public async Task<bool> UpdateRecommendationProgress(UpdateRecommendationProgressDto updateRecommendationProgressDto)
        {
            
            var recomendation = await _mmsUnitOfWork.MeetingAgendaRecommendations.GetAsync(x => x.Id == updateRecommendationProgressDto.Id);
            recomendation.Percentage = updateRecommendationProgressDto.Progress;
            recomendation.StatusId = updateRecommendationProgressDto.StatusId== (int)MeetingAgendaRecommendationStatusDbEnum.Draft ? recomendation.StatusId: updateRecommendationProgressDto.StatusId;
			
            if (updateRecommendationProgressDto.StatusId == (int)MeetingAgendaRecommendationStatusDbEnum.Completed||
				updateRecommendationProgressDto.Progress==100)
			{
				recomendation.Percentage = 100;
                recomendation.StatusId =(int) MeetingAgendaRecommendationStatusDbEnum.Completed;

			}
			_mmsUnitOfWork.MeetingAgendaRecommendations.Update(recomendation);
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<MeetingAgendaRecommendationFollowUpListItemDto?> GetRecommendation(int recommendationId, LanguageDbEnum language)
        {
            var recommendations = await _mmsUnitOfWork.MeetingAgendaRecommendations.ListAsyncIncludeUserAndMeetingAndStatus(x => x.Id == recommendationId, 1, 1);
            var recommendation = recommendations.FirstOrDefault();
            if (recommendation == null) return null;
            return _mapper.Map<MeetingAgendaRecommendationFollowUpListItemDto>((recommendation, language));
        }

        public async Task<bool> HasViewAccess(string userId, int recommendationId)
        {
            var hasAccess= await _mmsUnitOfWork.MeetingAgendaRecommendations.AnyAsync(x =>
                    x.Id == recommendationId
					&& (x.Owner==userId ||x.CreateBy==userId));
            if (hasAccess) {
                return true;
            }
            int? councilCommitteeId = await _mmsUnitOfWork.MeetingAgendaRecommendations.GetCommitteeId(recommendationId);
            if (!councilCommitteeId.HasValue)// no need to check for committee permission for user
            {
                return false;
            }
			return await _mmsUnitOfWork.CommitteePermissions.AnyAsync(x => x.UserId == userId &&x.CommitteeId== councilCommitteeId&& x.PermissionId == (int)CommitteePermissionDbEnum.CommitteeRecommendation);
		}
		public async Task<bool> HasWriteAccess(string userId, int recommendationId)
		{
			return await _mmsUnitOfWork.MeetingAgendaRecommendations.AnyAsync(x =>
					x.Id == recommendationId
					&& x.Owner == userId);

		}
		public async Task<bool> HasAccessToAttachment(string userId, int recommendationAttachmentId)
		{
			return await _mmsUnitOfWork.Attachments.AnyAsync(x=>x.Id==recommendationAttachmentId && x.CreatedBy==userId);
		}
		public async Task<bool> HasAccessToNote(string userId, int noteId)
		{
			var note = await _mmsUnitOfWork.RecommendationNotes.Find(noteId);
			if (note == null)
			{
				return false;
			}
			return await _mmsUnitOfWork.MeetingAgendaRecommendations.AnyAsync(x =>
					x.Id == note.RecommendationId
					&& x.Owner == userId );
		}

 
    }
}
