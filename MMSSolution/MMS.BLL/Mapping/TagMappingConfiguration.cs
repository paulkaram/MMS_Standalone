using Mapster;
using MMS.DAL.Enumerations;
using MMS.DAL.Models.MMS;
using MMS.DTO;
using MMS.DTO.Tags;

namespace MMS.BLL.Mapping
{
    internal class TagMappingConfiguration : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<(Tag tag, int UsageCount), TagDto>()
                .Map(dest => dest.Id, src => src.tag.Id)
                .Map(dest => dest.NameAr, src => src.tag.NameAr)
                .Map(dest => dest.NameEn, src => src.tag.NameEn)
                .Map(dest => dest.Color, src => src.tag.Color)
                .Map(dest => dest.CreatedDate, src => src.tag.CreatedDate)
                .Map(dest => dest.UsageCount, src => src.UsageCount);

            config.NewConfig<(Tag tag, LanguageDbEnum Language), ListItemDto>()
                .Map(dest => dest.Id, src => src.tag.Id)
                .Map(dest => dest.Name, src => src.Language == LanguageDbEnum.Arabic ? src.tag.NameAr : src.tag.NameEn);
        }
    }
}
