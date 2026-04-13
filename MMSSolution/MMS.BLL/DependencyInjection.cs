using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using MMS.BLL.Common.Helpers;
using MMS.BLL.Common.Security;
using MMS.BLL.Managers;
using MMS.BLL.Storage;
using System.Reflection;

namespace MMS.BLL
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
		{
			var config = TypeAdapterConfig.GlobalSettings;
			config.Scan(Assembly.GetExecutingAssembly());

			services.AddSingleton(config);
			services.AddScoped<IMapper, ServiceMapper>();
			services.AddMemoryCache();

			// DCC Compliance: Data masking service for PII protection
			services.AddSingleton<IDataMaskingService, DataMaskingService>();

			services.AddScoped<AttachmentManager>();
			services.AddScoped<LookupManager>();
			services.AddScoped<SettingManager>();
			services.AddScoped<UserManagementManager>();
			services.AddScoped<DictionaryManager>();
			services.AddScoped<StuctureManager>();
			services.AddScoped<EmailNotificationManager>();
			services.AddScoped<DataSourceManager>();
			services.AddScoped<RoleManager>();
			services.AddScoped<SmsManager>();
			services.AddScoped<AuditLogsManager>();
			services.AddScoped<LogIntalioActivityManager>();
			services.AddScoped<EmailTemplatesManager>();
			services.AddScoped<StorageFactory>();
			services.AddScoped<StorageManager>();
			services.AddScoped<TaskMappingManager>();
			services.AddScoped<CouncilCommitteeManager>();
			services.AddScoped<MeetingManager>();
			services.AddScoped<MeetingTranscriptManager>();
			services.AddSingleton<IamNotificationClient>();
			services.AddScoped<MmsNotificationService>();
			services.AddScoped<ChatManager>();
			services.AddScoped<MeetingAgendaManager>();
			services.AddScoped<IFilterHelper,FilterHelper>();
			services.AddScoped<MeetingAgendaRecommendationsManager>();
			services.AddScoped<DashboardManager>();
			services.AddScoped<ReportsManager>();
			services.AddScoped<CommitteeRoleManager>();
			services.AddScoped<MomTemplateManager>();
			services.AddScoped<SessionManager>();
			services.AddScoped<CommitteeItemManager>();
			services.AddScoped<TagManager>();
			services.AddScoped<ExternalMemberManager>();


			return services;
		}
	}
}
