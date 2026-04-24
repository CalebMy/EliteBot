using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using static NotEliteBot.Chanel;
using static NotEliteBot.Commander;
using Telegram.Bot.Types.ReplyMarkups;

namespace NotEliteBot
{
    partial class Group
    {
        public static async void Main(ITelegramBotClient botClient, long userID, long chatID, Update update, CancellationToken cancellationToken)
        {
            var session = SessionManager.Get(userID, chatID, SessionType.Group);
            try
            {
                var msg = update.Message;
                if (update.CallbackQuery != null) { CalloutProcess(botClient, update, cancellationToken, session); return; }

                string messageText = msg.Text
                              ?? msg.Caption
                              ?? string.Empty;

                if (string.IsNullOrEmpty(messageText)) messageText = "";

                bool isBanWorded = await BanWordProcess(botClient, update, session, cancellationToken);
                if (isBanWorded) return;
                CommonProcess(botClient, update, session, cancellationToken);
                MemberSpecificProcess(botClient, update, session, messageText, cancellationToken);
                CommandProcess(botClient, update, session, cancellationToken);

            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message, Debug.LogLevel.Error);
            }

        }
        public static async void CommonProcess(ITelegramBotClient botClient, Update update, Session session, CancellationToken cancellationToken)
        {

            var msg = update.Message;

            string messageText = msg.Text
                          ?? msg.Caption
                          ?? string.Empty;

            if (session.Id == IDs.Telegram)
            {
                var post = Memory.PostStorage
                    .FirstOrDefault(x => x.ThreadId == null);

                if (post != null && post.ChatId == update.Message.SenderChat.Id)
                {
                    post.ThreadId = msg.MessageId;
                    Debug.Log($"Синхронизированы посты\n        Канал: {post.MessageId}\n       Чат: {msg.MessageId}", Debug.LogLevel.Info);
                }
            }

            if (messageText.Contains("ELITE"))
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    ">ELITE\\*\n" +
                    "\\*Запрещённая в ЭЭ организация\\.",
                    parseMode: ParseMode.MarkdownV2,
                    replyToMessageId: update.Message.MessageId);
                Debug.Log("ЗАПРЕТКА", Debug.LogLevel.Info);
            }
            else if (messageText.Contains("37"))
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "37 САН-ФРАНЦИСКО 37",
                    replyToMessageId: update.Message.MessageId);
                Debug.Log("САН-ФРАНЦИСКО", Debug.LogLevel.Info);
            }


            if (msg.From.Id != IDs.Telegram && (messageText.ToLower().StartsWith("это не я") ||
                messageText.ToLower().StartsWith("ето не я") ||
                messageText.ToLower().StartsWith("енто не я")))
            {
                var msg2 = update.Message;

                // --- 1. базовая подпись ---
                string expectedSignature = string.Join(" ",
                    new[] { msg.From.FirstName, msg.From.LastName }
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                );

                var post = Chanel.GetPostFromThred(IDs.ElitkaChanel, msg.MessageThreadId);
                if (post == null) return;

                // --- 2. собираем все допустимые подписи ---
                var validSignatures = new HashSet<string>();

                if (!string.IsNullOrWhiteSpace(expectedSignature))
                    validSignatures.Add(expectedSignature);

                // --- 3. ассоциированные каналы ---
                var session2 = SessionManager.Get(
                    msg.From.Id,
                    msg.From.Id,
                    SessionType.Private
                );

                if (session2?.AssociatedIDs != null)
                {
                    foreach (var assocId in session2.AssociatedIDs)
                    {
                        try
                        {
                            var chat = await botClient.GetChatAsync(assocId);

                            if (!string.IsNullOrWhiteSpace(chat.Title))
                                validSignatures.Add(chat.Title);
                        }
                        catch
                        {
                            // нет доступа — пропускаем
                        }
                    }
                }

                // --- 4. проверка ---
                bool isValid = validSignatures.Any(sig =>
                    Chanel.CheckAuthorSignature(IDs.ElitkaChanel, post.MessageId, sig)
                );

                if (!isValid)
                    return;

                // --- 5. редактирование ---
                await botClient.SendTextMessageAsync(
                    msg.Chat.Id,
                    $"Фейк отредактирован по запросу ID\\#`\\{msg.From.Id}`",
                    replyToMessageId: msg.MessageThreadId,
                    parseMode: ParseMode.MarkdownV2);

                try
                {
                    if (!string.IsNullOrEmpty(post.Text))
                    {
                        await botClient.EditMessageTextAsync(
                            chatId: IDs.ElitkaChanel,
                            messageId: post.MessageId,
                            text: "⚠️ Пост удалён: фейковая подпись"
                        );
                    }
                    else
                    {
                        await botClient.EditMessageCaptionAsync(
                            chatId: IDs.ElitkaChanel,
                            messageId: post.MessageId,
                            caption: "⚠️ Это медиа отправлено от фейкового аккаунта"
                        );
                    }

                    Debug.Log(
                        $"Фейк отредактирован\n        Канал: {post.MessageId}\n       Чат: {post.ThreadId}",
                        Debug.LogLevel.Info);
                }
                catch (Exception ex)
                {
                    Debug.Log($"Ошибка при редактировании: {ex.Message}", Debug.LogLevel.Error);
                }
            }
            else if (msg.From.Id != IDs.Telegram && (  messageText.ToLower().StartsWith("правда") ||
                        messageText.ToLower().StartsWith("а правда") ||
                        messageText.ToLower().StartsWith("это правда") ||
                        messageText.ToLower().StartsWith("а это правда") ||
                        messageText.ToLower().StartsWith("врёш") ||
                        messageText.ToLower().StartsWith("вреш") ||
                        messageText.ToLower().StartsWith("пиздиш")) && messageText.EndsWith("?"))
            {
                string[] responses = {
                    "Да",
                    "Нет",
                    "Сомневаюсь",
                    "Возможно",
                    "Всё так",
                    "Не надейся",
                    "Вероятно да",
                    "Скорее всего нет",
                    "Не знаю",
                    "Не ебу",
                    "В душе не чаю",
                    "Depends",
                    "Именно так!",
                    "Ни в коем случае!"
                };
                Random rnd = new Random();
                string answer = responses[rnd.Next(0, responses.Length)];
                try
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        answer,
                        replyToMessageId: msg.MessageId
                    );
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message, Debug.LogLevel.Error);
                }
            }
            else if (msg.From.Id != IDs.Telegram && (messageText.ToLower().StartsWith("а ведь") && !messageText.ToLower().EndsWith("а ведь")))
            {
                string[] responses =
                {
                    "А ведь правда",
                    "Факты щас",
                    "А ведь ты ошибаешься",
                    "Минусы будут?",
                    "Минусов не вижу",
                    "А плюсы будут?",
                    "Еба факты щас",
                    "А ведь да",
                    "А ведь нет",
                    "Тру",
                    "Реально",
                    "А ведь действительно",
                    "Хммм... И правда",
                    "oh nonononononono wait wait wait",
                    "No way",
                    "😅"
                };
                Random rnd = new();
                string response = responses[rnd.Next(0, responses.Length)];
                try
                {
                    await botClient.SendTextMessageAsync(
                        msg.Chat.Id,
                        response,
                        replyToMessageId: msg.MessageId
                    );
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message, Debug.LogLevel.Error);
                }
            }
        }
        public static async void MemberSpecificProcess(ITelegramBotClient botClient, Update update, Session session, string messageText, CancellationToken cancellationToken)
        {
            var msg = update.Message;
            if (msg?.From == null) return;

            var handlers = MemberSpecificRegistry.Get(session.Id);

            foreach (var handler in handlers)
            {
                await handler.Handle(botClient, update, session, messageText, cancellationToken);
            }
        } // Выделено в Members.cs и MemberSpecification.cs
        public static async void CommandProcess(ITelegramBotClient botClient, Update update, Session session, CancellationToken cancellationToken)
        {
            try
            {
                await Commander.CommandProcess(
                    botClient,
                    update,
                    session,
                    GroupCommonCommandSet,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message, Debug.LogLevel.Error);
                try
                {
                    await botClient.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        $"Ошибка: {ex.Message}",
                        replyToMessageId: update.Message.MessageId,
                        cancellationToken: cancellationToken
                    );
                }
                catch (Exception ex2)
                {
                    Debug.Log(ex2.Message, Debug.LogLevel.Error);
                }

            }
        }

        public static async Task<bool> BanWordProcess(ITelegramBotClient botClient, Update update, Session session, CancellationToken cancellationToken)
        {
            var msg = update.Message;
            if (msg == null) return false;

            // игнорируем системные/бота
            if (msg.From == null || msg.From.Id == IDs.Telegram)
                return false;

            ChatMember member;
            try
            {
                member = await botClient.GetChatMemberAsync(msg.Chat.Id, msg.From.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.Log($"Ошибка при получении статуса участника: {ex.Message}", Debug.LogLevel.Error);
                return false;
            }
            if (member.Status == ChatMemberStatus.Administrator || member.Status == ChatMemberStatus.Creator)
            {
                // Игнорируем администраторов и создателей
                return false;
            }

            string text = msg.Text ?? msg.Caption;
            if (string.IsNullOrEmpty(text)) return false;

            // ищем совпадение
            var banned = Memory.BanWords
                .FirstOrDefault(word => L33t.L33tMatcher.ContainsL33t(text, word));

            if (banned == null) return false;
            
            string key = $"banword_{update.Message.Chat.Id}_{update.Message.From.Id}";
            
            if (Commander.Cooldowns.TryUse(key, TimeSpan.FromMinutes(1), out var remaining))
            {
                // ответ
                await botClient.SendTextMessageAsync(
                    chatId: msg.Chat.Id,
                    text: $"@{msg.From.Username} вы использовали запрещённое слово!",
                    replyToMessageId: msg.MessageId,
                    cancellationToken: cancellationToken
                );
            }
            // удаление
            await botClient.DeleteMessageAsync(
                chatId: msg.Chat.Id,
                messageId: msg.MessageId,
                cancellationToken: cancellationToken
            );

            Debug.Log($"Найден запрет: {banned}", Debug.LogLevel.Info);
            return true;
        }
        public static async void CalloutProcess(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, Session session)
        {
            var cb = update.CallbackQuery;
            var data = cb.Data;
            try
            {
                if (update.CallbackQuery?.Data?.StartsWith("help_") == true)
                {
                    var parts = cb.Data.Split('_');

                    long userId = long.Parse(parts[1]);
                    int page = int.Parse(parts[2]);

                    // --- защита ---
                    if (cb.From.Id != userId)
                    {
                        await botClient.AnswerCallbackQueryAsync(
                            cb.Id,
                            "Эта кнопка не для тебя!"
                        );
                        return;
                    }
                    var tempUpd = new Update { Message = new Message { From = new User { Id = userId }, Chat = new Chat { Id = cb.Message.Chat.Id } } };
                    var (text, totalPages) = await BuildHelpText(
                        botClient,
                        tempUpd,
                        GroupCommonCommandSet,
                        page
                    );

                    InlineKeyboardMarkup? keyboard = null;

                    if (totalPages > 1)
                    {
                        var buttons = new List<InlineKeyboardButton>();

                        if (page > 0)
                            buttons.Add(InlineKeyboardButton.WithCallbackData("⬅️", $"help_{userId}_{page - 1}"));

                        if (page < totalPages - 1)
                            buttons.Add(InlineKeyboardButton.WithCallbackData("➡️", $"help_{userId}_{page + 1}"));

                        keyboard = new InlineKeyboardMarkup(new[] { buttons });
                    }

                    await botClient.EditMessageTextAsync(
                        cb.Message.Chat.Id,
                        cb.Message.MessageId,
                        text,
                        replyMarkup: keyboard
                    );

                    await botClient.AnswerCallbackQueryAsync(cb.Id);
                }
            }
            catch (Exception ex)
            {

            }
        }

    }
}
