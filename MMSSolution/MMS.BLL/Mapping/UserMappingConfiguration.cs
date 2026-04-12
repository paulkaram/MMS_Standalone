using Mapster;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Committees;
using MMS.DTO.Permissions;
using MMS.DTO.Structures;
using MMS.DTO.Users;
using MMS.DTO.Users.Auth;

namespace MMS.BLL.Mapping
{
    internal class UserMappingConfiguration : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<(User User, string Token, string RefreshToken), LoggedInUserDto>()
                .Map(dest => dest.Token, src => src.Token)
                .Map(dest => dest.RefreshToken, src => src.RefreshToken)
                .Map(dest => dest.User, src => new LoggedInUserInfoDto(
                    src.User.Id,
                    src.User.FullnameAr,
                    src.User.FullnameEn,
                    src.User.DefaultLanguage.Code,
                    src.User.Email,
                    src.User.Mobile,
                    src.User.NationalId,
                    src.User.HasProfilePicture));

            config.NewConfig<(UserStructure UserStructure, LanguageDbEnum Language), UserListItemDto>()
                .Map(dest => dest.Id, src => src.UserStructure.User.Id)
                .Map(dest => dest.Approved, src => src.UserStructure.User.Approved)
                .Map(dest => dest.Email, src => src.UserStructure.User.Email)
                .Map(dest => dest.Fullname, src => src.Language == LanguageDbEnum.Arabic ? src.UserStructure.User.FullnameAr : src.UserStructure.User.FullnameEn)
                .Map(dest => dest.RoleName, src => src.Language == LanguageDbEnum.Arabic ? src.UserStructure.Role.RoleNameAr : src.UserStructure.Role.RoleNameEn);

            config.NewConfig<(UserCommittee userCommittee, LanguageDbEnum Language), UserListItemDto>()
              .Map(dest => dest.Id, src => src.userCommittee.User.Id)
              .Map(dest => dest.Approved, src => src.userCommittee.User.Approved)
              .Map(dest => dest.Email, src => src.userCommittee.User.Email)
              .Map(dest => dest.Fullname, src => src.Language == LanguageDbEnum.Arabic ? src.userCommittee.User.FullnameAr : src.userCommittee.User.FullnameEn)
              .Map(dest => dest.RoleName, src => src.Language == LanguageDbEnum.Arabic ? src.userCommittee.CommitteeRole.NameAr : src.userCommittee.CommitteeRole.NameEn);

            config.NewConfig<(UserCommittee userCommittee, LanguageDbEnum Language), UserCommitteeListItemDto>()
              .Map(dest => dest.Id, src => src.userCommittee.User.Id)
              .Map(dest => dest.Email, src => src.userCommittee.User.Email)
              .Map(dest => dest.CommitteeRoleId, src => src.userCommittee.CommitteeRoleId)
              .Map(dest => dest.PrivacyId, src => src.userCommittee.PrivacyId)
              .Map(dest => dest.Note, src => src.userCommittee.Note)
              .Map(dest => dest.Active, src => src.userCommittee.Active)
              .Map(dest => dest.Fullname, src => src.Language == LanguageDbEnum.Arabic ? src.userCommittee.User.FullnameAr : src.userCommittee.User.FullnameEn)
              .Map(dest => dest.PrivacyName, src => src.Language == LanguageDbEnum.Arabic ? src.userCommittee.Privacy.NameAr : src.userCommittee.Privacy.Name)
              .Map(dest => dest.RoleName, src => src.Language == LanguageDbEnum.Arabic ? src.userCommittee.CommitteeRole.NameAr : src.userCommittee.CommitteeRole.NameEn);


