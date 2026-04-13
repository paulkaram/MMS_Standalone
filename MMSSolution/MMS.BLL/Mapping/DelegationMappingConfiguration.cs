using Mapster;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Delegations;

namespace MMS.BLL.Mapping
{
    internal class DelegationMappingConfiguration : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<(Delegation delegation, LanguageDbEnum Language, DateTime Now), DelegationDto>()
                .Map(dest => dest.Id, src => src.delegation.Id)
                .Map(dest => dest.FromUserId, src => src.delegation.FromUserId)
                .Map(dest => dest.FromUserName, src => src.delegation.FromUser != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.delegation.FromUser.FullnameAr : src.delegation.FromUser.FullnameEn)
                    : string.Empty)
                .Map(dest => dest.ToUserId, src => src.delegation.ToUserId)
                .Map(dest => dest.ToUserName, src => src.delegation.ToUser != null
                    ? (src.Language == LanguageDbEnum.Arabic ? src.delegation.ToUser.FullnameAr : src.delegation.ToUser.FullnameEn)
                    : string.Empty)
                .Map(dest => dest.TypeId, src => src.delegation.TypeId)
                .Map(dest => dest.TypeName, src => src.delegation.TypeId == (int)DelegationTypeDbEnum.General
                    ? "General"
                    : "TaskSpecific")
                .Map(dest => dest.StartDate, src => src.delegation.StartDate)
                .Map(dest => dest.EndDate, src => src.delegation.EndDate)
                .Map(dest => dest.IsActive, src => src.delegation.IsActive)
                .Map(dest => dest.IsCurrentlyActive, src => src.delegation.IsActive
                    && src.delegation.StartDate <= src.Now
                    && src.delegation.EndDate >= src.Now)
                .Map(dest => dest.Reason, src => src.delegation.Reason)
                .Map(dest => dest.TaskIds, src => src.delegation.DelegationTasks.Select(t => t.TaskId).ToList())
                .Map(dest => dest.CreatedDate, src => src.delegation.CreatedDate);
        }
    }
}
