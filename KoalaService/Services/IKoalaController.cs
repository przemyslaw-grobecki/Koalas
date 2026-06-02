using Tellemetry.Models;

namespace Tellemetry.Services
{
    public interface IKoalaController
    {
        Task<List<Koala>> GetAllAliveKoalasAsync();
        Task<Koala?> GetAliveKoalaByIdAsync(int id);
        Task<object> FeedKoalaAsync(int id);
        Task<object> GetStatsAsync();
    }
}