namespace LeaderboardRedisMvc.Models;

public class PlayerProfile
{
    public string Player { get; set; } = string.Empty;

    public double Score { get; set; }

    public long? Rank { get; set; }

    public int MatchesPlayed { get; set; }

    public double WinRate { get; set; }

    public DateTime ComputedAt { get; set; }
}
