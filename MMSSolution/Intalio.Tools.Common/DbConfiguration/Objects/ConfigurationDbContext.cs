using Microsoft.EntityFrameworkCore;

namespace Intalio.Tools.Common.DbConfiguration.Objects
{
	internal class ConfigurationDbContext : DbContext
	{
		private readonly string _connectionString;

		public DbSet<AppSettings> AppSettings => Set<AppSettings>();

		public ConfigurationDbContext(string? connectionString) => _connectionString = connectionString ?? "";

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer(_connectionString);

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<AppSettings>(entity =>
			{
				entity.HasKey(e => e.Name);
			});
		}
	}
}