            config.NewConfig<(UserCommittee userCommittee, LanguageDbEnum Language), ListItemDto>()
              .Map(dest => dest.Id, src => src.userCommittee.Committee.Id)
              .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.userCommittee.Committee.NameAr : src.userCommittee.Committee.NameEn);

            config.NewConfig < (CommitteePermission userCommittee, LanguageDbEnum Language), CommitteeListItemDto>()
			  .Map(dest => dest.Id, src => src.userCommittee.Committee.Id)
			  .Map(dest => dest.TypeId, src => src.userCommittee.Committee.TypeId)
			  .Map(dest => dest.Code, src => src.userCommittee.Committee.Code)
			  .Map(dest => dest.Description, src => src.userCommittee.Committee.Description)
			  .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.userCommittee.Committee.NameAr : src.userCommittee.Committee.NameEn);


			config.NewConfig<User, UserAdminListItemDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.FullnameAr, src => src.FullnameAr)
                .Map(dest => dest.FullnameEn, src => src.FullnameEn)
                .Map(dest => dest.Username, src => src.Username)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.Mobile, src => src.Mobile)
                .Map(dest => dest.NationalId, src => src.NationalId)
                .Map(dest => dest.DefaultLanguageId, src => src.DefaultLanguageId)
                .Map(dest => dest.SmsEnabled, src => src.SmsEnabled)
                .Map(dest => dest.EmailNotificationEnabled, src => src.EmailNotificationEnabled)
                .Map(dest => dest.Approved, src => src.Approved);


            config.NewConfig<(Structure structure, LanguageDbEnum Language), ListItemDto>()
                .Map(dest => dest.Id, src => src.structure.Id)
                .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.structure.NameAr : src.structure.NameEn);

            config.NewConfig<(Role role, LanguageDbEnum Language), ListItemDto>()
                .Map(dest => dest.Id, src => src.role.Id)
                .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.role.RoleNameAr : src.role.RoleNameEn);

            config.NewConfig<StructureType, ListItemDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name);

            config.NewConfig<Structure, StructureDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.NameAr, src => src.NameAr)
                .Map(dest => dest.NameEn, src => src.NameEn)
                .Map(dest => dest.Code, src => src.Code)
                .Map(dest => dest.TypeId, src => src.TypeId)
                .Map(dest => dest.ParentId, src => src.ParentId)
                .Map(dest => dest.Active, src => src.Active)
                .Map(dest => dest.ExternalStructure, src => src.ExternalStructure)
                .Map(dest => dest.Description, src => src.Description);

            config.NewConfig<UserSignature, ListItemDto>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => Convert.ToBase64String(src.Signature));

            config.NewConfig<(User User, LanguageDbEnum Language), ListItemDto>()
                .Map(dest => dest.Id, src => src.User.Id)
                .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.User.FullnameAr : src.User.FullnameEn);


            config.NewConfig<(UserStructure userStructure, LanguageDbEnum Language), UserStructureRoleLstItemDto>()
                .Map(dest => dest.RoleName, src => src.Language == LanguageDbEnum.Arabic ? src.userStructure.Role.RoleNameAr : src.userStructure.Role.RoleNameEn)
                .Map(dest => dest.IsPrimary, src => src.userStructure.IsPrimary)
                .Map(dest => dest.StructureName, src => src.Language == LanguageDbEnum.Arabic ? src.userStructure.Strucutre.NameAr : src.userStructure.Strucutre.NameEn);

			config.NewConfig<(UserCommittee userCommittee, LanguageDbEnum Language), ComitteesGeneralInfoListItemDto>()
			  .Map(dest => dest.Id, src => src.userCommittee.CommitteeId)
			  .Map(dest => dest.Code, src => src.userCommittee.Committee.Code)
			  .Map(dest => dest.HasChilds, src => false)
			  .Map(dest => dest.TypeId, src => src.userCommittee.Committee.TypeId)
			  .Map(dest => dest.ParentId, src => src.userCommittee.Committee.ParentId)
			  .Map(dest => dest.Description, src => src.userCommittee.Committee.Description)
			  .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.userCommittee.Committee.NameAr : src.userCommittee.Committee.NameEn);

			config.NewConfig<(UserCommittee userCommittee, LanguageDbEnum Language), CommitteeListItemDto>()
			  .Map(dest => dest.Id, src => src.userCommittee.CommitteeId)
			  .Map(dest => dest.Code, src => src.userCommittee.Committee.Code)
			  .Map(dest => dest.TypeId, src => src.userCommittee.Committee.TypeId)
			  .Map(dest => dest.Description, src => src.userCommittee.Committee.Description)
			  .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.userCommittee.Committee.NameAr : src.userCommittee.Committee.NameEn);

            config.NewConfig<Permission, SystemPermissionListItemDto>()
              .Map(dest => dest.TypeName, src => src.Type.Name);


		}
	}
}
