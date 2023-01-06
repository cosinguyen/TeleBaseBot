# TeleBaseBotFW

During the process of learning how to create a Telegram Bot through C#, I combined an example with a framework to create this framework.
* Special thanks to the authors of the following projects:
1) [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot)
2) [YourEasyBot](https://github.com/wiz0u/YourEasyBot)
3) [Telegram.Bot.Examples.WebHook](https://github.com/TelegramBots/Telegram.Bot.Examples/tree/master/Telegram.Bot.Examples.WebHook)

## Webhook
Based on [karb0f0s](https://github.com/karb0f0s) example, I've reworked building a Webhook with the Telegram Bot server. With the current version (18.0.0) of the Telegram.Bot library, the `secretToken` variable not found in the `SetWebhookAsync` method. Therefore, the best current authentication can be used according to the suggestion of the library creator, which is to use a token string as the back of the domain name, because if this string is exposed, your Bot will also be stolen.

Example: https://your-domain.com/token

I have made a slight change that will use `Guid.NewGuid()` to generate a string of changes every time the application starts, so if you feel that your Webhook address is being spammed or hacked just restarting the app will have a new webhook address generated. You can also fix it by changing the `CustomAddress` property in `appsettings.json`.

In version `19.0.0-preview.2` of the library I have seen the appearance of `secretToken` variable in `SetWebhookAsync` method so maybe this is redundant you can use like [karb0f0s example](https://github.com/TelegramBots/Telegram.Bot.Examples/tree/master/Telegram.Bot.Examples.WebHook) or use mine for added security.

## TeleBaseBot
Based on [YourEasyBot](https://github.com/wiz0u/YourEasyBot) framework of [wiz0u](https://github.com/wiz0u), I've made a few other changes after a long time using his framework. Maybe that's not a drawback for many people but for me having to start a Task even though it will end up in the next answer would be a waste of server resources. Based on what I searched about storing Bot's state when talking to users, most of them will use a stored variable or save it to the database and compare when receiving the answer from user. The above usage is great if it is simple structures but to implement it requires quite a lot of variables or access to the database if your application has a multi-answer form. So I ventured to modify [wiz0u](https://github.com/wiz0u)'s framework to make it flexible to use whether it is necessary to instantiate a new Task to interact with the user.
* **Important**: This framework only focuses on sending messages and receiving messages, call back data. Therefore, the features used to handle other updates from users have been abandoned a lot, please use with caution.

**Example**
```
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
```

### CreateForm
The points to note in the example above are the method:
`CreateForm(UpdateInfo update, Func<Task> userForm, CancellationToken cancellationToken)`<br />
Only when necessary use the above method and pass in a `Func<Task>` containing all the content you would ask on a form.<br />
Also with the above example, users who do not type `/start` when interacting with the Bot will receive the answer immediately without generating any additional Tasks.

### WaitFirstReply
Also with the above example I changed the use of `ButtonClicked` method and `NewTextMessage` method and replaced it with the only `WaitFirstReply` method. <br />
**Note that the `WaitFirstReply` method must be called in a Task that the Task must reach the CreateForm method, otherwise it will immediately respond with the current data.**

How to use the `WaitFirstReply` method is very simple that just pass it the parameters (UpdateInfo update, ReplyKind replyKind, CancellationToken cancellationToken), it's very simple that you just only need to pass the `update` variable.

* In question number 1, when I asked the user for their first name included 3 buttons and call the `WaitFirstReply` method with `ReplyKind` is `null`. If they press the button or answer in text, the system will immediately record the first result.
* In question number 2, when I asked the user for their last name included 3 buttons and call the `WaitFirstReply` method with `ReplyKind` is `ReplyKind.Message`. If they keep pressing the buttons continuously, there won't be any record from the system that only accepts text input.
* In question number 3, when I asked the user for their gender included 3 buttons and call the `WaitFirstReply` method with `ReplyKind` is `ReplyKind.CallbackData`. You know it only accepts button presses.

This is very good if you have a request to the user to enter a text but it is accompanied by a button to cancel the command and the user has the option to click cancel or enter the data according to your request.

### Command Wait Time
To set timeout for a question we can generate a token using `CancellationTokenSource(TimeSpan).Token` and pass this variable to `WaitFirstReply` method. But sometimes we forget and make that Task go on forever waiting for the user's reply, so I built a global variable that fixed the maximum seconds of waiting for a reply and you can pass it in when inheriting `TeleBaseBot` class

```
public class TelegramBotServices : TeleBaseBot
{
    public TelegramBotServices(ITelegramBotClient bot, IOptions<TelegramBotConfig> botOptions)
        : base(bot, botOptions.Value.CommandWaitTime) { }
}
```
In the above example I have passed the initialized Bot parameter along with the time in seconds `base(bot, botOptions.Value.CommandWaitTime)` you can also pass null parameter for time and it will be permanent `base(bot)`

**How to use it in combination?**
I assume that I will pass it 60 seconds by `base(bot, 60)` and in my code will wait for the user's reply `firstName` about 90 seconds
```
CancellationToken ninetyToken = new CancellationTokenSource(TimeSpan.FromSeconds(90)).Token;
var firstName = await WaitFirstReply(update, cancellationToken: ninetyToken);
```
The user then only has 60 seconds maximum to reply because it hits the global variable above. But if I pass the global variable as `null` it will accept to wait up to 90 seconds. I added this feature so that you can set a maximum time possible and the system will throw an exception to destroy the task if it hits the maximum time, it will help you if you forgot to set a time for your question, avoid wasting server resources.

### Ban List
This feature works differently than you think, the ban list here is that when touching this, it immediately cancels the current **Form** and goes straight to the command handling method. When I talked about `WaitFirstReply` I said:

>This is very good if you have a request to the user to enter a text but it is accompanied by a button to cancel the command and the user has the option to click cancel or enter the data according to your request.

The case here will happen if you set `CallbackData = "close-command"` or the user types `/start` while we are waiting for their reply then we can get that string and process it. If your form is just like the example above it will terminate and return the resources to the system immediately. But if the code below still continues to do things without the form, how to exit the form properly.

**Example**
```
public class TelegramBotServices : TeleBaseBot
{
    public TelegramBotServices(ITelegramBotClient bot, IOptions<TelegramBotConfig> botOptions)
        : base(bot, botOptions.Value.CommandWaitTime)
    {
        BanCallbackData.Add("cancel");
        BanMessageText.Add("/start");
    }

    internal override async Task OnPrivateChat(UpdateInfo update, CancellationToken cancellationToken = default)
    {
        var chat = update.Message.Chat;

        if (update.Message.Text == "/start")
        {
            Random rd = new();

            var noOne = rd.Next(1, 100).ToString();
            var noTwo = rd.Next(1, 100).ToString();

            CreateForm(update, async () =>
            {
                await Bot.SendTextMessageAsync(chat, "Choose anything",
                    replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton[]
                    {
                        new(noOne) { CallbackData = noOne }, new(noTwo) { CallbackData = noTwo }, new("Cancel") { CallbackData = "cancel" }
                    }),
                    cancellationToken: cancellationToken);
                var result = await WaitFirstReply(update, cancellationToken: cancellationToken);

                await Bot.SendTextMessageAsync(chat, $"You choose, {result}", cancellationToken: cancellationToken);
            }, cancellationToken);
        }
        else if (update.CallbackData == "cancel")
        {
            await Bot.SendTextMessageAsync(chat, $"Thanks", cancellationToken: cancellationToken);
        }
    }
}
```
In this example I added Callbackdata ban list is `cancel` and the text ban list is `/start`. So when the user clicks the Cancel button, this form will immediately cancel and return resources to the system and then continue to go into the `OnPrivateChat` method without creating a Task that stores the state and returns "Thanks". Conversely, if the user types "/start", this form will immediately destroy and return resources to the system, then create a new form similar to this. If you don't use this function, the user will get an answer from the Bot as `You choose, cancel` or `You choose, /start`

### appsettings.json
```
"TelegramBot": {
  "APIKey": "<YOUR-BOT-TOKEN>",
  "HostAddress": "<YOUR-DOMAIN>",
  "CustomAddress": "/<your-api-address>", //Example: "/bot"
  "SecretToken": "<your-serect-token>", //Example: "myserectstring"
  "CommandWaitTime": <time-in-seconds> //Example: 60
}
```
* APIKey (**Required**): It is necessary to operate Telegram Bot
* HostAddress (**Required**): Your root domain, please see this example [Telegram.Bot.Examples.WebHook](https://github.com/TelegramBots/Telegram.Bot.Examples/tree/master/Telegram.Bot.Examples.WebHook#ngrok)
* CustomAddress(*Optional*): You can fill or delete it, the system will generate it automatically
* SecretToken(*Optional*): If you have fixed your `CustomAddress`, you should include this property for added security, but if the `CustomAddress` uses auto-generated then you probably won't need to fill this property.
* CommandWaitTime(*Optional*): You should fill a number here, if you delete it you need to pay attention to the time setting for the questions otherwise all will wait indefinitely.

Therefore, the configuration is simple and can be used immediately as shown below:
```
"TelegramBot": {
  "APIKey": "<YOUR-BOT-TOKEN>",
  "HostAddress": "<YOUR-DOMAIN>"
}
```

**Thank you for watching**
