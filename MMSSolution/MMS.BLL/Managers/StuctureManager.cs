using MapsterMapper;
using Microsoft.Extensions.Configuration;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Structures;
using MMS.DTO.Users;
using Task = System.Threading.Tasks.Task;

namespace MMS.BLL.Managers
{
    public class StuctureManager
    {
        private readonly IMapper _mapper;

        private readonly IUserManagementUnitOfWork _userManagementUnitOfWork;
        private readonly IConfiguration _configuration;
        public StuctureManager(IMapper mapper, IUserManagementUnitOfWork userManagementUnitOfWork, IConfiguration configuration)
        {
            _mapper = mapper;
            _userManagementUnitOfWork = userManagementUnitOfWork;
            _configuration = configuration;
        }

        public async Task CreateStructureAsync(StructureDto strutureObject)
        {
            Structure structure = new()
            {
                NameAr = strutureObject.NameAr,
                NameEn = strutureObject.NameEn,
                Description = strutureObject.Description,
                CreatedDate = DateTime.Now,
                Active = strutureObject.Active,
                Code = strutureObject.Code,
                ParentId = strutureObject.ParentId,
                TypeId = strutureObject.TypeId,
                ExternalStructure = strutureObject.ExternalStructure,
                BranchId = strutureObject.BranchId,
            };
            await _userManagementUnitOfWork.Structures.AddAsync(structure);
            await _userManagementUnitOfWork.SaveChangesAsync();
        }

        public async Task<List<TreeviewListItemDto>?> ListOrganizationStructuresAsync(LanguageDbEnum language, bool onlyActive)
        {

            var organization = await _userManagementUnitOfWork.Structures.ListAsync(x => x.Active == onlyActive ? true : false);
            string? applicationName = _configuration.GetValue<string>(Constants.AppSettingsConstants.ApplicationName);

            if (organization != null)
            {
                List<TreeviewListItemDto> list = new List<TreeviewListItemDto>();
                var rootItem = new TreeviewListItemDto
                {
                    Name = applicationName,
                    Children = BindTree(organization, null, new List<TreeviewListItemDto>(), language)
                };
                list.Add(rootItem);
                return list;
            }
            return null;
        }

        private List<TreeviewListItemDto>? BindTree(IEnumerable<Structure> structures, int? parentId, List<TreeviewListItemDto> retVal, LanguageDbEnum language)
        {
            var nodes = parentId == null ? structures.Where(x => x.ParentId == null) : structures.Where(x => x.ParentId == parentId);

            if (nodes != null && nodes.Any())
            {
                retVal.AddRange(nodes.Select(x => new TreeviewListItemDto
                {
                    Id = x.Id.ToString(),
                    Name = language == LanguageDbEnum.Arabic ? x.NameAr : x.NameEn,
                    Children = BindTree(structures, x.Id, new List<TreeviewListItemDto>(), language)
                }).ToList());
            }
            return retVal;
        }

        public async Task<List<UserListItemDto>?> ListUsersInStructureAsync(int structureId, LanguageDbEnum language)
        {
            var userStructures = await _userManagementUnitOfWork.UserStructures.ListIncludeAllAsync(x => x.StrucutreId == structureId);
            if (userStructures != null)
            {
                return userStructures.Select(x => _mapper.Map<UserListItemDto>((x, language))).ToList();
            }
            return null;
        }

        public async Task AddUserToStructureAsync(int structureId, string userId, int roleId)
        {
            var structure = await _userManagementUnitOfWork.Structures.GetIncludeUserAsync(x => x.Id == structureId);
            if (structure != null)
            {
                structure.UserStructures.Add(new UserStructure { StrucutreId = structureId, UserId = userId, RoleId = roleId });
                await _userManagementUnitOfWork.SaveChangesAsync();
            }
        }

        public async Task RemoveUserFromStructureAsync(int structureId, string userId)
        {
            var structure = await _userManagementUnitOfWork.Structures.GetIncludeUserAsync(x => x.Id == structureId);
            if (structure != null)
            {
                var userStructure = structure.UserStructures.FirstOrDefault(x => x.UserId == userId);
                if (userStructure != null)
                {
                    _userManagementUnitOfWork.UserStructures.Remove(userStructure);
                    await _userManagementUnitOfWork.SaveChangesAsync();
                }
            }
        }

        public async Task<StructureDto?> GetStructureAsync(int structureId)
        {
            var structure = await _userManagementUnitOfWork.Structures.GetAsync(x => x.Id == structureId);
            if (structure != null)
            {
                return _mapper.Map<StructureDto>(structure);
            }
            return null;
        }

        public async Task UpdateStructureAsync(int structureId, StructureDto structureObj, string userId)
        {
            var structure = await _userManagementUnitOfWork.Structures.GetAsync(x => x.Id == structureId);
            if (structure != null)
            {
                structure.NameAr = structureObj.NameAr;
                structure.NameEn = structureObj.NameEn;
                structure.Description = structureObj.Description;
                structure.ParentId = structureObj.ParentId;
                structure.Active = structureObj.Active;
                structure.ExternalStructure = structureObj.ExternalStructure;
                structure.BranchId = structureObj.BranchId;
                await _userManagementUnitOfWork.SaveChangesAsync();
            }
        }
        public async Task DeleteStructureAsync(int structureId)
        {
            var structure = await _userManagementUnitOfWork.Structures.Find(structureId);
            _userManagementUnitOfWork.Structures.Remove(structure);
            await _userManagementUnitOfWork.SaveChangesAsync();

        }
        public List<ListItemDto>? ListRolesInStructureAsync(int structureId, LanguageDbEnum language)
        {
            var roles = _userManagementUnitOfWork.Structures.ListRolesInStructure(structureId);
            if (roles != null)
            {
                return roles.Select(x => _mapper.Map<ListItemDto>((x, language))).ToList();
            }
            return null;
        }
    }
}
