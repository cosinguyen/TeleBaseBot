using Microsoft.Extensions.Options;

namespace TeleBaseBotFW
{
    internal static class Helper
    {
        internal static ControllerActionEndpointConventionBuilder MapBotWebhookRoute<T>(
        this IEndpointRouteBuilder endpoints,
        string route)
        {
            var controllerName = typeof(T).Name.Replace("Controller", "");
            var actionName = typeof(T).GetMethods()[0].Name;

            return endpoints.MapControllerRoute(
                name: "telegram_bot_webhook",
                pattern: route,
                defaults: new { controller = controllerName, action = actionName });
        }

        internal static T GetConfiguration<T>(this IServiceProvider serviceProvider) where T : class
        {
            var o = serviceProvider.GetService<IOptions<T>>();
            if (o is null)
                throw new ArgumentNullException(nameof(T));
                
            return o.Value;
        }
    }
}