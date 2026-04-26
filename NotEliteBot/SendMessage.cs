using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace NotEliteBot
{
    public class InternalMessage
    {
        public long ChatId { get; set; }
        public int MessageId { get; set; }

        public int RemainingMessages { get; set; } // сколько осталось до удаления
    }
    public static class MessageManager
    {
        private static readonly List<InternalMessage> Messages = new();

        // --- отправка ---
        public static async Task<Message> SendAsync(
            ITelegramBotClient bot,
            long chatId,
            string text,
            int ttlMessages,
            int? replyToMessageId = null,
            IReplyMarkup? replyMarkup = null,
            ParseMode? parseMode = null)
        {
            try
            {
                var msg = await bot.SendTextMessageAsync(
                    chatId,
                    text,
                    replyToMessageId: replyToMessageId,
                    parseMode: parseMode,
                    replyMarkup: replyMarkup
                );

                // если -1 → не добавляем вообще
                if (ttlMessages >= 0)
                {
                    Messages.Add(new InternalMessage
                    {
                        ChatId = chatId,
                        MessageId = msg.MessageId,
                        RemainingMessages = ttlMessages
                    });
                }

                return msg;

            }
            catch (Exception ex)
            {
                Debug.Log("Ошибка при отправке сообщения: " + ex.Message, Debug.LogLevel.Error);
                return new Message();
            }
        }

        // --- тик при каждом апдейте ---
        public static async Task Tick(
            ITelegramBotClient bot,
            Update update,
            int weight = 1)
        {
            var msg = update.Message;
            if (msg == null) return;

            long chatId = msg.Chat.Id;

            // уменьшаем только для этого чата
            var toRemove = new List<InternalMessage>();

            foreach (var m in Messages.Where(x => x.ChatId == chatId))
            {
                m.RemainingMessages -= weight;

                if (m.RemainingMessages <= 0)
                    toRemove.Add(m);
            }

            foreach (var m in toRemove)
            {
                try
                {
                    await bot.DeleteMessageAsync(m.ChatId, m.MessageId);
                }
                catch
                {
                    // игнор (сообщение уже удалено или нет прав)
                }

                Messages.Remove(m);
            }
        }
    }
}
