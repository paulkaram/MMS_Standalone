using Intalio.Tools.Common.DbConfiguration.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Intalio.Tools.Common.DbConfiguration
{
	internal class DbConfigurationProvider : ConfigurationProvider
	{
		//https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-configuration-provider

		private readonly string? _connectionString;

		public DbConfigurationProvider(string? connectionString) => _connectionString = connectionString;

		public override void Load()
		{
			using var dbContext = new ConfigurationDbContext(_connectionString);

			Data = dbContext.AppSettings.AsNoTracking().ToDictionary(c => c.Name, c => c.Value, StringComparer.OrdinalIgnoreCase);
		}

	}
}
