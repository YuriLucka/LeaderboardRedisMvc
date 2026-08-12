using LeaderboardRedisMvc.Services;
using LeaderboardRedisMvc.Settings;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var settings = builder.Configuration.GetSection("Redis").Get<RedisSettings>()!;
    return ConnectionMultiplexer.Connect(settings.ConnectionString);
});

builder.Services.AddSingleton<LeaderboardService>();
builder.Services.AddSingleton<PlayerProfileService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Leaderboard}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
