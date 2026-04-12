using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Intalio.Tools.Common.Extensions.FileExtensions;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MMS.BLL.Constants;
using MMS.BLL.Storage;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.CommitteePermissions;
using MMS.DTO.Committees;
using MMS.DTO.CouncilCommittees;
using MMS.DTO.Meetings;
using MMS.DTO.Permissions;
using MMS.DTO.Users;
using System.Collections.Generic;
using System.Linq.Expressions;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class CouncilCommitteeManager
    {
        private readonly IMapper _mapper;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;
        private readonly ISettingsUnitOfWork _settingsUnitOfWork;
        private readonly IConfiguration _configuration;
        private readonly StorageManager _storageManager;


        public CouncilCommitteeManager(IMapper mapper, IMMSUnitOfWork mmsUnitOfWork, IConfiguration configuration, ISettingsUnitOfWork settingsUnitOfWork, StorageManager storageManager)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
            _configuration = configuration;
            _settingsUnitOfWork = settingsUnitOfWork;
            _storageManager = storageManager;

        }

        public async Task AddCommitteeAsync(CommitteeDto committee, Microsoft.AspNetCore.Http.IFormFileCollection files, string userId)
        {
            Committee newCommittee = new()
            {
                NameAr = committee.NameAr,
                NameEn = committee.NameEn,
                Description = committee.Description,
                CreatedDate = DateTime.Now,
                Active = committee.Active,
                Code = committee.Code,
                ParentId = committee.ParentId,
                TypeId = committee.TypeId,
                CommitteeClassificationId = committee.CommitteeClassificationId,
                CommitteeStyleId = committee.CommitteeStyleId,
                CommitteeStatusId = committee.CommitteeStatusId,
                StartDate = committee.StartDate,
                EndDate = committee.EndDate,
                HasAdditionalMembers = committee.HasAdditionalMembers,
                AdditionalMemberName = committee.AdditionalMemberName,
                IsInternal = committee.IsInternal,
                IsPresentationRelated = committee.IsPresentationRelated


            };
            await _mmsUnitOfWork.Committees.AddAsync(newCommittee);

            
            Attachment? attachment = null;
            if (files.Count > 0)
            {
                string committeeFileDirectory = StorageFactory.GetCommitteeCreationFilesDirectory(newCommittee.Id);
                string fileRelativeUrl = $"{committeeFileDirectory}{Guid.NewGuid()}";
                attachment = new Attachment
                {
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    FileName = files[0].FileName,
                    FileRelativeUrl = fileRelativeUrl,
                    FileSize = files[0].ToBytes().Length,
                    RecordId = newCommittee.Id,
                    RecordTypeId = (int)AttachmentRecordTypeDbEnum.CommitteeCreation,
                    Title = files[0].FileName,
                    Version = 1,
                };
                await _mmsUnitOfWork.Attachments.AddAsync(attachment);
            }
            await _mmsUnitOfWork.SaveChangesAsync();

            if (attachment != null)
            {
                await _storageManager.SaveToStorage(files[0].ToBytes(), attachment.Id, attachment.FileRelativeUrl);

            }



        }
        public async Task UpdateAsync(int councilCommitteeId, CommitteeDto committee, Microsoft.AspNetCore.Http.IFormFileCollection files, string userId)
        {
            var committeeToUpdate = await _mmsUnitOfWork.Committees.GetAsync(x => x.Id == councilCommitteeId);
            if (committeeToUpdate != null)
            {
                committeeToUpdate.NameAr = committee.NameAr;
                committeeToUpdate.NameEn = committee.NameEn;
                committeeToUpdate.Code = committee.Code;
                committeeToUpdate.Description = committee.Description;
                committeeToUpdate.ParentId = committee.ParentId;
                committeeToUpdate.Active = committee.Active;
                committeeToUpdate.TypeId = committee.TypeId;
                committeeToUpdate.StartDate = committee.StartDate;
                committeeToUpdate.EndDate = committee.EndDate;
                committeeToUpdate.CommitteeClassificationId = committee.CommitteeClassificationId;
                committeeToUpdate.CommitteeStyleId = committee.CommitteeStyleId;
                committeeToUpdate.CommitteeStatusId = committee.CommitteeStatusId;
                committeeToUpdate.HasAdditionalMembers = committee.HasAdditionalMembers;
                committeeToUpdate.AdditionalMemberName = committee.AdditionalMemberName;
                committeeToUpdate.IsInternal = committee.IsInternal;
                committeeToUpdate.IsPresentationRelated = committee.IsPresentationRelated;
            }
            if (files.Count > 0)
            {
                string committeeFileDirectory = StorageFactory.GetCommitteeCreationFilesDirectory(councilCommitteeId);
                string fileRelativeUrl = $"{committeeFileDirectory}{Guid.NewGuid()}";

                var attachment = await _mmsUnitOfWork.Attachments.GetAsync(x => x.RecordId == councilCommitteeId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.CommitteeCreation);
                if (attachment != null)
                {
                    attachment.CreatedBy = userId;
                    attachment.FileSize = files[0].ToBytes().Length;
                    attachment.Title = files[0].FileName;
                    attachment.Version++;
                    attachment.CreatedDate = DateTime.Now;
                    attachment.FileName = files[0].FileName;
                    attachment.FileRelativeUrl = fileRelativeUrl;


                }
                else
                {
                    attachment = new Attachment
                    {
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        FileName = files[0].FileName,
                        FileRelativeUrl = fileRelativeUrl,
                        FileSize = files[0].ToBytes().Length,
                        RecordId = councilCommitteeId,
                        RecordTypeId = (int)AttachmentRecordTypeDbEnum.CommitteeCreation,
                        Title = files[0].FileName,
                        Version = 1,
                    };
                    await _mmsUnitOfWork.Attachments.AddAsync(attachment);

                }
                if (attachment != null)
                {
                    await _storageManager.SaveToStorage(files[0].ToBytes(), attachment.Id, attachment.FileRelativeUrl);

                }

            }
            await _mmsUnitOfWork.SaveChangesAsync();

        }

        public async Task AddCommitteeDutyAsync(int councilCommitteeId, CommitteeDutyDto committeeDutyObj)
        {
            CommitteeDuty committeeDuty = new()
            {
                CommitteeId = councilCommitteeId,
                Title = committeeDutyObj.Title,
                Description = committeeDutyObj.Description,
                IsDeleted = false
            };
            await _mmsUnitOfWork.CommitteeDuty.AddAsync(committeeDuty);
            await _mmsUnitOfWork.SaveChangesAsync();
        }

        public async Task AddCommitteeActivityAsync(int councilCommitteeId, CommitteeDutyDto committeeDutyObj)
        {
            CommitteeActivity committeeActivity = new()
            {
                CommitteeId = councilCommitteeId,
                Title = committeeDutyObj.Title,
                Description = committeeDutyObj.Description,
                IsDeleted = false
            };
            await _mmsUnitOfWork.CommitteeActivity.AddAsync(committeeActivity);
            await _mmsUnitOfWork.SaveChangesAsync();
        }

        public async Task<(bool, string)> AddUserToCommitteeAsync(CommitteeUserPostDro committeeUserPostDro, LanguageDbEnum language)
        {
            var exist = await _mmsUnitOfWork.UserCommittee.AnyAsync(x =>
                    x.UserId == committeeUserPostDro.UserId &&
                    x.CommitteeId == committeeUserPostDro.CommitteeId);
            if (exist)
            {
                var existIn = await _settingsUnitOfWork.Dictionary.GetAsync(
                         x => x.Keyword == DictionaryConstansts.ExistInCommittee);
                return (false, language == LanguageDbEnum.Arabic ? existIn.Ar : existIn.En);

            }
            var committee = await _mmsUnitOfWork.Committees.GetIncludeUserAsync(x => x.Id == committeeUserPostDro.CommitteeId);
            if (committee != null)
            {
                var userCommitteesCount = await _mmsUnitOfWork.UserCommittee.CountAsync(x => x.UserId == committeeUserPostDro.UserId);
                int? maxCount = _configuration.GetValue<int>(AppSettingsConstants.MaxCommitteesForUser);
                if (userCommitteesCount >= maxCount)
                {
                    var exceedMax = await _settingsUnitOfWork.Dictionary.GetAsync(
                         x => x.Keyword == DictionaryConstansts.ExceedMaxCommittees);
                    return (false, language == LanguageDbEnum.Arabic ? exceedMax.Ar : exceedMax.En);
                }
                var committeeToAdd = _mapper.Map<UserCommittee>(committeeUserPostDro);
                committee.UserCommittees.Add(committeeToAdd);
                await _mmsUnitOfWork.SaveChangesAsync();
                return (true, "");
            }
            var notFound = await _settingsUnitOfWork.Dictionary.GetAsync(x => x.Keyword == DictionaryConstansts.NotFound);
            return (false, language == LanguageDbEnum.Arabic ? notFound.Ar : notFound.En);

        }

        public async Task<CommitteeDto?> GetAsync(int councilCommitteeId)
        {
            var committee = await _mmsUnitOfWork.Committees.GetAsync(x => x.Id == councilCommitteeId);
            if (committee != null)
            {
                var committeeDto = _mapper.Map<CommitteeDto>(committee);
                var creationAttachment = await _mmsUnitOfWork.Attachments.GetAsync(x => x.RecordId == committee.Id && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.CommitteeCreation);
                if (creationAttachment != null)
                {
                    committeeDto.fileName = creationAttachment.FileName;
                }
                return committeeDto;
            }
            return null;
        }

        public async Task<List<CommitteeDutyListItemDto>?> ListCommitteeDutiesAsync(int councilCommitteeId)
        {
            var committeeDuties = await _mmsUnitOfWork.CommitteeDuty.ListAsync(x => x.CommitteeId == councilCommitteeId && x.IsDeleted == false);
            return committeeDuties.Select(x => _mapper.Map<CommitteeDutyListItemDto>(x)).ToList();
        }

        public async Task<List<CommitteeDutyListItemDto>?> ListCommitteeActivitiesAsync(int councilCommitteeId)
        {
            var committeeActivities = await _mmsUnitOfWork.CommitteeActivity.ListAsync(x => x.CommitteeId == councilCommitteeId && x.IsDeleted == false);
            return committeeActivities.Select(x => _mapper.Map<CommitteeDutyListItemDto>(x)).ToList();
        }

        public async Task<List<CommitteeListItemDto>?> ListCommitteesAsync(LanguageDbEnum language)
        {
            var committees = await _mmsUnitOfWork.Committees.ListAsync();
            return committees.Select(x => _mapper.Map<CommitteeListItemDto>((x, language))).ToList();

        }

        public async Task<List<TreeviewListItemDto>?> ListCommitteeStructuresAsync(LanguageDbEnum language, bool onlyActive)
        {
            string? applicationName = _configuration.GetValue<string>(Constants.AppSettingsConstants.ApplicationName);

            // Always fetch all items - frontend will filter by IsActive
            var committeeStructure = await _mmsUnitOfWork.Committees.ListAsync(x => true);

            if (committeeStructure != null && committeeStructure.Any())
            {
                List<TreeviewListItemDto> list = new List<TreeviewListItemDto>();
                var rootItem = new TreeviewListItemDto
                {
                    Name = applicationName,
                    IsActive = true,
                    Children = BindTree(committeeStructure, null, new List<TreeviewListItemDto>(), language)
                };
                list.Add(rootItem);
                return list;
            }
            return null;
        }

        public async Task<List<TreeviewListItemDto>?> ListCommitteeStructuresForUserAsync(LanguageDbEnum language, bool onlyActive, List<int> committeeIds)
        {
            if (committeeIds == null || !committeeIds.Any())
                return null;

            string? applicationName = _configuration.GetValue<string>(Constants.AppSettingsConstants.ApplicationName);

            var allCommittees = await _mmsUnitOfWork.Committees.ListAsync(x => true);

            if (allCommittees == null || !allCommittees.Any())
                return null;

            // Find the admin committees and all their ancestors (parents) for proper tree hierarchy
            var relevantIds = new HashSet<int>(committeeIds);
            var committeeLookup = allCommittees.ToDictionary(c => c.Id);

            // Walk up the parent chain for each admin committee to include parent councils
            foreach (var id in committeeIds)
            {
                var current = committeeLookup.GetValueOrDefault(id);
                while (current?.ParentId != null)
                {
                    relevantIds.Add(current.ParentId.Value);
                    current = committeeLookup.GetValueOrDefault(current.ParentId.Value);
                }
            }

            var filteredCommittees = allCommittees.Where(c => relevantIds.Contains(c.Id)).ToList();

            List<TreeviewListItemDto> list = new List<TreeviewListItemDto>();
            var rootItem = new TreeviewListItemDto
            {
                Name = applicationName,
                IsActive = true,
                Children = BindTree(filteredCommittees, null, new List<TreeviewListItemDto>(), language)
            };
            list.Add(rootItem);
            return list;
        }

        public async Task<GenericPaginationListDto<MeetingAgendaRecommendationFollowUpListItemDto>?> ListCommitteeTasks(int councilCommitteeId, int page, int pageSize, LanguageDbEnum language)
        {
            var tasks = await _mmsUnitOfWork.MeetingAgendaRecommendations.ListAsyncIncludeUserAndMeetingAndStatus(
                filter: x => x.MeetingAgenda.Meeting.CommitteeId == councilCommitteeId, page: page,
                pageSize: pageSize);
            var count = await _mmsUnitOfWork.MeetingAgendaRecommendations.CountAsync(x => x.MeetingAgenda.Meeting.CommitteeId == councilCommitteeId);
            var data = tasks.Select(x => _mapper.Map<MeetingAgendaRecommendationFollowUpListItemDto>((x, language))).ToList();

            return new GenericPaginationListDto<MeetingAgendaRecommendationFollowUpListItemDto>(count, data);
        }
        public async Task<GenericPaginationListDto<MeetingListItemDto>?> ListCommitteeMeetings(int councilCommitteeId, int page, int pageSize, LanguageDbEnum language)
        {
            Expression<Func<Meeting, bool>> filter = x => x.CommitteeId == councilCommitteeId && x.StatusId != (int)MeetingStatusDbEnum.Draft;
            var meetings = await _mmsUnitOfWork.Meetings.ListIncludeCommitteeAndStatusAsync(filter, page, pageSize);
            var count = await _mmsUnitOfWork.Meetings.CountAsync(filter);

            var data = meetings.Select(x => _mapper.Map<MeetingListItemDto>((x, language))).ToList();
            return new GenericPaginationListDto<MeetingListItemDto>(count, data);


        }
        public async Task<List<ListItemDto>?> ListUserCommitteesAsync(string userId, LanguageDbEnum language)
        {
            var userCommittees = await _mmsUnitOfWork.UserCommittee.ListIncludeCommitteeAsync(x => x.UserId == userId);
            if (userCommittees != null)
            {
                return userCommittees.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
            }
            return null;
        }
        public async Task<List<CommitteeListItemDto>?> ListUserCommitteesForMeeting(string userId, LanguageDbEnum language)
        {
            var userCommittees = await _mmsUnitOfWork.CommitteePermissions.ListIncludeCommitteeAsync(x => x.UserId == userId && x.PermissionId == (int)CommitteePermissionDbEnum.CommitteeAddMeeting);
            if (userCommittees != null)
            {
                return userCommittees.Select(x => _mapper.Map<CommitteeListItemDto>((x, language))).ToList();
            }
            return null;
        }
        public async Task<List<CommitteeListItemDto>?> ListUserCommitteesForListingAsync(string userId, LanguageDbEnum language)
        {
            var userCommittees = await _mmsUnitOfWork.UserCommittee.ListIncludeCommitteeAsync(x => x.UserId == userId);
            if (userCommittees != null)
            {
                return userCommittees.Select(x => _mapper.Map<CommitteeListItemDto>((x, language))).ToList();
            }
            return null;
        }
        public async Task<List<ComitteesGeneralInfoListItemDto>?> ListUserCommitteesByCouncil(int councilCommitteeId, string userId, LanguageDbEnum language)
        {
            //TODO  enahnce all distinct queries and enehance method readability and logic
            var isSuperAdmin = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(p => p.UserId == userId && p.PermissionId == (int)PermissionDbEnum.SuperAdmin);
            var classifications = (await _mmsUnitOfWork.PermissionMatrices.ListIncludePermission(x => x.UserId == userId && x.Permission.MapId != null))
                                    .Select(x => x.Permission.MapId);
            HashSet<int> userCommitteesPemittedIds = new HashSet<int>();
            if (!isSuperAdmin)
            {
                var committeesIds = (await _mmsUnitOfWork.CommitteePermissions.ListAsync(x =>
                    x.UserId == userId &&
                    (x.PermissionId == (int)CommitteePermissionDbEnum.CommitteeUsers ||
                    x.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetings ||
                    x.PermissionId == (int)CommitteePermissionDbEnum.CommitteeRecommendation)
                    ))
                    .Select(x => x.CommitteeId).ToList();

                if (classifications.Count() > 0)
                {
                    List<int> ids = await _mmsUnitOfWork.Committees.GetIdsByClassifications(classifications);
                    committeesIds.AddRange(ids);
                }
                foreach (int committeId in committeesIds)
                {
                    userCommitteesPemittedIds.Add(committeId);
                }
            }
            var userCommittees = (await _mmsUnitOfWork.UserCommittee.ListIncludeCommitteeAsync(
                x => (isSuperAdmin || x.UserId == userId || userCommitteesPemittedIds.Contains(x.CommitteeId)) && x.Committee.ParentId == councilCommitteeId)).DistinctBy(x => x.CommitteeId).ToList();


            var comittees = userCommittees.Select(x => _mapper.Map<ComitteesGeneralInfoListItemDto>((x, language))).ToList();

            List<Committee> parents = new List<Committee>();
            if (comittees != null && comittees.Count > 0)
            {
                var parentId = comittees.FirstOrDefault().ParentId;
                while (parentId != null)
                {
                    var comittee = await _mmsUnitOfWork.Committees.GetAsync(x => x.Id == parentId);
                    parents.Insert(0, comittee);
                    parentId = comittee.ParentId;
                }
            }
            foreach (var item in comittees)
            {
                item.ChildernsCount = await _mmsUnitOfWork.Committees.CountAsync(x => x.ParentId == item.Id && (isSuperAdmin || x.UserCommittees.Any(x => x.UserId == userId)));
                item.HasChilds = item.ChildernsCount > 0;
                item.ShowDetails = isSuperAdmin || userCommitteesPemittedIds.Contains(item.Id);


            }
            foreach (var item in comittees)
            {
                item.Parents = parents.Select(x => new ListItemDto(x.Id.ToString(), language == LanguageDbEnum.Arabic ? x.NameAr : x.NameEn)).ToList();

            }
            return comittees;
        }

        public async Task<List<ListItemDto>> GetCommitteeParents(int councilCommitteeId, LanguageDbEnum language)
        {
            var committee = await _mmsUnitOfWork.Committees.GetAsync(x => x.Id == councilCommitteeId);
            List<Committee> parents = new List<Committee>();

            if (committee != null)
            {
                parents.Add(committee);

                var parentId = committee.ParentId;
                while (parentId != null)
                {
                    var comittee = await _mmsUnitOfWork.Committees.GetAsync(x => x.Id == parentId);
                    parents.Insert(0, comittee);
                    parentId = comittee.ParentId;
                }
            }
            return parents.Select(x => new ListItemDto(x.Id.ToString(), language == LanguageDbEnum.Arabic ? x.NameAr : x.NameEn)).ToList();

        }

        public async Task<List<ComitteesGeneralInfoListItemDto>> listUserCouncilsAndCommitteesForGeneralInfo(string userId, LanguageDbEnum language, CouncilsAndCommitteesSearchCriteriaPostDto SearchCriteriaPostDto)
        {
            var isSuperAdmin = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(p => p.PermissionId == (int)PermissionDbEnum.SuperAdmin && p.UserId == userId);
            var classifications =(await _mmsUnitOfWork.PermissionMatrices.ListIncludePermission(x => x.UserId == userId && x.Permission!=null && x.Permission.MapId != null)).Select(x => x.Permission.MapId);
            HashSet<int> userCommitteesPemittedIds =new HashSet<int>();
            if (!isSuperAdmin)
            {
                var committeesIds = (await _mmsUnitOfWork.CommitteePermissions.ListAsync(x =>
                    x.UserId == userId &&
                    (x.PermissionId == (int)CommitteePermissionDbEnum.CommitteeUsers ||
                    x.PermissionId == (int)CommitteePermissionDbEnum.CommitteeMeetings ||
                    x.PermissionId == (int)CommitteePermissionDbEnum.CommitteeRecommendation)
                    ))
                    .Select(x => x.CommitteeId).ToList();
                
                if (classifications.Count()>0) {
                   List<int> ids=await _mmsUnitOfWork.Committees.GetIdsByClassifications(classifications);
                    committeesIds.AddRange(ids);
                }
                foreach (int committeId in committeesIds)
                {
                    userCommitteesPemittedIds.Add(committeId);
                }
            }
            bool includeAllSearchCriteria = !string.IsNullOrEmpty(SearchCriteriaPostDto.MeetingTitle)
                || !string.IsNullOrEmpty(SearchCriteriaPostDto.RecommendationTitle)
                || !string.IsNullOrEmpty(SearchCriteriaPostDto.AgendaTitle)
                || !string.IsNullOrEmpty(SearchCriteriaPostDto.AgendaTopicTitle)
                || !string.IsNullOrEmpty(SearchCriteriaPostDto.AgendaNote) ? true : false;

            var userCommittees = (await _mmsUnitOfWork.UserCommittee.ListIncludeCommitteeAsync(x => 
            (isSuperAdmin || x.UserId == userId || classifications.Contains(x.Committee.CommitteeClassificationId)) 
            && x.Committee.ParentId == null
                && (
               String.IsNullOrEmpty(SearchCriteriaPostDto.CommitteeTitle)
               || x.Committee.NameAr.Contains(SearchCriteriaPostDto.CommitteeTitle)
               || x.Committee.NameEn.ToLower().Contains(SearchCriteriaPostDto.CommitteeTitle.ToLower()))))
                   .DistinctBy(x => x.CommitteeId);

            if (includeAllSearchCriteria)
            {
                var meetings = (!String.IsNullOrEmpty(SearchCriteriaPostDto.RecommendationTitle)
                    || !String.IsNullOrEmpty(SearchCriteriaPostDto.AgendaTitle)
                    || !String.IsNullOrEmpty(SearchCriteriaPostDto.AgendaTopicTitle)
                    || !String.IsNullOrEmpty(SearchCriteriaPostDto.AgendaNote))
                    ? //if true
                    await _mmsUnitOfWork.Meetings.ListIncludeCommitteeAndUserAndAgendaAndAgendaTopicAndAgendaNotesAndRecommendationAsync(x =>
                    x.Committee.ParentId == null && x.Committee.UserCommittees.Any(y => isSuperAdmin || y.UserId == userId) &&
                    (
                        String.IsNullOrEmpty(SearchCriteriaPostDto.AgendaTitle) ||
                        x.MeetingAgenda.Any(i => i.Title.ToLower().Contains(SearchCriteriaPostDto.AgendaTitle.ToLower()))
                    )
                    &&
                    (
                        String.IsNullOrEmpty(SearchCriteriaPostDto.AgendaTopicTitle) ||
                        x.MeetingAgenda.Any(i => i.AgendaTopics.Any(y => y.Text.ToLower().Contains(SearchCriteriaPostDto.AgendaTopicTitle.ToLower())))
                    )
                    &&
                    (
                        String.IsNullOrEmpty(SearchCriteriaPostDto.AgendaNote) ||
                        x.MeetingAgenda.Any(i => i.MeetingAgendaNotes.Any(y => y.Text.ToLower().Contains(SearchCriteriaPostDto.AgendaNote.ToLower())))
                    )
                    &&
                    (
                        String.IsNullOrEmpty(SearchCriteriaPostDto.RecommendationTitle) ||
                        x.MeetingAgenda.Any(i => i.MeetingAgendaRecommendations.Any(y => y.Text.ToLower().Contains(SearchCriteriaPostDto.RecommendationTitle.ToLower())))
                    )
                    &&
                    (
                    String.IsNullOrEmpty(SearchCriteriaPostDto.MeetingTitle)
                    || x.Title.Contains(SearchCriteriaPostDto.MeetingTitle))
                    )
                    //else
                    : await _mmsUnitOfWork.Meetings.ListAsync(x => string.IsNullOrEmpty(SearchCriteriaPostDto.MeetingTitle)
                    || x.Title.ToLower().Contains(SearchCriteriaPostDto.MeetingTitle.ToLower()));

                userCommittees = userCommittees
                    .Where(uc =>
                     meetings.Select(x => x.CommitteeId).Contains(uc.CommitteeId)).ToList();
            }
            var comittees = userCommittees.Select(x => _mapper.Map<ComitteesGeneralInfoListItemDto>((x, language))).ToList();
            foreach (var item in comittees)
            {
                item.ChildernsCount = await _mmsUnitOfWork.Committees.CountAsync(x => x.ParentId == item.Id && x.UserCommittees.Any(x => isSuperAdmin || x.UserId == userId));
                item.HasChilds = item.ChildernsCount > 0;
                item.ShowDetails = isSuperAdmin || userCommitteesPemittedIds.Contains(item.Id);
            }
            return comittees;
        }

        public async Task<List<UserCommitteeListItemDto>?> ListUsersInCommitteeAsync(int councilCommitteeId, LanguageDbEnum language)
        {
            var userCommittee = await _mmsUnitOfWork.UserCommittee.ListIncludeAllAsync(x => x.CommitteeId == councilCommitteeId);
            if (userCommittee != null)
            {
                return userCommittee.Select(x => _mapper.Map<UserCommitteeListItemDto>((x, language))).ToList();
            }
            return null;
        }
        public async Task<bool> UpdateCommitteeUser(UpdateCommitteeUserDto updateCommitteeUserDto, int CommitteeId)
        {

            var user = await _mmsUnitOfWork.UserCommittee.GetAsync(x =>
                x.CommitteeId == CommitteeId &&
                x.UserId == updateCommitteeUserDto.UserId);
            var updatedUser = _mapper.Map<UserCommittee>(updateCommitteeUserDto);
            updatedUser.Active = updateCommitteeUserDto.Active;

            _mmsUnitOfWork.UserCommittee.Remove(user);
            await _mmsUnitOfWork.SaveChangesAsync();
            updatedUser.CommitteeId = CommitteeId;
            await _mmsUnitOfWork.UserCommittee.AddAsync(updatedUser);

            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<GenericPaginationListDto<UserCommitteeListItemDto>> ListUsersInCommitteeForGeneralInfoAsync(int councilCommitteeId, int page, int pageSize, LanguageDbEnum language)
        {
            var userCommittee = await _mmsUnitOfWork.UserCommittee.ListIncludeUserAndRolePaginatedAsync(x => x.CommitteeId == councilCommitteeId, page, pageSize);
            var data = userCommittee.Select(x => _mapper.Map<UserCommitteeListItemDto>((x, language))).ToList();
            var count = await _mmsUnitOfWork.UserCommittee.CountAsync(x => x.CommitteeId == councilCommitteeId);

            return new GenericPaginationListDto<UserCommitteeListItemDto>(count, data);
        }

        public async Task<GenericPaginationListDto<ActivitiesCommitteeListItemDto>> ListActivitiesInCommitteeForGeneralInfoAsync(int councilCommitteeId, int page, int pageSize, LanguageDbEnum language)
        {
            var activityCommittee = await _mmsUnitOfWork.CommitteeActivity.ListIncludeActivityPaginatedAsync(x => x.CommitteeId == councilCommitteeId, page, pageSize);
            var data = activityCommittee.Select(x => _mapper.Map<ActivitiesCommitteeListItemDto>((x, language))).ToList();
            var count = await _mmsUnitOfWork.CommitteeActivity.CountAsync(x => x.CommitteeId == councilCommitteeId);

            return new GenericPaginationListDto<ActivitiesCommitteeListItemDto>(count, data);
        }

        public async Task<GenericPaginationListDto<AttachmentListItemDto>> ListAttachmentsInCommitteeForGeneralInfoAsync(int councilCommitteeId, int page, int pageSize, LanguageDbEnum language)
        {
            var attachments = await _mmsUnitOfWork.Attachments.ListIncludePrivacyAndType(x => x.RecordId == councilCommitteeId &&
                    (x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.CommitteeCreation ||
                     x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.CommitteeFile));
            var data = attachments.Select(x => _mapper.Map<AttachmentListItemDto>((x, language))).ToList();
            
            var count = await _mmsUnitOfWork.Attachments.CountAsync(x => x.RecordId == councilCommitteeId);

            return new GenericPaginationListDto<AttachmentListItemDto>(count, data);
        }
        public async Task RemoveCommitteeDutyAsync(int dutyId)
        {
            var committeeDuty = await _mmsUnitOfWork.CommitteeDuty.GetAsync(x => x.Id == dutyId);
            if (committeeDuty != null)
            {
                committeeDuty.IsDeleted = true;
                await _mmsUnitOfWork.SaveChangesAsync();
            }
        }
        public async Task RemoveCommitteeActivityAsync(int activityId)
        {
            var committeeActivity = await _mmsUnitOfWork.CommitteeActivity.GetAsync(x => x.Id == activityId);
            if (committeeActivity != null)
            {
                committeeActivity.IsDeleted = true;
                await _mmsUnitOfWork.SaveChangesAsync();
            }
        }
        public async Task RemoveUserFromCommitteeAsync(int councilCommitteeId, string userId)
        {
            var committee = await _mmsUnitOfWork.Committees.GetIncludeUserAsync(x => x.Id == councilCommitteeId);
            if (committee != null)
            {
                var userStructure = committee.UserCommittees.FirstOrDefault(x => x.UserId == userId);
                if (userStructure != null)
                {
                    _mmsUnitOfWork.UserCommittee.Remove(userStructure);
                    var perms = await _mmsUnitOfWork.CommitteePermissions.ListWithTrackAsync(x => x.CommitteeId == councilCommitteeId && x.UserId == userId);
                    _mmsUnitOfWork.CommitteePermissions.RemoveRange(perms);
                    await _mmsUnitOfWork.SaveChangesAsync();
                }
            }
        }


        public async Task UpdateDutyAsync(int committeeDutyId, CommitteeDutyDto committeeDutyObj)
        {
            var committeeDuty = await _mmsUnitOfWork.CommitteeDuty.GetAsync(x => x.Id == committeeDutyId);
            if (committeeDuty != null)
            {
                committeeDuty.Title = committeeDutyObj.Title;
                committeeDuty.Description = committeeDutyObj.Description;
                await _mmsUnitOfWork.SaveChangesAsync();
            }
        }

        public async Task UpdateActivityAsync(int committeeActivityId, CommitteeDutyDto committeeDutyObj)
        {
            var committeeActivity = await _mmsUnitOfWork.CommitteeActivity.GetAsync(x => x.Id == committeeActivityId);
            if (committeeActivity != null)
            {
                committeeActivity.Title = committeeDutyObj.Title;
                committeeActivity.Description = committeeDutyObj.Description;
                await _mmsUnitOfWork.SaveChangesAsync();
            }
        }
        private List<TreeviewListItemDto>? BindTree(IEnumerable<Committee> committees, int? parentId, List<TreeviewListItemDto> retVal, LanguageDbEnum language)
        {
            var nodes = parentId == null ? committees.Where(x => x.ParentId == null) : committees.Where(x => x.ParentId == parentId);

            if (nodes != null && nodes.Any())
            {
                retVal.AddRange(nodes.Select(x => new TreeviewListItemDto
                {
                    Id = x.Id.ToString(),
                    Name = language == LanguageDbEnum.Arabic ? x.NameAr : x.NameEn,
                    TypeId = x.TypeId,
                    IsActive = x.Active,
                    Children = BindTree(committees, x.Id, new List<TreeviewListItemDto>(), language)
                }).ToList());
            }
            return retVal;
        }

        public async Task<PermissionListItemDto> ListUsersPermissionsInCommitteeAsync(int councilCommitteeId, string userId)
        {
            var permissions = await _mmsUnitOfWork.Permissions.ListIncludeTypeAsync(x => x.IsSpecific == false && x.TypeId == (int)PermissionTypeDbEnum.Committee);

            var userPermissions = await _mmsUnitOfWork.CommitteePermissions.ListAsync(x => x.UserId == userId && x.CommitteeId == councilCommitteeId);
            return new PermissionListItemDto
            {
                Items = permissions.OrderBy(x => x.GroupName).GroupBy(x => x.GroupName, (key, group) => new SecondLevelPermissionDto
                {
                    GroupName = key,
                    Items = group.OrderBy(x => x.GroupItemOrder).Select(per => new PermissionAccessListItemDto
                    {
                        Id = per.Id,
                        Name = per.Name,
                        HasLevel = per.ShowLevel,
                        GroupName = per.GroupName,
                        HasAccess = userPermissions.Any(p => p.PermissionId == per.Id),
                        LevelId = (int)PermissionLevelDbEnum.Write
                    }).ToList()
                }).ToList()
            };

        }

        public async Task<bool> UpdateUserPermissionsInCommitteeAsync(int councilCommitteeId, CommitteePermissionPutDto committeePermissionPutDto)
        {
            var existPermission = await _mmsUnitOfWork.CommitteePermissions.GetAsync(x =>
                x.UserId == committeePermissionPutDto.UserId &&
                x.PermissionId == committeePermissionPutDto.PermissionId &&
                x.CommitteeId == councilCommitteeId);
            if (committeePermissionPutDto.Enabled)//to Add
            {
                if (existPermission != null)//already exist
                {
                    return true;
                }
                await _mmsUnitOfWork.CommitteePermissions.AddAsync(new CommitteePermission()
                {
                    PermissionId = committeePermissionPutDto.PermissionId,
                    UserId = committeePermissionPutDto.UserId,
                    CommitteeId = councilCommitteeId,
                });
            }
            else //disable the permission
            {
                if (existPermission == null)//already not exist
                {
                    return true;
                }
                _mmsUnitOfWork.CommitteePermissions.Remove(existPermission);
            }
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<UserCommitteePermissions> GetUserCommitteePermission(int councilCommitteeId, string userId)
        {
            var isSuperAdmin = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(p => p.UserId == userId && p.PermissionId == (int)PermissionDbEnum.SuperAdmin);
            var classificationId = (await _mmsUnitOfWork.Committees.Find(councilCommitteeId)).CommitteeClassificationId;
            bool isCommittClassificationAdmin = false;
            if (classificationId != null)
            {
                isCommittClassificationAdmin = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(x => x.UserId == userId && x.Permission.MapId == classificationId);
            }
            List<int> userPerm = new List<int>();
            if (!isSuperAdmin)
            {
                userPerm = (await _mmsUnitOfWork.CommitteePermissions.ListAsync(x => x.UserId == userId && x.CommitteeId == councilCommitteeId)).Select(x => x.PermissionId).ToList();

            }
            return new UserCommitteePermissions(
                Users: isSuperAdmin || isCommittClassificationAdmin || userPerm.Contains((int)CommitteePermissionDbEnum.CommitteeUsers),
                Meetings: isSuperAdmin || isCommittClassificationAdmin || userPerm.Contains((int)CommitteePermissionDbEnum.CommitteeMeetings),
                Recommendations: isSuperAdmin || isCommittClassificationAdmin || userPerm.Contains((int)CommitteePermissionDbEnum.CommitteeRecommendation),
                MeetingsAttachments: isSuperAdmin || isCommittClassificationAdmin || userPerm.Contains((int)CommitteePermissionDbEnum.CommitteeMeetingAttachments),
                MeetingsVotingResults: isSuperAdmin || isCommittClassificationAdmin || userPerm.Contains((int)CommitteePermissionDbEnum.VotingResults),
                MeetingsMinutes: isSuperAdmin || isCommittClassificationAdmin || userPerm.Contains((int)CommitteePermissionDbEnum.CommitteeMeetingMinutes),
                CommitteeActivities: isSuperAdmin || isCommittClassificationAdmin || userPerm.Contains((int)CommitteePermissionDbEnum.CommitteeActivities),
                CommitteeAttachments: isSuperAdmin || isCommittClassificationAdmin || userPerm.Contains((int)CommitteePermissionDbEnum.CommitteeAttachments),
                CommitteeAttachmentButtonAdd: isSuperAdmin || isCommittClassificationAdmin || userPerm.Contains((int)CommitteePermissionDbEnum.CommitteeAttachmentButtonAdd)
                );
        }

        public async Task<bool> HasCommitteeAccess(int councilCommitteeId, string userId, CommitteePermissionDbEnum permission)
        {
            var isSuperAdmin = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(p => p.UserId == userId && p.PermissionId == (int)PermissionDbEnum.SuperAdmin);
            var classificationId = (await _mmsUnitOfWork.Committees.Find(councilCommitteeId)).CommitteeClassificationId;
            bool isCommittClassificationAdmin = false;
            if (classificationId != null)
            {
                isCommittClassificationAdmin = await _mmsUnitOfWork.PermissionMatrices.AnyAsync(x => x.UserId == userId && x.Permission.MapId == classificationId);
            }
            return isSuperAdmin || isCommittClassificationAdmin || await _mmsUnitOfWork.CommitteePermissions.AnyAsync(x =>
                x.CommitteeId == councilCommitteeId &&
                x.UserId == userId &&
                x.PermissionId == (int)permission);
        }

        public async Task<List<AttachmentListItemDto>?> ListCommitteeAttachments(int councilCommitteeId, LanguageDbEnum language)
        {
            var attachments = await _mmsUnitOfWork.Attachments.ListIncludePrivacyAndType(x => x.RecordId == councilCommitteeId &&
                    (x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.CommitteeCreation ||
                     x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.CommitteeFile));
            return attachments.Select(x => _mapper.Map<AttachmentListItemDto>((x, language))).ToList();
        }
        public async Task<List<AttachmentListItemDto>?> AddCommitteeAttachments(int councilCommitteeId, IFormFileCollection files, string userId, short privacyId, LanguageDbEnum language)
        {
            var attachmentsToAdd = new List<Attachment>();
            string meetingDirectory = StorageFactory.GetCommitteeFilesDirectory(councilCommitteeId);
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
                    RecordId = councilCommitteeId,
                    RecordTypeId = (int)AttachmentRecordTypeDbEnum.CommitteeFile,
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
            var attachments = await _mmsUnitOfWork.Attachments.ListIncludePrivacyAndType(x =>
            x.RecordId == councilCommitteeId && x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.CommitteeCreation ||
             x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.CommitteeFile);
            return attachments.Select(x => _mapper.Map<AttachmentListItemDto>((x, language))).ToList();
        }

        public async Task<bool> DeleteCommitteeAttachments(int attachmentId)
        {
            var attachment = await _mmsUnitOfWork.Attachments.GetAsync(x => x.Id == attachmentId &&
             (x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.CommitteeFile ||
             x.RecordTypeId == (int)AttachmentRecordTypeDbEnum.CommitteeCreation));
            _mmsUnitOfWork.Attachments.Remove(attachment);
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }

        public async Task<bool> SaveFinancialCompensation(int committteeId,bool hasFinancial)
        {
            var committee = await _mmsUnitOfWork.Committees.GetAsync(x => x.Id == committteeId);
            committee.HasFinancialCompensation = hasFinancial;
            //_mmsUnitOfWork.Attachments.Remove(attachment);
            return await _mmsUnitOfWork.SaveChangesAsync() > 0;
        }
        
        public async Task<bool> GetCommitteeFinancialCompensationAsync(int committteeId)
        {
            var committee = await _mmsUnitOfWork.Committees.GetAsync(x => x.Id == committteeId);
            return committee.HasFinancialCompensation;
        }

        private static readonly int[] AdminPermissionIds = new[]
        {
            (int)CommitteePermissionDbEnum.CommitteeAddMeeting,       // 29
            (int)CommitteePermissionDbEnum.CommitteeRecommendation,   // 32
            (int)CommitteePermissionDbEnum.CommitteeMeetings,         // 33
            (int)CommitteePermissionDbEnum.CommitteeUsers,            // 34
            (int)CommitteePermissionDbEnum.CommitteeMeetingAttachments,// 35
            (int)CommitteePermissionDbEnum.CommitteeMeetingMinutes,   // 37
            (int)CommitteePermissionDbEnum.VotingResults,             // 38
            (int)CommitteePermissionDbEnum.CommitteeActivities,       // 44
            (int)CommitteePermissionDbEnum.CommitteeAttachmentButtonAdd,// 45
            (int)CommitteePermissionDbEnum.CommitteeAttachments       // 46
        };

        public async Task<UserAdminCommitteesDto> GetUserAdminCommitteesAsync(string userId)
        {
            var userPermissions = await _mmsUnitOfWork.CommitteePermissions.ListAsync(x =>
                x.UserId == userId && AdminPermissionIds.Contains(x.PermissionId));

            // Group by committee and find committees where user has ALL admin permissions
            var adminCommitteeIds = userPermissions
                .GroupBy(x => x.CommitteeId)
                .Where(g => AdminPermissionIds.All(pid => g.Any(p => p.PermissionId == pid)))
                .Select(g => g.Key)
                .ToList();

            return new UserAdminCommitteesDto
            {
                UserId = userId,
                CommitteeIds = adminCommitteeIds
            };
        }

        public async Task<bool> UpdateBulkCommitteeAdminAsync(BulkCommitteeAdminDto dto)
        {
            var currentResult = await GetUserAdminCommitteesAsync(dto.UserId);
            var currentIds = new HashSet<int>(currentResult.CommitteeIds);
            var desiredIds = new HashSet<int>(dto.CommitteeIds);

            // Committees to add all admin permissions
            var toAdd = desiredIds.Except(currentIds).ToList();
            // Committees to remove all admin permissions
            var toRemove = currentIds.Except(desiredIds).ToList();

            // Remove permissions for removed committees
            if (toRemove.Count > 0)
            {
                var permsToRemove = await _mmsUnitOfWork.CommitteePermissions.ListWithTrackAsync(x =>
                    x.UserId == dto.UserId &&
                    toRemove.Contains(x.CommitteeId) &&
                    AdminPermissionIds.Contains(x.PermissionId));
                _mmsUnitOfWork.CommitteePermissions.RemoveRange(permsToRemove);
            }

            // Add permissions for new committees
            if (toAdd.Count > 0)
            {
                var newPermissions = new List<CommitteePermission>();
                foreach (var committeeId in toAdd)
                {
                    foreach (var permId in AdminPermissionIds)
                    {
                        newPermissions.Add(new CommitteePermission
                        {
                            UserId = dto.UserId,
                            CommitteeId = committeeId,
                            PermissionId = permId
                        });
                    }
                }
                await _mmsUnitOfWork.CommitteePermissions.AddRangeAsync(newPermissions);
            }

            return await _mmsUnitOfWork.SaveChangesAsync() >= 0;
        }
    }
}
