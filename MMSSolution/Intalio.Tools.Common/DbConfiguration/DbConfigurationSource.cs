using Microsoft.Extensions.Configuration;

namespace Intalio.Tools.Common.DbConfiguration
{
	internal class DbConfigurationSource : IConfigurationSource
	{
		private readonly string? _connectionString;

		public DbConfigurationSource(string connectionString) => _connectionString = connectionString;

		public IConfigurationProvider Build(IConfigurationBuilder builder) => new DbConfigurationProvider(_connectionString);
	}
}
