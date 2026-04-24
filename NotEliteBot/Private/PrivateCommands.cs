using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NotEliteBot.Commander;
using static System.Collections.Specialized.BitVector32;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace NotEliteBot
{
    partial class Private
    {
        private static CommandSet PrivateDefaultCommandSet = new CommandSet();
        private static CommandSet PrivateCancelSet = new CommandSet();
        public static void RegisterPrivateCommands()
        {
            foreach (var cmd in PrivateCommandsDefault)
            {
                PrivateDefaultCommandSet.Commands.Add(cmd);
            }
            PrivateCancelSet.Commands.Add(PrivateCancel[0]);
        }

        private static readonly List<CommandDefinition> PrivateCommandsDefault = new List<CommandDefinition>()
        {
            // Банворды (добавить, удалить, список)
            new CommandDefinition()
            {
                Name = "banword",
                Description = "управление банвордами. Допустимый первый аргумент: add, remove, list",
                Arguments = {
                    new ArgumentDefinition { Type = ArgumentType.Word },
                    new ArgumentDefinition { Type = ArgumentType.Rest, Optional = true }
                },
                AllowedUserIds = { IDs.admin, IDs.Ironman },
                Execute = async ctx =>
                {
                    var bot = ctx.Bot;
                    var msg = ctx.Update.Message;

                    string action = (string)ctx.Args[0];

                    if (action == "add")
                    {
                        if (ctx.Args.Count < 2)
                        {
                            await bot.SendTextMessageAsync(msg.Chat.Id, "Укажите слово для добавления", replyToMessageId: msg.MessageId);
                            return;
                        }
                        string wordToAdd = (string)ctx.Args[1];
                        if (!Memory.BanWords.Contains(wordToAdd))
                        {
                            Memory.BanWords.Add(wordToAdd);
                            Memory.SaveAll();
                            await bot.SendTextMessageAsync(msg.Chat.Id, $"Слово '{wordToAdd}' добавлено в банворды", replyToMessageId: msg.MessageId);
                        }
                        else
                        {
                            await bot.SendTextMessageAsync(msg.Chat.Id, $"Слово '{wordToAdd}' уже есть в банвордах", replyToMessageId: msg.MessageId);
                        }
                    }
                    else if (action == "remove")
                    {
                        if (ctx.Args.Count < 2)
                        {
                            await bot.SendTextMessageAsync(msg.Chat.Id, "Укажите слово для удаления", replyToMessageId: msg.MessageId);
                            return;
                        }
                        string wordToRemove = (string)ctx.Args[1];
                        if (Memory.BanWords.Remove(wordToRemove))
                        {
                            Memory.SaveAll();
                            await bot.SendTextMessageAsync(msg.Chat.Id, $"Слово '{wordToRemove}' удалено из банвордов", replyToMessageId: msg.MessageId);
                        }
                        else
                        {
                            await bot.SendTextMessageAsync(msg.Chat.Id, $"Слово '{wordToRemove}' не найдено в банвордах", replyToMessageId: msg.MessageId);
                        }
                    }
                    else if (action == "list")
                    {
                        if (Memory.BanWords.Count == 0)
                        {
                            await bot.SendTextMessageAsync(msg.Chat.Id, "Список банвордов пуст", replyToMessageId: msg.MessageId);
                        }
                        else
                        {
                            string list = string.Join("\n", Memory.BanWords.Select(w => $"- {w}"));
                            await bot.SendTextMessageAsync(msg.Chat.Id, $"Банворды:\n{list}", replyToMessageId: msg.MessageId);
                        }
                    }
                    else
                    {
                         await bot.SendTextMessageAsync(msg.Chat.Id, $"Неизвестное действие! Допустимы только add, remove, list", replyToMessageId: msg.MessageId);
                    }
                }
            },
            // Добавить ассоциативный айди
            new CommandDefinition()
            {
                Name = "id_link",
                Description = "добавить ассоциативный айди",
                Arguments = {
                    new ArgumentDefinition { Type = ArgumentType.Word },
                    new ArgumentDefinition { Type = ArgumentType.Word }
                },
                AllowedUserIds = { IDs.admin },
                Execute = async ctx =>
                {
                    var bot = ctx.Bot;
                    var msg = ctx.Update.Message;

                    string userArg = (string)ctx.Args[0];
                    string channelArg = (string)ctx.Args[1];

                    long userId;
                    long channelId;

                    // --- 1. парсим юзера ---
                    if (userArg.StartsWith("@"))
                    {
                        var chat = await bot.GetChatAsync(userArg);
                        userId = chat.Id;
                    }
                    else
                    {
                        if (!long.TryParse(userArg, out userId))
                            throw new Exception("Неверный user id");
                    }

                    // --- 2. парсим канал ---
                    if (channelArg.StartsWith("@"))
                    {
                        var chat = await bot.GetChatAsync(channelArg);
                        channelId = chat.Id;
                    }
                    else
                    {
                        if (!long.TryParse(channelArg, out channelId))
                            throw new Exception("Неверный channel id");
                    }

                    // --- 3. получаем сессию ---
                    var session = SessionManager.Get(
                        userId,
                        userId,
                        SessionType.Private
                    );

                    session.AssociatedIDs ??= new List<long>();

                    // --- 4. добавляем ---
                    if (!session.AssociatedIDs.Contains(channelId))
                    {
                        session.AssociatedIDs.Add(channelId);

                        await bot.SendTextMessageAsync(
                            msg.Chat.Id,
                            $"Связка добавлена:\n{userId} → {channelId}",
                            replyToMessageId: msg.MessageId
                        );
                        Memory.SaveAll();
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(
                            msg.Chat.Id,
                            $"Уже существует",
                            replyToMessageId: msg.MessageId
                        );
                    }
                }
            },
            // Удалить
            new CommandDefinition()
            {
                Name = "id_unlink",
                Description = "удалить ассоциативный айди",
                Arguments = {
                    new ArgumentDefinition { Type = ArgumentType.Word },
                    new ArgumentDefinition { Type = ArgumentType.Word }
                },
                AllowedUserIds = { IDs.admin },

                Execute = async ctx =>
                {
                    var bot = ctx.Bot;
                    var msg = ctx.Update.Message;

                    string userArg = (string)ctx.Args[0];
                    string channelArg = (string)ctx.Args[1];

                    long userId;
                    long channelId;

                    // --- 1. парсим юзера ---
                    if (userArg.StartsWith("@"))
                    {
                        var chat = await bot.GetChatAsync(userArg);
                        userId = chat.Id;
                    }
                    else
                    {
                        if (!long.TryParse(userArg, out userId))
                            throw new Exception("Неверный user id");
                    }

                    // --- 2. парсим канал ---
                    if (channelArg.StartsWith("@"))
                    {
                        var chat = await bot.GetChatAsync(channelArg);
                        channelId = chat.Id;
                    }
                    else
                    {
                        if (!long.TryParse(channelArg, out channelId))
                            throw new Exception("Неверный channel id");
                    }

                    // --- 3. получаем сессию ---
                    var session = SessionManager.Get(
                        userId,
                        userId,
                        SessionType.Private
                    );

                    if (session.AssociatedIDs == null || session.AssociatedIDs.Count == 0)
                    {
                        await bot.SendTextMessageAsync(
                            msg.Chat.Id,
                            "Список ассоциаций пуст",
                            replyToMessageId: msg.MessageId
                        );
                        return;
                    }

                    // --- 4. удаляем ---
                    if (session.AssociatedIDs.Remove(channelId))
                    {
                        await bot.SendTextMessageAsync(
                            msg.Chat.Id,
                            $"Связка удалена:\n{userId} → {channelId}",
                            replyToMessageId: msg.MessageId
                        );
                        Memory.SaveAll();
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(
                            msg.Chat.Id,
                            $"Такой связки нет",
                            replyToMessageId: msg.MessageId
                        );
                    }
                }
            },
            // Разбан анон постинга
            new CommandDefinition()
            {
                Name = "pardon_post",
                Description = "вернуть пользователю возможность постить анонимно",
                Arguments = { new ArgumentDefinition { Type = ArgumentType.Long } },
                AllowedUserIds = { IDs.admin },
                Execute = async ctx =>
                {
                    var sessionToPardon = SessionManager.Get((long)ctx.Args[0], (long)ctx.Args[0], SessionType.Private);
                    sessionToPardon.PostingBan = false;
                    await ctx.Bot.SendTextMessageAsync(
                        ctx.Session.Id,
                        "Юзер снова может постить анонимно");
                    Memory.SaveAll();
                }
            },
            // Анонимный пост
            new CommandDefinition()
            {
                Name = "post",
                Description = "сделать анонимный* пост в Элитке (только для постящих админов канала)",
                Arguments = {},
                Execute = async ctx =>
                {
                    if (ctx.Session.PostingBan == true)
                    {
                        await ctx.Bot.SendTextMessageAsync(ctx.Session.Id, "Вам временно отключили возможность делать анонимные посты", cancellationToken: ctx.CancellationToken);
                        Debug.Log($"Забаненный пытался сделать пост", Debug.LogLevel.Info);
                        return;
                    }
                    if (CanUserPost(ctx.Bot, IDs.ElitkaChanel, ctx.Session.Id).Result == false)
                    {
                        await ctx.Bot.SendTextMessageAsync(ctx.Session.Id, "У вас нет прав на публикацию постов в Элитке", cancellationToken: ctx.CancellationToken);
                        Debug.Log($"Неуспешная попытка сделать пост", Debug.LogLevel.Info);
                        return;
                    }
                    else
                    {
                        await ctx.Bot.SendTextMessageAsync(
                            ctx.Session.Id,
                            "Напишите пост, который хотите отправить анонимно* в Элитку. Его запостят после одобрения.\n\n" +
                            "При необходимости ваша личность может быть раскрыта, однако для этого вы должны быть преследуемыми по законам ЭЭ.\nВведите /cancel для отмены действия.",
                            cancellationToken: ctx.CancellationToken);
                        ctx.Session.Mode = Mode.AnonymousPost;
                        Memory.SaveAll();
                    }
                }
            }
        };
        private static readonly List<CommandDefinition> PrivateCancel = new List<CommandDefinition>()
        {
            // Анонимный пост
            new CommandDefinition()
            {
                Name = "cancel",
                Description = "отменить действие",
                Arguments = {},
                Execute = async ctx =>
                {
                    await ctx.Bot.SendTextMessageAsync(ctx.Session.Id, "Действие отменено", cancellationToken: ctx.CancellationToken);
                    ctx.Session.Mode = Mode.Default;
                    Memory.SaveAll();
                    return;
                }
            }
        };
    }

}
