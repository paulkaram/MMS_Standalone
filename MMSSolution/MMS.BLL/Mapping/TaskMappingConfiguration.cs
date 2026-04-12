using Mapster;
using MMS.DAL.Enumerations;
using MMS.DTO.Tasks;

namespace MMS.BLL.Mapping
{
	internal class TaskMappingConfiguration : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.NewConfig<(DAL.Models.MMS.Task task, LanguageDbEnum language), MeetingTaskListItemDto>()
				.Map(dest => dest.Id, src => src.task.Id)
				.Map(dest => dest.MeetingId, src => src.task.MeetingId)
				.Map(dest => dest.MeetingReference, src => src.task.Meeting.ReferenceNumber)
				.Map(dest => dest.MeetingTitle, src => src.task.Meeting.Title)
				.Map(dest => dest.Type, src => src.language == LanguageDbEnum.Arabic ? src.task.Type.NameAr : src.task.Type.NameEn)
				.Map(dest => dest.Claimed, src => src.task.Claimed ?? false)
				.Map(dest => dest.TypeId, src => src.task.TypeId)
				.Map(dest => dest.IsDelayed, src => DateTime.Now > src.task.CreatedDate.AddHours(src.task.DueDate));
		}
	}
}
