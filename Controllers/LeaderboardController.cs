using LeaderboardRedisMvc.Models;
using LeaderboardRedisMvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeaderboardRedisMvc.Controllers;

public class LeaderboardController : Controller
{
    private readonly LeaderboardService _leaderboard;
    private readonly PlayerProfileService _profiles;

    public LeaderboardController(LeaderboardService leaderboard, PlayerProfileService profiles)
    {
        _leaderboard = leaderboard;
        _profiles = profiles;
    }

    public async Task<IActionResult> Index()
    {
        var top = await _leaderboard.GetTopAsync(10);
        return View(top);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddScore(string player, double points)
    {
        if (!string.IsNullOrWhiteSpace(player))
        {
            await _leaderboard.AddScoreAsync(player.Trim(), points);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Player(string name)
    {
        var (score, rank) = await _leaderboard.GetPlayerRankAsync(name);
        if (score is null)
        {
            return NotFound();
        }

        var (profile, fromCache) = await _profiles.GetProfileAsync(name);
        ViewBag.FromCache = fromCache;

        return View(profile);
    }
}
