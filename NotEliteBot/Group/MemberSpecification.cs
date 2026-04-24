using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace NotEliteBot
{
    public interface IMemberSpecificHandler
    {
        long UserId { get; }

        Task Handle(
            ITelegramBotClient botClient,
            Update update,
            Session session,
            string messageText,
            CancellationToken ct);
    }
    public static class MemberSpecificRegistry
    {
        private static readonly Dictionary<long, List<IMemberSpecificHandler>> Handlers = new();

        public static void Register(IMemberSpecificHandler handler)
        {
            if (!Handlers.ContainsKey(handler.UserId))
                Handlers[handler.UserId] = new List<IMemberSpecificHandler>();

            Handlers[handler.UserId].Add(handler);
        }
        public static void RegisterAll()
        {
            // Регистрируем все обработчики
            Register(new Members.Dylan());
            Register(new Members.Almazman());
            Register(new Members.Artik());
            Register(new Members.Dinar());
            // Добавляйте другие обработчики здесь

        }
        public static IEnumerable<IMemberSpecificHandler> Get(long userId)
        {
            return Handlers.TryGetValue(userId, out var list)
                ? list
                : Enumerable.Empty<IMemberSpecificHandler>();
        }
    }
}
