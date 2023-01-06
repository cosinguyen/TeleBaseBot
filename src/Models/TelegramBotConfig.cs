#pragma warning disable CS8618

namespace TeleBaseBotFW.Models
{
    public class TelegramBotConfig
    {
        public string APIKey { get; set; }
        public string HostAddress { get; set; }
        public string CustomAddress { get; set; } = $"/{Guid.NewGuid()}";
        public string? SecretToken { get; set; }
        public int CommandWaitTime { get; set; } = 0;
    }
}