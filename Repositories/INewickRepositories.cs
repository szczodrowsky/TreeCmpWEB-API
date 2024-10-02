using TreeCmpWebAPI.Models.Domain;
using TreeCmpWebAPI.Models.DTO;

namespace TreeCmpWebAPI.Repositories
{
    public interface INewickRepositories
    {
        Task<List<Newick>> GetAllAsync();

        Task<List<CombinedNewickData>> GetAllFinalRecordsAsync(string username);

        Task<Newick?> GetByIdAsync(Guid id);

        Task<Newick> CreateAsync(Newick newick);
        
        Task<Newick?> UpdateAsync(Guid id, Newick newick);

        Task<Newick?> DeleteAsync(Guid id);

        Task SaveOutputFileAsync(NewickResponseFile newickResponseFile);
       
    }
}
