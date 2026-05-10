using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static NotEliteBot.Commander;

namespace NotEliteBot
{
    partial class Private
    {
        public static async void Main(long chatId, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Session session = SessionManager.Get(chatId, chatId, SessionType.Private);
            if (session.BotBan == true) return;
            
            long userId = session.Id;
            if (update.CallbackQuery != null) { CalloutProcess(botClient, update, cancellationToken, session); return; }
            try
            {
                switch (session.Mode)
                {
                    case Mode.Default:
                        await DefaultMode(botClient, update, cancellationToken, session);
                        break;
                    case Mode.AnonymousPost:
                        await AnonymousPostMode(botClient, update, cancellationToken, session);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message, Debug.LogLevel.Error);
            }
        }
        static async Task DefaultMode(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, Session session)
        {
            if (update.Message.Text.StartsWith("/")) CommandProcess(botClient, update, session, PrivateDefaultCommandSet, cancellationToken);
        }
        static async Task AnonymousPostMode(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, Session session)
        {
            if (!string.IsNullOrEmpty(update.Message.Text) && update.Message.Text.StartsWith("/")) CommandProcess(botClient, update, session, PrivateCancelSet, cancellationToken);
            else
            {
                if (session.Mode != Mode.AnonymousPost)
                    return;

                var msg = update.Message;
                await botClient.SendTextMessageAsync(
                    session.Id,
                    $"Принято, ожидайте одобрения. Пост будет опубликован после успешной проверки."
                );

                // 1. пересылаем админу
                var forwarded = await botClient.CopyMessageAsync(
                    chatId: IDs.admin,
                    fromChatId: msg.From.Id,
                    messageId: msg.MessageId
                );

                // 2. отправляем кнопки
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("✅ Принять", $"appr_{forwarded.Id}"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("❌ NSFW", $"decl_nsfw_{forwarded.Id}_{msg.From.Id}_{msg.MessageId}"),
                        InlineKeyboardButton.WithCallbackData("❌ BAD MOOD", $"decl_badmood_{forwarded.Id}_{msg.From.Id}_{msg.MessageId}")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("❌ DONT LIKE", $"decl_dontlike_{forwarded.Id}_{msg.From.Id}_{msg.MessageId}"),
                        InlineKeyboardButton.WithCallbackData("❌ SUS", $"decl_sus_{forwarded.Id}_{msg.From.Id}_{msg.MessageId}")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("💀 BAN", $"ban_{forwarded.Id}_{msg.From.Id}")
                    }
                });

                await botClient.SendTextMessageAsync(
                    IDs.admin,
                    $"ID\\: ||{session.Id}||\n" +
                    $"Действие\\?",
                    replyToMessageId: forwarded.Id,
                    replyMarkup: keyboard,
                    parseMode: ParseMode.MarkdownV2
                );

                Debug.Log($"Анонимка отправлена админу: {msg.Text}", Debug.LogLevel.Info);

                session.Mode = Mode.Default;
            }
        }
        public static async Task<bool> CanUserPost(
            ITelegramBotClient botClient,
            long chatId,
            long userId)
        {
            var member = await botClient.GetChatMemberAsync(chatId, userId);

            return member switch
            {
                ChatMemberOwner => true,

                ChatMemberAdministrator admin =>
                    admin.CanPostMessages == true,

                _ => false
            };
        }

        public static async void CommandProcess(ITelegramBotClient botClient, Update update, Session session, Commander.CommandSet commandSet,CancellationToken cancellationToken)
        {
            try
            {
                await Commander.CommandProcess(
                    botClient,
                    update,
                    session,
                    commandSet,
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    $"Ошибка: {ex.Message}",
                    replyToMessageId: update.Message.MessageId,
                    cancellationToken: cancellationToken
                );
                Debug.Log(ex.Message, Debug.LogLevel.Error);

            }
        }
        public static async void CalloutProcess(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, Session session)
        {
            var cb = update.CallbackQuery;
            var data = cb.Data;
            try
            {
                if (session.ChatId == IDs.admin)
                {
                    
                        if (data.StartsWith("appr_"))
                        {
                            int msgId = int.Parse(data.Split('_')[1]);
                            await botClient.CopyMessageAsync(
                               chatId: IDs.ElitkaChanel,
                               fromChatId: IDs.admin,
                               messageId: msgId
                            );
                            await botClient.DeleteMessageAsync(IDs.admin, msgId);
                            await botClient.DeleteMessageAsync(IDs.admin, cb.Message.MessageId);
                            await botClient.AnswerCallbackQueryAsync(cb.Id, "Пост опубликован");
                        }
                        else if (data.StartsWith("decl_"))
                        {
                            var parts = data.Split('_');
                            string reason = parts[1];
                            int msgId = int.Parse(data.Split('_')[2]);
                            long userChatId = long.Parse(parts[3]);
                            int answMsgId = int.Parse(parts[4]);

                            string reasonText = reason switch
                            {
                                "nsfw" => "Обнаружены NSFW контент или «потерянная грань».",
                                "badmood" => "У админа плохое настроение, попробуйте позже.",
                                "dontlike" => "Админ не оценил пост по достоинству — не понравилось. Возможно, стоит запостить неанонимно?",
                                "sus" => "Повод поста или ваше поведение показалось подозрительным. Попробуйте позже или запостите неанонимно.",
                                _ => "Читаю... Причина не указана??? Это явно баг и об этом надо немедленно доложить!"
                            };

                            // уведомляем юзера
                            await botClient.SendTextMessageAsync(
                                userChatId,
                                $"Отклонено.\n\n" +
                                $"{reasonText}",
                                replyToMessageId: answMsgId
                            );
                            await botClient.DeleteMessageAsync(IDs.admin, msgId);
                            await botClient.DeleteMessageAsync(IDs.admin, cb.Message.MessageId);
                            await botClient.AnswerCallbackQueryAsync(cb.Id, "Пост отклонён");
                        }
                        else if (data.StartsWith("ban_"))
                        {
                            var parts = data.Split('_');
                            int msgId = int.Parse(data.Split('_')[1]);
                            long userChatId = long.Parse(parts[2]);

                            var sessionToBan = SessionManager.Get(userChatId, userChatId, SessionType.Private);
                            sessionToBan.PostingBan = true;

                            await botClient.SendTextMessageAsync(
                                userChatId,
                                $"После недавних попыток сделать анонимный пост, админ временно отключил вам эту возможность."
                            );

                            await botClient.DeleteMessageAsync(IDs.admin, msgId);
                            await botClient.DeleteMessageAsync(IDs.admin, cb.Message.MessageId);
                            await botClient.AnswerCallbackQueryAsync(cb.Id, "Юзер забанен");
                            Memory.SaveAll();
                        }

                    
                    
                }
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
                            "Эта кнопка не для тебя, хотя, а как ты её получил вообще?"
                        );
                        return;
                    }

                    var tempUpd = new Update { Message = new Message { From = new User { Id = userId }, Chat = new Chat { Id = userId } } };
                    var (text, totalPages) = await BuildHelpText(
                        botClient,
                        tempUpd,
                        PrivateDefaultCommandSet,
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
                        parseMode: ParseMode.MarkdownV2,
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
