using MMS.DAL.Core.UnitOfWork.AuditLogs;
using MMS.DAL.Core.UnitOfWork.MMS;
using MMS.DAL.Data.UnitOfWork.AuditLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MMS.DAL.Models.AuditLogs;
using MMS.DAL.Data.UnitOfWork.MMS;
using MMS.DAL.Models.MMS;
using MMS.DAL.Models.Chat;
using MMS.DAL.Core.UnitOfWork.Chats;
using MMS.DAL.Data.UnitOfWork.Chats;

namespace MMS.DAL
{
    public static class DependencyInjection
	{
        public const string MMSConnectionStringName = "MMS";
		public const string AuditTrailConnectionStringName = "AuditTrail";
		public const string ChatConnectionStringName = "Chat";

		public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
		{
			return services
				.AddIntalioMMSDI(configuration)
				.AddIntalioAuditTrailDI(configuration)
				.AddChatDI(configuration);
		}

		private static IServiceCollection AddIntalioMMSDI(this IServiceCollection services, IConfiguration configuration)
		{
			string? connectionString = configuration.GetConnectionString(MMSConnectionStringName);

			services.AddDbContext<MmsContext>(options =>
			{
				string? connectionString = configuration.GetConnectionString(MMSConnectionStringName);
				options.UseSqlServer(connectionString);
			});

			services.AddScoped<IProcessUnitOfWork, ProcessUnitOfWork>();
			services.AddScoped<ISettingsUnitOfWork, SettingUnitOfWork>();
			services.AddScoped<IUserManagementUnitOfWork, UserManagementUnitOfWork>();
			services.AddScoped<IMMSUnitOfWork, MMSUnitOfWork>();
			return services;
		}

		private static IServiceCollection AddIntalioAuditTrailDI(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<MomraAuditTrailContext>(options =>
			{
				string? connectionString = configuration.GetConnectionString(AuditTrailConnectionStringName);
				options.UseSqlServer(connectionString);
			});

			services.AddScoped<IAuditLogUnitOfWork, AuditLogsUnitOfWork>();

			return services;
		}

		private static IServiceCollection AddChatDI(this IServiceCollection services, IConfiguration configuration)
		{
			string? connectionString = configuration.GetConnectionString(ChatConnectionStringName);
			services.AddDbContext<InatalioChatContext>(options =>
			{
				options.UseSqlServer(connectionString);
			});

			services.AddScoped<IChatUnitOfWork, ChatUnitOfWork>();

			return services;
		}

	}
}
