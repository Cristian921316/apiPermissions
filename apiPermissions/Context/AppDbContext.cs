using apiPermissions.Models;
using Microsoft.EntityFrameworkCore;

namespace apiPermissions.Context
{
	public class AppDbContext:DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<Permissions> Permissions { get; set; }
		public DbSet<PermissionType> PermissionTypes { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

		}



	}
}
