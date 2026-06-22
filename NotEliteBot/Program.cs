using System;
using System.Collections.Concurrent;
using System.Formats.Tar;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Collections.Specialized.BitVector32;
using System.IO;
namespace NotEliteBot
{
    class Program
    {
        private static ITelegramBotClient _botClient;
        private static ReceiverOptions _receiverOptions;

        static async Task Main()
        {
            AppDomain.CurrentDomain.ProcessExit += (_, __) => Memory.SaveAll();
            new Timer(_ => Memory.SaveAll(), null, 3_600_000, 3_600_000);
            MemberSpecificRegistry.RegisterAll();
            Group.RegisterGroupCommonCommands();
            Private.RegisterPrivateCommands();
            Memory.LoadAll();
            _botClient = new TelegramBotClient(System.IO.File.ReadAllText("key.txt"));
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
                {
                UpdateType.Message,
                UpdateType.CallbackQuery,
                UpdateType.ChannelPost
            },
                // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда ваш бот был оффлайн
                // True - не обрабатывать, False (стоит по умолчанию) - обрабаывать
                ThrowPendingUpdates = true,
            };

            using var cts = new CancellationTokenSource();

            // UpdateHander - обработчик приходящих Update`ов
            // ErrorHandler - обработчик ошибок, связанных с Bot API

            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Запускаем бота

            var me = await _botClient.GetMeAsync(); // Создаем переменную, в которую помещаем информацию о нашем боте.
            Console.WriteLine($"{me.FirstName} запущен!");
            //await _botClient.SendTextMessageAsync(admin, "Бот запущен");

            await Task.Delay(-1); // Устанавливаем бесконечную задержку, чтобы наш бот работал постоянно

        }
        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            var exitButton = new InlineKeyboardMarkup(
                                                    new List<InlineKeyboardButton[]>
                                                    {
                                                        new[]
                                                        {
                                                            InlineKeyboardButton.WithCallbackData("Выход", "exit")
                                                        }
                                                    }
                                                 );

            var msg = update.Message
                   ?? update.ChannelPost
                   ?? update.EditedMessage;
            if (update.CallbackQuery != null) msg = update.CallbackQuery.Message;
            if (msg == null) return;

            var chatType = msg.Chat.Type;
            try
            {
                
                await MessageManager.Tick(botClient, update);
                string key = $"primary-cooldown_{msg?.From?.Id}_{msg.Chat.Id}";
                if (!Commander.Cooldowns.TryUse(key, TimeSpan.FromMilliseconds(500), out var remaining))
                {
                    return;
                }
                ThoughtLogger.Log(IDs.admin, update);
                switch (chatType)
                {
                    case ChatType.Private:
                        Debug.Log($"ЛС: {msg.Chat.Id}", Debug.LogLevel.Action);
                        Private.Main(msg.Chat.Id, botClient, update, cancellationToken);
                        break;
                    case ChatType.Supergroup:
                    case ChatType.Group:
                        Debug.Log($"Группа {msg.Chat.Title} {msg.From.FirstName} {msg.From.Id}", Debug.LogLevel.Action);
                        Group.Main(botClient, msg.From.Id, msg.Chat.Id, update, cancellationToken);
                        break;
                    case ChatType.Channel:
                        if (msg.Chat.Id != IDs.ElitkaChanel) return;
                        Debug.Log($"Канал {msg.Chat.Title} {msg.Chat.Id} {msg.AuthorSignature}", Debug.LogLevel.Action);
                        Chanel.Main(botClient, update, cancellationToken);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception er)
            {
                Debug.Log(er.Message, Debug.LogLevel.Error);
                Debug.Log($"Где: {msg.Chat.Type.ToString()} {msg.Chat.Title}{msg.Chat.FirstName} {msg?.From?.FirstName}{msg?.AuthorSignature}", Debug.LogLevel.Info);
            }
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Debug.Log("Cлучилось некоторое дерьмо! Плохой интернет?", Debug.LogLevel.Warning);
            //Debug.Log($"{error}", Debug.LogLevel.Error);
            return Task.CompletedTask;
        }
    }
    public class IDs
    {
        public static readonly long admin = 5403845498;
        public static readonly long Telegram = 777000;
        public static readonly long ElitkaChanel = -1001983256332;
        public static readonly long ElitkaChat = -1001850405088;
        public static readonly long AlitkaChanel = -1003040366092;
        public static readonly long AlitkaChat = -1002706252827;
        public static readonly long Sglypa = 6444735563;
        public static readonly long Ironman = 5462752233;
    }
    public static class ThoughtLogger
    {
        private static readonly object _lock = new();

        private static DateTime _lastMessageTime = DateTime.MinValue;

        private static long _lastChatId = 0;

        public static void Log(
            long userId,
            Update update,
            string path = "C:\\Users\\Almazman\\Desktop\\RTP-v2-p4\\packs\\RoR2\\assets\\text\\blocks\\admin_thoughts.txt")
        {   
            
            var msg = update.Message;

            if (msg == null)
                return;

            if (msg.From?.Id != userId)
                return;
            if (msg.ForwardFrom != null)
                return;
            if (msg.Text == null || msg.Text.StartsWith('/'))
                return;

            string text =
                msg.Text ??
                msg.Caption ??
                "";

            text = text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            DateTime now = DateTime.UtcNow;

            // -----------------------------
            // НАСТРОЙКИ
            // -----------------------------

            const int shortMessageLength = 12;

            const int mergeWindowSeconds = 90;

            bool isShort =
                text.Length <= shortMessageLength;

            bool isRecent =
                (now - _lastMessageTime).TotalSeconds
                <= mergeWindowSeconds;

            bool sameChat =
                msg.Chat.Id == _lastChatId;

            // -----------------------------
            // РАЗДЕЛИТЕЛЬ
            // -----------------------------

            string separator =
                (isShort && isRecent && sameChat)
                ? "\n"
                : "\n\n";

            lock (_lock)
            {
                bool fileExists = System.IO.File.Exists(path);

                if (!fileExists ||
                    new FileInfo(path).Length == 0)
                {
                    separator = "";
                }

                System.IO.File.AppendAllText(
                    path,
                    separator + text.Replace("\r", "")
                );
            }

            _lastMessageTime = now;
            _lastChatId = msg.Chat.Id;
        }
    }

}