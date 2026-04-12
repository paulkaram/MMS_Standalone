using Intalio.Tools.Common.DbConfiguration;
using Microsoft.Extensions.Configuration;

namespace Intalio.Tools.Common.Extensions
{
	public static class ConfigurationExtensions
	{
		/// <summary>
		/// Load configuration from database instead of appsettings.
		/// <para>The database must contains the table AppSettings(string Name, string? Value)</para>
		/// </summary>
		/// <param name="builder"></param>
		/// <param name="connectionString"></param>
		/// <returns></returns>
		public static IConfigurationBuilder AddDbConfiguration(this IConfigurationBuilder builder, string connectionString)
		{
			return builder.Add(new DbConfigurationSource(connectionString));
		}

	}
}
