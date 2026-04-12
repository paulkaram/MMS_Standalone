using MapsterMapper;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.CommitteeRoles;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{

    public class CommitteeRoleManager
	{
        private readonly IMapper _mapper;
        private readonly ISettingsUnitOfWork _settingsUnitOfWork;

        public CommitteeRoleManager(IMapper mapper, ISettingsUnitOfWork settingsUnitOfWork)
        {
            _mapper = mapper;
            _settingsUnitOfWork = settingsUnitOfWork;
        }

        public async Task CreateRole(CommitteeRoleDto roleObj)
        {
			CommitteeRole role = new()
            {
                NameAr = roleObj.NameAr,
                NameEn = roleObj.NameEn,

            };
            await _settingsUnitOfWork.CommitteeRoles.AddAsync(role);
            await _settingsUnitOfWork.SaveChangesAsync();
        }

        public async Task<ListItemDto?> GetRoleById(int roleId, LanguageDbEnum language)
        {
            var role = await _settingsUnitOfWork.CommitteeRoles.GetAsync(x => x.Id == roleId);
            if (role != null)
            {
                return _mapper.Map<ListItemDto?>((role, language));
            }
            return null;
        }

        public async Task<List<CommitteeRoleListItemDto?>?> ListCommitteeRoles()
        {
            var roles = await _settingsUnitOfWork.CommitteeRoles.ListAsync();
            if (roles != null)
            {
                return roles.Select(x => _mapper.Map<CommitteeRoleListItemDto?>(x)).ToList();
            }
            return null;
        }

        public async Task RemoveByCommitteeRoleId(int roleId)
        {
            var role = await _settingsUnitOfWork.CommitteeRoles.GetAsync(x => x.Id == roleId);
            if (role != null)
            {
                _settingsUnitOfWork.CommitteeRoles.Remove(role);
                await _settingsUnitOfWork.SaveChangesAsync();
            }
        }

        public async Task UpdateRole(int roleId, CommitteeRoleDto roleObj)
        {
            var role = await _settingsUnitOfWork.CommitteeRoles.GetAsync(x => x.Id == roleId);
            if (role != null)
            {
                role.NameAr = roleObj.NameAr;
                role.NameEn = roleObj.NameEn;
            
                await _settingsUnitOfWork.SaveChangesAsync();
            }
        }
    }
}
