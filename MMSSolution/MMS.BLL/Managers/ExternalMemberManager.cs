using MapsterMapper;
using MMS.BLL.Constants;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.ExternalMembers;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class ExternalMemberManager
    {
        private readonly IMapper _mapper;
        private readonly IMMSUnitOfWork _mmsUnitOfWork;

        public ExternalMemberManager(IMapper mapper, IMMSUnitOfWork mmsUnitOfWork)
        {
            _mapper = mapper;
            _mmsUnitOfWork = mmsUnitOfWork;
        }

        public async Task<List<ExternalMemberDto>> ListAdminAsync()
        {
            var members = await _mmsUnitOfWork.ExternalMembers.ListAsync();
            var memberships = (await _mmsUnitOfWork.CommitteeExternalMembers.ListAsync())
                .GroupBy(m => m.ExternalMemberId)
                .ToDictionary(g => g.Key, g => g.Count());

            return members
                .OrderBy(m => m.FullnameEn)
                .Select(m =>
                {
                    int count = memberships.TryGetValue(m.Id, out var c) ? c : 0;
                    return _mapper.Map<ExternalMemberDto>((m, count));
                })
                .ToList();
        }

        public async Task<List<ListItemDto>> ListAutoCompleteAsync(string? search, LanguageDbEnum language)
        {
            var all = await _mmsUnitOfWork.ExternalMembers.ListAsync(m => m.IsActive);
            var filtered = string.IsNullOrWhiteSpace(search)
                ? all
                : all.Where(m =>
                    m.FullnameAr.Contains(search) ||
                    m.FullnameEn.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    m.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
            return filtered.Select(m => _mapper.Map<ListItemDto>((m, language))).ToList();
        }

        public async Task<ExternalMemberDto> CreateAsync(ExternalMemberPostDto dto, string userId)
        {
            var existing = await _mmsUnitOfWork.ExternalMembers.GetByEmailAsync(dto.Email);
            if (existing != null)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var member = new ExternalMember
            {
                FullnameAr = dto.FullnameAr,
                FullnameEn = dto.FullnameEn,
                Email = dto.Email,
                Mobile = dto.Mobile,
                Organization = dto.Organization,
                Position = dto.Position,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.Now,
                CreatedBy = userId
            };

            await _mmsUnitOfWork.ExternalMembers.AddAsync(member);
            await _mmsUnitOfWork.SaveChangesAsync();

            return _mapper.Map<ExternalMemberDto>((member, 0));
        }

        public async Task<ExternalMemberDto?> UpdateAsync(int id, ExternalMemberPostDto dto)
        {
            var member = await _mmsUnitOfWork.ExternalMembers.GetAsync(m => m.Id == id);
            if (member == null) return null;

            member.FullnameAr = dto.FullnameAr;
            member.FullnameEn = dto.FullnameEn;
            member.Email = dto.Email;
            member.Mobile = dto.Mobile;
            member.Organization = dto.Organization;
            member.Position = dto.Position;
            member.IsActive = dto.IsActive;

            await _mmsUnitOfWork.SaveChangesAsync();

            int count = await _mmsUnitOfWork.CommitteeExternalMembers.CountAsync(c => c.ExternalMemberId == id);
            return _mapper.Map<ExternalMemberDto>((member, count));
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var member = await _mmsUnitOfWork.ExternalMembers.GetAsync(m => m.Id == id);
            if (member == null) return false;

            // Block delete if member is in any committee
            var inUse = await _mmsUnitOfWork.CommitteeExternalMembers.AnyAsync(c => c.ExternalMemberId == id);
            if (inUse) return false;

            _mmsUnitOfWork.ExternalMembers.Remove(member);
            await _mmsUnitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<List<CommitteeExternalMemberDto>> ListByCommitteeAsync(int committeeId, LanguageDbEnum language)
        {
            var links = await _mmsUnitOfWork.CommitteeExternalMembers.ListByCommitteeAsync(committeeId);
            return links
                .OrderBy(l => language == LanguageDbEnum.Arabic ? l.ExternalMember.FullnameAr : l.ExternalMember.FullnameEn)
                .Select(l => _mapper.Map<CommitteeExternalMemberDto>((l, language)))
                .ToList();
        }

        public async Task<CommitteeExternalMemberDto> AddToCommitteeAsync(CommitteeExternalMemberPostDto dto, LanguageDbEnum language)
        {
            var exists = await _mmsUnitOfWork.CommitteeExternalMembers.AnyAsync(c =>
                c.CommitteeId == dto.CommitteeId &&
                c.ExternalMemberId == dto.ExternalMemberId &&
                c.CommitteeRoleId == dto.CommitteeRoleId);
            if (exists)
                throw new InvalidOperationException(MessageConstants.ErrorOccured);

            var link = new CommitteeExternalMember
            {
                CommitteeId = dto.CommitteeId,
                ExternalMemberId = dto.ExternalMemberId,
                CommitteeRoleId = dto.CommitteeRoleId,
                Active = true,
                Note = dto.Note,
                CreatedDate = DateTime.Now
            };

            await _mmsUnitOfWork.CommitteeExternalMembers.AddAsync(link);
            await _mmsUnitOfWork.SaveChangesAsync();

            // reload with relations
            var reloaded = (await _mmsUnitOfWork.CommitteeExternalMembers.ListByCommitteeAsync(dto.CommitteeId))
                .First(l => l.Id == link.Id);
            return _mapper.Map<CommitteeExternalMemberDto>((reloaded, language));
        }

        public async Task<bool> RemoveFromCommitteeAsync(int linkId)
        {
            var link = await _mmsUnitOfWork.CommitteeExternalMembers.GetAsync(c => c.Id == linkId);
            if (link == null) return false;

            _mmsUnitOfWork.CommitteeExternalMembers.Remove(link);
            await _mmsUnitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
