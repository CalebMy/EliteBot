using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace NotEliteBot
{
    class Chanel
    {
        public static async void Main(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                
                string signature = update.ChannelPost.AuthorSignature;
                if (update.ChannelPost.Chat.Id == IDs.ElitkaChanel && !Memory.AllowCustomSignature)
                {
                    var signatures = await GetAdminSignatures(botClient, update.ChannelPost.Chat.Id);
                    bool isValid = IsValidSignature(signature, signatures);
                    if (!isValid)
                    {
                        await botClient.DeleteMessageAsync(update.ChannelPost.Chat.Id, update.ChannelPost.MessageId);
                        Debug.Log($"Удалён пост с кастомной подписью: {update.ChannelPost.Text}", Debug.LogLevel.Info);
                        return;
                    }
                }
                if (string.IsNullOrEmpty(signature) && update.ChannelPost.Chat.Id == IDs.ElitkaChanel)
                {
                    await botClient.DeleteMessageAsync(update.ChannelPost.Chat.Id, update.ChannelPost.MessageId);
                    Debug.Log($"Удалена анонимка: {update.ChannelPost.Text}", Debug.LogLevel.Info);
                }
                else StorePost(update);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message, Debug.LogLevel.Error);
            }
        }
        public static void StorePost(Update update)
        {
            var post = update.ChannelPost;

            if (post == null) return;

            var info = new ChannelPostInfo
            {
                ChatId = post.Chat.Id,
                MessageId = post.MessageId,
                Text = post.Text ?? post.Caption,
                AuthorSignature = post.AuthorSignature
            };

            Memory.PostStorage.Add(info);
        }
        public class ChannelPostInfo
        {
            public long ChatId { get; set; }
            public int MessageId { get; set; }

            // подпись/текст
            public string Text { get; set; }

            // подпись автора (если есть)
            public string AuthorSignature { get; set; }

            // thread id (если пост участвует в discussion group)
            public int? ThreadId { get; set; }
        }

        public static ChannelPostInfo GetPost(long chatId, int messageId)
        {
            return Memory.PostStorage
                .FirstOrDefault(x => x.ChatId == chatId && x.MessageId == messageId);
        }
        public static ChannelPostInfo GetPostFromThred(long chatId, int? threadId)
        {
            return Memory.PostStorage
                .FirstOrDefault(x => x.ChatId == chatId && x.ThreadId == threadId);
        }
        public static bool CheckAuthorSignature(long chatId, int messageId, string signature)
        {
            try
            {
                var post = GetPost(chatId, messageId);

                if (post == null) return false;

                return post.AuthorSignature == signature;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message, Debug.LogLevel.Error);
                return false;
            }
        }
        public static async Task<HashSet<string>> GetAdminSignatures(
     ITelegramBotClient botClient,
     long channelId)
        {
            var result = new HashSet<string>();

            var admins = await botClient.GetChatAdministratorsAsync(channelId);

            foreach (var admin in admins)
            {
                // фильтр по правам
                if (admin is ChatMemberAdministrator a && a.CanPostMessages != true)
                    continue;

                if (admin is ChatMemberOwner || admin is ChatMemberAdministrator)
                {
                    var user = admin.User;
                    if (user == null) continue;

                    // --- 1. обычная подпись ---
                    string signature = string.Join(" ",
                        new[] { user.FirstName, user.LastName }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                    );

                    if (!string.IsNullOrWhiteSpace(signature))
                        result.Add(signature);

                    // --- 2. ассоциированные каналы ---
                    var session = SessionManager.Get(
                        user.Id,
                        user.Id,
                        SessionType.Private
                    );

                    if (session?.AssociatedIDs == null)
                        continue;

                    foreach (var assocId in session.AssociatedIDs)
                    {
                        try
                        {
                            var chat = await botClient.GetChatAsync(assocId);

                            if (!string.IsNullOrWhiteSpace(chat.Title))
                                result.Add(chat.Title);
                        }
                        catch
                        {
                            // бот не имеет доступа / канал не существует
                        }
                    }
                }
            }

            return result;
        }
        public static bool IsValidSignature(string input, HashSet<string> signatures)
        {
            return signatures.Contains(input);
        }
    }

}
