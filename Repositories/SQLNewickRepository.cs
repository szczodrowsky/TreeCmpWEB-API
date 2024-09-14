using Microsoft.EntityFrameworkCore;
using TreeCmpWebAPI.Data;
using TreeCmpWebAPI.Models.Domain;

namespace TreeCmpWebAPI.Repositories
{
    public class SQLNewickRepository : INewickRepositories
    {
        private readonly NewickDbContext dbContext;

        public SQLNewickRepository(NewickDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public async Task<List<Newick>> GetAllAsync()
        {
            return await dbContext.Newicks.ToListAsync();
        }

        public async Task<Newick?> GetByIdAsync(Guid id)
        {
            return await dbContext.Newicks.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Newick> CreateAsync(Newick newick)
        {
            await dbContext.Newicks.AddAsync(newick);
            await dbContext.SaveChangesAsync();
            return newick;
        }

        public async Task<Newick?> UpdateAsync(Guid id, Newick newick)
        {
            var existingNewick = await dbContext.Newicks.FirstOrDefaultAsync(x => x.Id == id);
            if (existingNewick == null)
            {
                return null;
            }

            existingNewick.comparisionMode = newick.comparisionMode;
            existingNewick.comparisionMode = newick.comparisionMode;
            existingNewick.newickFirstString = newick.newickFirstString;
            existingNewick.newickSecondString = newick.newickSecondString;
            existingNewick.windowWidth = newick.windowWidth;
            existingNewick.rootedMetrics = newick.rootedMetrics;
            existingNewick.unrootedMetrics = newick.unrootedMetrics;
            existingNewick.normalizedDistances = newick.normalizedDistances;
            existingNewick.pruneTrees = newick.pruneTrees;
            existingNewick.includeSummary = newick.includeSummary;
            existingNewick.zeroWeightsAllowed = newick.zeroWeightsAllowed;
            existingNewick.bifurcationTreesOnly = newick.bifurcationTreesOnly;

            await dbContext.SaveChangesAsync();
            return existingNewick;
        }



        public async Task <Newick?>DeleteAsync(Guid id)
        {
            var existingNewick = await dbContext.Newicks.FirstOrDefaultAsync(x => x.Id == id);
            
            if (existingNewick == null)
            {
                return null;
            }

            dbContext.Newicks.Remove(existingNewick);
            await dbContext.SaveChangesAsync();
            return existingNewick;
        }
    } 
}
