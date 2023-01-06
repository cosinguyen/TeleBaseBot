using Microsoft.AspNetCore.Mvc;
using TeleBaseBotFW.Attributes;
using TeleBaseBotFW.Services;
using Telegram.Bot.Types;

namespace TeleBaseBotFW.Controllers
{
    public class BotController : ControllerBase
    {
        [HttpPost]
        [ValidateTelegramBot]
        public async Task<IActionResult> Post(
            [FromBody] Update update,
            [FromServices] TelegramBotServices handleUpdate,
            CancellationToken cancellationToken)
        {
            await handleUpdate.HandleUpdateAsync(update, cancellationToken);
            return Ok();
        }
    }
}