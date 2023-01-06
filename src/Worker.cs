using Microsoft.Extensions.Options;
using TeleBaseBotFW.Models;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace TeleBaseBotFW
{
    public class Worker : IHostedService
    {
        private readonly ITelegramBotClient _bot;
        private readonly TelegramBotConfig _botCfg;

        public Worker(ITelegramBotClient bot, IOptions<TelegramBotConfig> botOptions)
        {
            _bot = bot;
            _botCfg = botOptions.Value;
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            var me = await _bot.GetMeAsync(cancellationToken: stoppingToken);
            Console.WriteLine($"Successfully connected to Telegram Bot ({me.Username})");

            var webhookAddress = _botCfg.HostAddress + _botCfg.CustomAddress;

            var webhookInfo = await _bot.GetWebhookInfoAsync(cancellationToken: stoppingToken);

            if (webhookInfo.Url != webhookAddress)
            {
                await _bot.SetWebhookAsync(url: webhookAddress,
                    allowedUpdates: Array.Empty<UpdateType>(),
                    secretToken: _botCfg.SecretToken,
                    cancellationToken: stoppingToken);
                Console.WriteLine($"Set new Webhook: {webhookAddress}");
            }
            else
            { Console.WriteLine($"Reuse previous Webhook address: {webhookInfo.Url}"); }

            if (!string.IsNullOrEmpty(webhookInfo.LastErrorMessage))
            { Console.WriteLine($"Last webhook error: {webhookInfo.LastErrorDate} {webhookInfo.LastErrorMessage}"); }
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            var webhookAddress = _botCfg.HostAddress + _botCfg.CustomAddress;

            await _bot.DeleteWebhookAsync(cancellationToken: stoppingToken);

            Console.WriteLine($"Webhook deleted: {webhookAddress}");
        }
    }
}