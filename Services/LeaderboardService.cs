using LeaderboardRedisMvc.Models;
using LeaderboardRedisMvc.Settings;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace LeaderboardRedisMvc.Services;

public class LeaderboardService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly string _key;

    public LeaderboardService(IConnectionMultiplexer redis, IOptions<RedisSettings> options)
    {
        _redis = redis;
        _key = options.Value.LeaderboardKey;
    }

    private IDatabase Db => _redis.GetDatabase();

    public async Task<double> AddScoreAsync(string player, double points)
    {
        var newScore = await Db.SortedSetIncrementAsync(_key, player, points);

        // Invalida o perfil cacheado: score/rank mudaram, próxima leitura deve recalcular.
        await Db.KeyDeleteAsync(PlayerProfileService.CacheKey(player));

        return newScore;
    }

    public async Task<List<LeaderboardEntry>> GetTopAsync(int count)
    {
        var entries = await Db.SortedSetRangeByRankWithScoresAsync(_key, 0, count - 1, Order.Descending);

        var result = new List<LeaderboardEntry>();
        for (var i = 0; i < entries.Length; i++)
        {
            result.Add(new LeaderboardEntry
            {
                Player = entries[i].Element!,
                Score = entries[i].Score,
                Rank = i + 1,
            });
        }

        return result;
    }

    public async Task<(double? Score, long? Rank)> GetPlayerRankAsync(string player)
    {
        var score = await Db.SortedSetScoreAsync(_key, player);
        if (score is null)
        {
            return (null, null);
        }

        var rank = await Db.SortedSetRankAsync(_key, player, Order.Descending);
        return (score, rank + 1);
    }
}
