using System.Globalization;
using LeaderboardRedisMvc.Services;
using LeaderboardRedisMvc.Settings;
using Microsoft.AspNetCore.Localization;
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

// Forca en-US: SO em pt-BR usa "." como separador de milhar, o que faz o
// model binding de double interpretar "22.5" como 225. Pontuacao precisa
// do "." como decimal, nao formatacao regional.
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = [new CultureInfo("en-US")],
    SupportedUICultures = [new CultureInfo("en-US")]
});

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Leaderboard}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
