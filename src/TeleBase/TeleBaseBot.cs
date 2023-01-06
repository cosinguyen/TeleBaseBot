using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TeleBaseBotFW.TeleBase
{
    public class TeleBaseBot
    {
        internal readonly ITelegramBotClient Bot;
        internal readonly User Me;
        internal List<string> BanCallbackData = new();
        internal List<string> BanMessageText = new();
        private readonly int _commandWaitTime;
        private readonly Dictionary<long, TaskInfo> _tasks = new();
        private int _lastUpdateId = -1;

        internal virtual Task OnPrivateChat(UpdateInfo update, CancellationToken cancellationToken = default) => Task.CompletedTask;
        internal virtual Task OnGroupChat(UpdateInfo update, CancellationToken cancellationToken = default) => Task.CompletedTask;
        internal virtual Task OnChannel(UpdateInfo update, CancellationToken cancellationToken = default) => Task.CompletedTask;
        internal virtual Task OnOtherEvents(UpdateInfo update, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public TeleBaseBot(ITelegramBotClient bot, int commandWaitTime = 0)
        {
            Bot = bot;
            _commandWaitTime = commandWaitTime > 0 ? commandWaitTime : 0;
            Me = Bot.GetMeAsync().Result;
        }

        internal Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default)
        {
            if (update.Id <= _lastUpdateId)
                return Task.CompletedTask;

            _lastUpdateId = update.Id;

            switch (update.Type)
            {
                case UpdateType.Message when update.Message != null:
                    _ = HandleUpdateAsync(update, update.Message, UpdateKind.NewMessage, cancellationToken);
                    break;
                case UpdateType.ChannelPost when update.ChannelPost != null:
                    _ = HandleUpdateAsync(update, update.ChannelPost, UpdateKind.NewMessage, cancellationToken);
                    break;
                case UpdateType.CallbackQuery when update.CallbackQuery?.Message != null:
                    _ = HandleUpdateAsync(update, update.CallbackQuery.Message, UpdateKind.CallbackQuery, cancellationToken);
                    break;
                default: break;
            }

            return Task.CompletedTask;
        }
        private async Task HandleUpdateAsync(Update update, Message message, UpdateKind updateKind, CancellationToken cancellationToken = default)
        {
            var chat = message.Chat;
            var chatId = chat.Id;
            var updateType = update.Type;

            UpdateInfo updateInfo = new(updateKind, update, message);
            if (updateType == UpdateType.CallbackQuery)
                updateInfo.CallbackData = update.CallbackQuery?.Data;

            if ((updateKind == UpdateKind.NewMessage && message.Text != null && BanMessageText.Contains(message.Text)) ||
            (updateKind == UpdateKind.CallbackQuery && updateInfo.CallbackData != null && BanCallbackData.Contains(updateInfo.CallbackData)))
            {
                lock (_tasks)
                {
                    if (_tasks.TryGetValue(chatId, out var taskInfo))
                    {
                        taskInfo.Task = null;
                        _tasks.Remove(chatId);
                    }
                }
            }
            else
            {
                lock (_tasks)
                {
                    if (_tasks.TryGetValue(chatId, out var taskInfo))
                    {
                        lock (taskInfo)
                        {
                            if (taskInfo.Task != null)
                            {
                                taskInfo.Updates.Enqueue(updateInfo);
                                taskInfo.Semaphore.Release();
                                return;
                            }
                            else
                            { _tasks.Remove(chatId); }
                        }
                    }
                }
            }

            await HandleUpdateAsync(updateInfo, cancellationToken);
        }
        private async Task HandleUpdateAsync(UpdateInfo update, CancellationToken cancellationToken = default)
        {
            var chatType = update.Message.Chat.Type;

            switch (chatType)
            {
                case ChatType.Private:
                    await OnPrivateChat(update, cancellationToken);
                    break;
                case ChatType.Group or ChatType.Supergroup:
                    await OnGroupChat(update, cancellationToken);
                    break;
                case ChatType.Channel:
                    await OnChannel(update, cancellationToken);
                    break;
                default:
                    await OnOtherEvents(update, cancellationToken);
                    break;
            }
        }

        private async Task<UpdateKind> NextEvent(UpdateInfo update, CancellationToken cancellationToken = default)
        {
            CancellationToken countDownToken;
            if (_commandWaitTime == 0)
                countDownToken = new CancellationTokenSource().Token;
            else
                countDownToken = new CancellationTokenSource(TimeSpan.FromSeconds(_commandWaitTime)).Token;

            using CancellationTokenSource bothCT = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, countDownToken);
            UpdateInfo newUpdate = await ((IGetNext)update).NextUpdateAsync(bothCT.Token);

            update.Message = newUpdate.Message;
            update.CallbackData = newUpdate.CallbackData;
            update.Update = newUpdate.Update;

            return update.UpdateKind = newUpdate.UpdateKind;
        }
        internal void CreateForm(UpdateInfo update, Func<Task> userForm, CancellationToken cancellationToken = default)
        {
            TaskInfo? taskInfo;
            var chatId = update.Message.Chat.Id;

            lock (_tasks)
            {
                if (!_tasks.TryGetValue(chatId, out taskInfo))
                { _tasks[chatId] = taskInfo = new TaskInfo(); }
            }

            lock (update)
            { update.TaskInfo = taskInfo; }

            taskInfo.Task = Task.Run(userForm, cancellationToken).ContinueWith(t =>
            {
                lock (taskInfo)
                    if (taskInfo.Semaphore.CurrentCount == 0)
                    {
                        taskInfo.Task = null;
                        lock (_tasks)
                            _tasks.Remove(chatId);
                        return;
                    }
            }, cancellationToken);
        }
        internal async Task<string> WaitFirstReply(UpdateInfo update, ReplyKind replyKind = ReplyKind.All, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                var newEvent = await NextEvent(update, cancellationToken);
                switch (newEvent)
                {
                    case UpdateKind.NewMessage
                    when replyKind == ReplyKind.Message || replyKind == ReplyKind.All:
                        return update.Message.Text ?? string.Empty;
                    case UpdateKind.CallbackQuery:
                        if (replyKind == ReplyKind.CallbackData || replyKind == ReplyKind.All)
                        { return update.CallbackData ?? string.Empty; }
                        else
                        { await ReplyCallbackAsync(update, null, showAlert: false, cancellationToken: cancellationToken); }
                        continue;
                }
            }
        }
        internal async Task ReplyCallbackAsync(UpdateInfo update, string? text = null, string? url = null, bool showAlert = true, CancellationToken cancellationToken = default)

        {
            if (update.Update.Type != UpdateType.CallbackQuery)
                throw new InvalidOperationException("This method can be called only for CallbackQuery updates");

            if (update.Update.CallbackQuery?.Id != null)
                await Bot.AnswerCallbackQueryAsync(update.Update.CallbackQuery.Id, text, showAlert, url, cancellationToken: cancellationToken);
        }
    }
}