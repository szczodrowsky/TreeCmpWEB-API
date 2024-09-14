using TreeCmpWebAPI.Models.Domain;

namespace TreeCmpWebAPI.Repositories
{
    public interface INewickRepositories
    {
        Task<List<Newick>> GetAllAsync();

        Task<Newick?> GetByIdAsync(Guid id);

        Task<Newick> CreateAsync(Newick newick);
        
        Task<Newick?> UpdateAsync(Guid id, Newick newick);

        Task<Newick?> DeleteAsync(Guid id);

    }
}
