using Microsoft.Extensions.Options;
using TeleBaseBotFW.Models;
using TeleBaseBotFW.TeleBase;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace TeleBaseBotFW.Services
{
    public class TelegramBotServices : TeleBaseBot
    {
        public TelegramBotServices(ITelegramBotClient bot, IOptions<TelegramBotConfig> botOptions)
           : base(bot, botOptions.Value.CommandWaitTime) { }

        internal override async Task OnPrivateChat(UpdateInfo update, CancellationToken cancellationToken = default)
        {
            var chat = update.Message.Chat;

            if (update.Message.Text == "/start")
            {
                CreateForm(update, async () =>
                {
                    await Bot.SendTextMessageAsync(chat, "What is your first name?",
                        replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[]
                        {
                            new("Bob1") { CallbackData = "Bob1" }, new("Bob2") { CallbackData = "Bob2" }, new("Bob3") { CallbackData = "Bob3" }
                        }),
                        cancellationToken: cancellationToken);
                    var firstName = await WaitFirstReply(update, cancellationToken: cancellationToken);

                    await Bot.SendTextMessageAsync(chat, "What is your last name?",
                       replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[]
                        {
                            new("Bob1") { CallbackData = "Bob1" }, new("Bob2") { CallbackData = "Bob2" }, new("Bob3") { CallbackData = "Bob3" }
                        }),
                       cancellationToken: cancellationToken);
                    var lastName = await WaitFirstReply(update, ReplyKind.Message, cancellationToken: cancellationToken);

                    await Bot.SendTextMessageAsync(chat, "What is your gender?",
                        replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[]
                        {
                            new("Male") { CallbackData = "🚹" }, new("Female") { CallbackData = "🚺" }, new("Other") { CallbackData = "⚧" }
                        }));
                    var genderEmoji = await WaitFirstReply(update, ReplyKind.CallbackData, cancellationToken: cancellationToken);
                    await ReplyCallbackAsync(update, "You clicked " + genderEmoji);
                    await Bot.SendTextMessageAsync(chat, $"Welcome, {firstName} {lastName}! ({genderEmoji})", cancellationToken: cancellationToken);
                }, cancellationToken);
            }
            else
            { await Bot.SendTextMessageAsync(chat, "Please type /start to interact", cancellationToken: cancellationToken); }
        }

        internal override Task OnGroupChat(UpdateInfo update, CancellationToken cancellationToken = default)
        {
            var chat = update.Message.Chat;
            if (update.Message.Text == "/group")
            {
                CreateForm(update, async () =>
                {
                    await Bot.SendTextMessageAsync(chat, "Please click or key-in some text",
                        replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[] { new("Check") { CallbackData = "Check" } }),
                        cancellationToken: cancellationToken);
                    var result = await WaitFirstReply(update, cancellationToken: cancellationToken);

                    await Bot.SendTextMessageAsync(chat, $"Your reply: {result}", cancellationToken: cancellationToken);

                }, cancellationToken);
            }
            return Task.CompletedTask;
        }

        internal override Task OnChannel(UpdateInfo update, CancellationToken cancellationToken = default)
        {
            var chat = update.Message.Chat;
            if (update.Message.Text == "/channel")
            {
                CreateForm(update, async () =>
                {
                    await Bot.SendTextMessageAsync(chat, "Please click or key-in some text",
                        replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[] { new("Check") { CallbackData = "Check" } }),
                        cancellationToken: cancellationToken);
                    var result = await WaitFirstReply(update, cancellationToken: cancellationToken);

                    await Bot.SendTextMessageAsync(chat, $"Your reply: {result}", cancellationToken: cancellationToken);
                }, cancellationToken);
            }
            return Task.CompletedTask;
        }
    }
}