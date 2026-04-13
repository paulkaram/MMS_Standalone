using MapsterMapper;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Delegations;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class DelegationManager
    {
        private readonly IMapper _mapper;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;

        public DelegationManager(IMapper mapper, IMMSUnitOfWork mmsUnitOfWork)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
        }

        public async Task<List<DelegationDto>> ListOutgoingAsync(string fromUserId, LanguageDbEnum language)
        {
            var items = await _mmsUnitOfWork.Delegations.ListByFromUserAsync(fromUserId);
            var now = DateTime.Now;
            return items.Select(d => _mapper.Map<DelegationDto>((d, language, now))).ToList();
        }

        public async Task<List<DelegationDto>> ListIncomingAsync(string toUserId, LanguageDbEnum language)
        {
            var items = await _mmsUnitOfWork.Delegations.ListByToUserAsync(toUserId);
            var now = DateTime.Now;
            return items.Select(d => _mapper.Map<DelegationDto>((d, language, now))).ToList();
        }

        public async Task<DelegationDto> CreateAsync(string fromUserId, DelegationPostDto dto, LanguageDbEnum language)
        {
            ValidatePostDto(fromUserId, dto);

            // For General delegations, no overlapping active period allowed
            if (dto.TypeId == (int)DelegationTypeDbEnum.General)
            {
                var overlap = await _mmsUnitOfWork.Delegations
                    .HasOverlappingGeneralAsync(fromUserId, dto.StartDate, dto.EndDate, null);
                if (overlap)
                    throw new InvalidOperationException(MessageConstants.ErrorOccured);
            }

            var delegation = new Delegation
            {
                FromUserId = fromUserId,
                ToUserId = dto.ToUserId,
                TypeId = dto.TypeId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true,
                Reason = dto.Reason,
                CreatedDate = DateTime.Now,
                CreatedBy = fromUserId
            };

            await _mmsUnitOfWork.Delegations.AddAsync(delegation);
            await _mmsUnitOfWork.SaveChangesAsync();

            if (dto.TypeId == (int)DelegationTypeDbEnum.TaskSpecific && dto.TaskIds.Any())
            {
                foreach (var taskId in dto.TaskIds.Distinct())
                {
                    await _mmsUnitOfWork.DelegationTasks.AddAsync(new DelegationTask
                    {
                        DelegationId = delegation.Id,
                        TaskId = taskId
                    });
                }
                await _mmsUnitOfWork.SaveChangesAsync();
            }

            return await GetAsync(delegation.Id, language)
                ?? throw new InvalidOperationException(MessageConstants.ErrorOccured);
        }

        public async Task<DelegationDto?> GetAsync(int id, LanguageDbEnum language)
        {
            var d = await _mmsUnitOfWork.Delegations.GetIncludeRelationsAsync(id);
            if (d == null) return null;
            return _mapper.Map<DelegationDto>((d, language, DateTime.Now));
        }

        public async Task<DelegationDto?> RevokeAsync(int id, string requestingUserId, LanguageDbEnum language)
        {
            var delegation = await _mmsUnitOfWork.Delegations.GetAsync(d => d.Id == id);
            if (delegation == null) return null;

            // Only the delegator can revoke
            if (delegation.FromUserId != requestingUserId)
                throw new UnauthorizedAccessException();

            delegation.IsActive = false;
            await _mmsUnitOfWork.SaveChangesAsync();

            return await GetAsync(id, language);
        }

        public async Task<bool> DeleteAsync(int id, string requestingUserId)
        {
            var delegation = await _mmsUnitOfWork.Delegations.GetAsync(d => d.Id == id);
            if (delegation == null) return false;

            if (delegation.FromUserId != requestingUserId)
                throw new UnauthorizedAccessException();

            _mmsUnitOfWork.Delegations.Remove(delegation);
            await _mmsUnitOfWork.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Returns the list of user IDs who have currently-active general delegations
        /// granting the requesting user their access. Used during permission resolution.
        /// </summary>
        public async Task<List<string>> GetEffectiveDelegatorsAsync(string toUserId)
        {
            var delegations = await _mmsUnitOfWork.Delegations.ListActiveGeneralForToUserAsync(toUserId, DateTime.Now);
            return delegations.Select(d => d.FromUserId).Distinct().ToList();
        }

        private static void ValidatePostDto(string fromUserId, DelegationPostDto dto)
        {
            if (string.IsNullOrEmpty(dto.ToUserId))
                throw new ArgumentException(MessageConstants.ErrorOccured);
            if (dto.ToUserId == fromUserId)
                throw new ArgumentException(MessageConstants.ErrorOccured);
            if (dto.EndDate < dto.StartDate)
                throw new ArgumentException(MessageConstants.ErrorOccured);
            if (dto.TypeId != (int)DelegationTypeDbEnum.General && dto.TypeId != (int)DelegationTypeDbEnum.TaskSpecific)
                throw new ArgumentException(MessageConstants.ErrorOccured);
            if (dto.TypeId == (int)DelegationTypeDbEnum.TaskSpecific && !dto.TaskIds.Any())
                throw new ArgumentException(MessageConstants.ErrorOccured);
        }
    }
}
