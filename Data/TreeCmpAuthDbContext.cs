using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TreeCmpWebAPI.Data
{
    public class TreeCmpAuthDbContext: IdentityDbContext
    {

        public TreeCmpAuthDbContext(DbContextOptions<TreeCmpAuthDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var readerId = "9b894882-8bbe-4686-a583-e8ce71733073";
            var writterId = "21f69fee-3324-4346-8745-7be32e7d49e1";


            var roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = readerId,
                    ConcurrencyStamp = readerId,
                    Name = "Reader",
                    NormalizedName = "Reader".ToUpper()
                },
                new IdentityRole
                {
                    Id =writterId,
                    ConcurrencyStamp = writterId,
                    Name = "Writter",
                    NormalizedName = "Writter".ToUpper()
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);  
        }
    }
}
