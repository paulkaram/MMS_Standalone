using Intalio.Tools.Common.Extensions.FileExtensions;
using Intalio.Tools.Common.Storage;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using MMS.BLL.Storage;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Sessions;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class SessionManager
    {
        private readonly IMapper _mapper;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;
        private readonly StorageManager _storageManager;

        public SessionManager(
            IMapper mapper,
            IMMSUnitOfWork mmsUnitOfWork,
            StorageManager storageManager)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
            _storageManager = storageManager;
        }

        public async Task<SessionDto> CreateSessionAsync(SessionPostDto dto, string userId, LanguageDbEnum language)
        {
            var session = new Session
            {
                ReferenceNumber = await GenerateReferenceNumber(dto.CommitteeId),
                ExternalReferenceNumber = dto.ExternalReferenceNumber,
                Subject = dto.Subject,
                Note = dto.Note,
                MeetingDate = dto.MeetingDate,
                DueDate = dto.DueDate,
                Tags = dto.Tags,
                CommitteeId = dto.CommitteeId,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };

            if (dto.SessionItems != null)
            {
                foreach (var item in dto.SessionItems)
                {
                    session.SessionItems.Add(new SessionItem
                    {
                        ExternalId = item.ExternalId,
                        Subject = item.Subject,
                        ItemTypeId = item.ItemTypeId,
                        Tags = item.Tags,
                        InternalNote = item.InternalNote,
                        RelatedSessionItemId = item.RelatedSessionItemId,
                        Order = item.Order
                    });
                }
            }

            await _mmsUnitOfWork.Sessions.AddAsync(session);
            await _mmsUnitOfWork.SaveChangesAsync();

            return await GetSessionAsync(session.Id, language);
        }

        public async Task<SessionDto> GetSessionAsync(int sessionId, LanguageDbEnum language)
        {
            var session = await _mmsUnitOfWork.Sessions.GetIncludeItemsAsync(x => x.Id == sessionId);
            if (session == null)
                return null!;

            return _mapper.Map<SessionDto>((session, language));
        }

        public async Task<List<ListItemDto>> ListPresentationRelatedCommitteesAsync(string userId, LanguageDbEnum language)
        {
            var userCommitteeIds = (await _mmsUnitOfWork.UserCommittee.ListAsync(x => x.UserId == userId && x.Active))
                .Select(x => x.CommitteeId)
                .ToHashSet();

            var committees = await _mmsUnitOfWork.Committees.ListAsync(x =>
                x.IsPresentationRelated &&
                x.Active &&
                (x.IsDeleted == null || x.IsDeleted == false) &&
                userCommitteeIds.Contains(x.Id));

            return committees.Select(c => new ListItemDto(
                c.Id.ToString(),
                language == LanguageDbEnum.Arabic ? c.NameAr : c.NameEn
            )).ToList();
        }

        public async Task<List<ListItemDto>> ListSessionItemTypesAsync(LanguageDbEnum language)
        {
            var types = await _mmsUnitOfWork.SessionItemTypes.ListAsync();
            return types.Select(t => new ListItemDto(
                t.Id.ToString(),
                language == LanguageDbEnum.Arabic ? t.NameAr : t.NameEn
            )).ToList();
        }

        public async Task<List<SessionItemDto>> SearchSessionItemsAsync(string searchText, LanguageDbEnum language)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<SessionItemDto>();

            var items = await _mmsUnitOfWork.SessionItems.ListAsync(x =>
                x.Subject.Contains(searchText) ||
                (x.ExternalId != null && x.ExternalId.Contains(searchText)) ||
                (x.Tags != null && x.Tags.Contains(searchText)));

            return items.Select(i => new SessionItemDto
            {
                Id = i.Id,
                ExternalId = i.ExternalId,
                Subject = i.Subject,
                ItemTypeId = i.ItemTypeId,
                Tags = i.Tags,
                InternalNote = i.InternalNote,
                RelatedSessionItemId = i.RelatedSessionItemId,
                Order = i.Order
            }).Take(20).ToList();
        }

        public async Task<List<AttachmentListItemDto>> AddSessionAttachmentsAsync(
            int sessionId, IFormFileCollection files, string userId, short privacyId, LanguageDbEnum language)
        {
            var attachmentsToAdd = new List<Attachment>();
            string sessionDirectory = StorageFactory.GetSessionDirectory(sessionId);

            for (int i = 0; i < files.Count; i++)
            {
                string fileRelativeUrl = $"{sessionDirectory}{Guid.NewGuid()}";
                attachmentsToAdd.Add(new Attachment
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    FileName = files[i].FileName,
                    FileRelativeUrl = fileRelativeUrl,
                    FileSize = files[i].ToBytes().Length,
                    RecordId = sessionId,
                    RecordTypeId = (int)AttachmentRecordTypeDbEnum.Session,
                    Title = files[i].FileName,
                    Version = 1,
                    PrivacyId = privacyId
                });
            }

            await _mmsUnitOfWork.Attachments.AddRangeAsync(attachmentsToAdd);
            await _mmsUnitOfWork.SaveChangesAsync();

            for (int i = 0; i < attachmentsToAdd.Count; i++)
            {
                var attachment = attachmentsToAdd[i];
                await _storageManager.SaveToStorage(files[i].ToBytes(), attachment.Id, attachment.FileRelativeUrl);
            }

            return await ListSessionAttachmentsAsync(sessionId, language);
        }

        public async Task<List<AttachmentListItemDto>> ListSessionAttachmentsAsync(int sessionId, LanguageDbEnum language)
        {
            var attachments = await _mmsUnitOfWork.Attachments.ListIncludePrivacyAndType(x =>
                x.RecordId == sessionId &&
                x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.Session &&
                !x.Deleted);

            return attachments.Select(x => _mapper.Map<AttachmentListItemDto>((x, language))).ToList();
        }

        public async Task DeleteSessionAttachmentAsync(int sessionId, int attachmentId)
        {
            var attachment = await _mmsUnitOfWork.Attachments.GetAsync(x =>
                x.Id == attachmentId &&
                x.RecordId == sessionId &&
                x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.Session);

            if (attachment != null)
            {
                attachment.Deleted = true;
                _mmsUnitOfWork.Attachments.Update(attachment);
                await _mmsUnitOfWork.SaveChangesAsync();
            }
        }

        public async Task<(string? Filename, byte[]? Bytes)> DownloadSessionAttachmentAsync(int sessionId, int attachmentId)
        {
            var attachment = await _mmsUnitOfWork.Attachments.GetAsync(x =>
                x.Id == attachmentId &&
                x.RecordId == sessionId &&
                x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.Session &&
                !x.Deleted);

            if (attachment != null)
            {
                var bytes = await _storageManager.GetFile(attachment.Id, attachment.FileRelativeUrl);
                return (attachment.FileName, bytes);
            }

            return (null, null);
        }

        private async Task<string> GenerateReferenceNumber(int committeeId)
        {
            int counter = await _mmsUnitOfWork.Sessions.CountAsync() + 1;
            return $"SES-{DateTime.Now.Year}-{committeeId}-{counter}";
        }
    }
}
