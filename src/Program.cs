
using TeleBaseBotFW;
using TeleBaseBotFW.Controllers;
using TeleBaseBotFW.Models;
using TeleBaseBotFW.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TelegramBotConfig>(builder.Configuration.GetSection("TelegramBot"));
builder.Services.AddSingleton<TelegramBotServices>();
builder.Services.AddHttpClient("http_telegram_bot")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
        var botConfig = sp.GetConfiguration<TelegramBotConfig>();
        var options = new TelegramBotClientOptions(botConfig.APIKey);
        return new TelegramBotClient(options, httpClient);
    });
builder.Services.AddHostedService<Worker>();
builder.Services.AddControllers().AddNewtonsoftJson();

var app = builder.Build();

var botCfg = app.Services.GetConfiguration<TelegramBotConfig>();
app.MapBotWebhookRoute<BotController>(route: botCfg.CustomAddress);
app.MapControllers();

app.Run();