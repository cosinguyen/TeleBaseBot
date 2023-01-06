using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using TeleBaseBotFW.Models;

namespace TeleBaseBotFW.Attributes
{
    // <summary>
    /// Check for "X-Telegram-Bot-Api-Secret-Token"
    /// Read more: <see href="https://core.telegram.org/bots/api#setwebhook"/> "secret_token"
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidateTelegramBotAttribute : TypeFilterAttribute
    {
        public ValidateTelegramBotAttribute()
            : base(typeof(ValidateTelegramBotFilter))
        {
        }

        private class ValidateTelegramBotFilter : IActionFilter
        {
            private readonly string? _secretToken;

            public ValidateTelegramBotFilter(IOptions<TelegramBotConfig> options)
            {
                var botConfiguration = options.Value;
                _secretToken = botConfiguration.SecretToken;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (!IsValidRequest(context.HttpContext.Request))
                {
                    context.Result = new ObjectResult("\"X-Telegram-Bot-Api-Secret-Token\" is invalid")
                    { StatusCode = 403 };
                }
            }

            private bool IsValidRequest(HttpRequest request)
            {
                if (string.IsNullOrEmpty(_secretToken))
                    return true;

                var isSecretTokenProvided = request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var secretTokenHeader);
                if (!isSecretTokenProvided) return false;

                return string.Equals(secretTokenHeader, _secretToken, StringComparison.Ordinal);
            }
        }
    }
}