using System.Text.Json;
using LeaderboardRedisMvc.Models;
using LeaderboardRedisMvc.Settings;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace LeaderboardRedisMvc.Services;

public class PlayerProfileService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly LeaderboardService _leaderboard;
    private readonly int _ttlSeconds;

    public PlayerProfileService(IConnectionMultiplexer redis, LeaderboardService leaderboard, IOptions<RedisSettings> options)
    {
        _redis = redis;
        _leaderboard = leaderboard;
        _ttlSeconds = options.Value.ProfileCacheTtlSeconds;
    }

    private IDatabase Db => _redis.GetDatabase();

    public static string CacheKey(string player) => $"profile:{player}";

    public async Task<(PlayerProfile Profile, bool FromCache)> GetProfileAsync(string player)
    {
        var cached = await Db.StringGetAsync(CacheKey(player));
        if (cached.HasValue)
        {
            return (JsonSerializer.Deserialize<PlayerProfile>((string)cached!)!, true);
        }

        var profile = await ComputeProfileAsync(player);

        await Db.StringSetAsync(CacheKey(player), JsonSerializer.Serialize(profile), TimeSpan.FromSeconds(_ttlSeconds));

        return (profile, false);
    }

    private async Task<PlayerProfile> ComputeProfileAsync(string player)
    {
        // Simula uma consulta cara (ex: agregação em outro sistema) que justifica cachear.
        await Task.Delay(1500);

        var (score, rank) = await _leaderboard.GetPlayerRankAsync(player);
        var random = new Random(player.GetHashCode());

        return new PlayerProfile
        {
            Player = player,
            Score = score ?? 0,
            Rank = rank,
            MatchesPlayed = random.Next(10, 200),
            WinRate = Math.Round(random.NextDouble() * 100, 1),
            ComputedAt = DateTime.UtcNow,
        };
    }
}
