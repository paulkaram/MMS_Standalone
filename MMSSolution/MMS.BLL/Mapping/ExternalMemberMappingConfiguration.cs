using Mapster;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.ExternalMembers;

namespace MMS.BLL.Mapping
{
    internal class ExternalMemberMappingConfiguration : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<(ExternalMember member, int CommitteesCount), ExternalMemberDto>()
                .Map(dest => dest.Id, src => src.member.Id)
                .Map(dest => dest.FullnameAr, src => src.member.FullnameAr)
                .Map(dest => dest.FullnameEn, src => src.member.FullnameEn)
                .Map(dest => dest.Email, src => src.member.Email)
                .Map(dest => dest.Mobile, src => src.member.Mobile)
                .Map(dest => dest.Organization, src => src.member.Organization)
                .Map(dest => dest.Position, src => src.member.Position)
                .Map(dest => dest.IsActive, src => src.member.IsActive)
                .Map(dest => dest.CreatedDate, src => src.member.CreatedDate)
                .Map(dest => dest.CommitteesCount, src => src.CommitteesCount);

            config.NewConfig<(ExternalMember member, LanguageDbEnum Language), ListItemDto>()
                .Map(dest => dest.Id, src => src.member.Id)
                .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.member.FullnameAr : src.member.FullnameEn);

            config.NewConfig<(CommitteeExternalMember link, LanguageDbEnum Language), CommitteeExternalMemberDto>()
                .Map(dest => dest.Id, src => src.link.Id)
                .Map(dest => dest.CommitteeId, src => src.link.CommitteeId)
                .Map(dest => dest.ExternalMemberId, src => src.link.ExternalMemberId)
                .Map(dest => dest.MemberName, src => src.link.ExternalMember != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.link.ExternalMember.FullnameAr : src.link.ExternalMember.FullnameEn)
                    : string.Empty)
                .Map(dest => dest.Email, src => src.link.ExternalMember != null ? src.link.ExternalMember.Email : string.Empty)
                .Map(dest => dest.Organization, src => src.link.ExternalMember != null ? src.link.ExternalMember.Organization : null)
                .Map(dest => dest.Position, src => src.link.ExternalMember != null ? src.link.ExternalMember.Position : null)
                .Map(dest => dest.CommitteeRoleId, src => src.link.CommitteeRoleId)
                .Map(dest => dest.CommitteeRoleName, src => src.link.CommitteeRole != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.link.CommitteeRole.NameAr : src.link.CommitteeRole.NameEn)
                    : null)
                .Map(dest => dest.Active, src => src.link.Active)
                .Map(dest => dest.Note, src => src.link.Note)
                .Map(dest => dest.CreatedDate, src => src.link.CreatedDate);
        }
    }
}
