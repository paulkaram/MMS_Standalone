using Mapster;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO.Committees;
using MMS.DTO.Reports;

namespace MMS.BLL.Mapping
{
    internal class CommitteeMappingConfiguration : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
			config.NewConfig<(Committee comittee, LanguageDbEnum language), ComitteeSummaryReportDto>()
				  .Map(dest => dest.Id, src => src.comittee.Id)
				  .Map(dest => dest.MembersCount, src => src.comittee.UserCommittees.Count)
				  .Map(dest => dest.IsActive, src => src.comittee.Active)
				  .Map(dest => dest.StartDate, src => src.comittee.StartDate)
				  .Map(dest => dest.EndDate, src => src.comittee.EndDate)
				  .Map(dest => dest.Name, src =>src.language== LanguageDbEnum.Arabic? src.comittee.NameAr : src.comittee.NameEn);

            config.NewConfig<(CommitteeActivity comitteeActivity, LanguageDbEnum language), ActivitiesCommitteeListItemDto>()
                  .Map(dest => dest.Id, src => src.comitteeActivity.Id)
                  .Map(dest => dest.Title, src => src.comitteeActivity.Title)
                  .Map(dest => dest.Description, src => src.comitteeActivity.Description);

        }
	}
}
