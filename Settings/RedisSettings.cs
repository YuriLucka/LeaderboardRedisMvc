namespace LeaderboardRedisMvc.Settings;

public class RedisSettings
{
    public string ConnectionString { get; set; } = string.Empty;

    public string LeaderboardKey { get; set; } = string.Empty;

    public int ProfileCacheTtlSeconds { get; set; }
}
