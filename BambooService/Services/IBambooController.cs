using BambooService.Models;

namespace BambooService.Services
{
    public interface IBambooController
    {
        public Task<List<Bamboo>> GetAllBambooAsync();
        public Task<Bamboo?> GetBambooByIdAsync(int id);
        public Task<object> GetTotalWeightAsync();
        public Task<object> HarvestBambooAsync(double weight);
        public Task<object> GetStatsAsync();
    }
}