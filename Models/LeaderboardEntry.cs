namespace LeaderboardRedisMvc.Models;

public class LeaderboardEntry
{
    public string Player { get; set; } = string.Empty;

    public double Score { get; set; }

    public long Rank { get; set; }
}
