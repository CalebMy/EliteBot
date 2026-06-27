using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using static NotEliteBot.Commander;

namespace NotEliteBot
{
    partial class Group
    {
        const double BASE_CHANCE = 0.10;
        const double MIN_CHANCE = 0.00;
        const double MAX_CHANCE = 0.20;

        public static List<CommandDefinition> GetElimpCommands()
        {
            return new List<CommandDefinition>
            {
                new CommandDefinition
                {
                    Name = "elimp",
                    Description = "покорите гору Элимп и защитите честь своего Чата!",
                    Arguments = new() { new ArgumentDefinition { Type = ArgumentType.Rest, Optional = true } },
                    Execute = async ctx =>
                    {
                        Random rnd = new();
                        string key1 = $"elimp_global";
                        TimeSpan remaining;
                        bool isOnCooldown = !Commander.Cooldowns.TryUse(key1, TimeSpan.FromHours(Memory.ElimpLeader.Cooldown), out remaining);
                        if (Memory.ElimpLeader.CurrentLeader != 0 && isOnCooldown)
                        {
                            if (ctx.Update.Message.From.Id == Memory.ElimpLeader.CurrentLeader && Memory.ElimpLeader.LeaderMsg == "отсутствует" && !string.IsNullOrEmpty((string)ctx.Args[0]))
                            {
                                if (((string)ctx.Args[0]).Length > 250)
                                {
                                    await MessageManager.SendAsync(
                                        ctx.Bot,
                                        ctx.Update.Message.Chat.Id,
                                        $"Девиз слишком длинный! Максимум 250 символов.",
                                        5,
                                        replyToMessageId: ctx.Update.Message.MessageId
                                    );
                                    return;
                                }
                                Memory.ElimpLeader.LeaderMsg = $"\n\n«{(string)ctx.Args[0]}»".Replace("@", "");
                                await MessageManager.SendAsync(
                                ctx.Bot,
                                ctx.Update.Message.Chat.Id,
                                $"Девиз добавлен",
                                5,
                                replyToMessageId: ctx.Update.Message.MessageId
                                );
                                Memory.SaveAll();
                                return;
                            }
                            var leaderInfo = await ctx.Bot.GetChatMemberAsync(Memory.ElimpLeader.LeaderChat, Memory.ElimpLeader.CurrentLeader);
                            var leaderChatInfo = await ctx.Bot.GetChatAsync(Memory.ElimpLeader.LeaderChat);
                            await MessageManager.SendAsync(
                                ctx.Bot,
                                ctx.Update.Message.Chat.Id,
                                $"Гора Элимп уже покорена\n\n" +
                                $"Текущий лидер: {leaderInfo.User.FirstName} {leaderInfo.User.LastName}\n" +
                                $"Флаг чата: {leaderChatInfo.Title}\n" +
                                $"Девиз лидера: {Memory.ElimpLeader.LeaderMsg}\n\n" +
                                $"Гора опустеет через: {remaining.Hours}ч. {remaining.Minutes}м. {remaining.Seconds}c.",
                                5,
                                replyToMessageId: ctx.Update.Message.MessageId
                                );
                            return;
                        }
                        else if (Memory.ElimpLeader.CurrentLeader != 0 && !isOnCooldown)
                        {
                            List<long> NotifyChats = new()
                            {
                                IDs.AlitkaChat,
                                IDs.ElitkaChat,
                            };
                            Memory.ElimpLeader.CurrentLeader = 0;
                            string text = "Гора Элимп опустела!\nПокорите её!";

                            long currentChatId = ctx.Update.Message.Chat.Id;

                            await MessageManager.SendAsync(ctx.Bot, currentChatId, text, -1);

                            foreach (var chatId in NotifyChats)
                            {
                                if (chatId == currentChatId)
                                    continue;

                                try
                                {
                                      await MessageManager.SendAsync(ctx.Bot, chatId, text, -1);
                                }
                                catch { }
                            }

                            return;
                        }
                        string key2 = $"elimptry_user_{ctx.Update.Message.From.Id}";

                        var UserPrivateSession = SessionManager.Get(
                            ctx.Session.Id, ctx.Session.Id, SessionType.Private
                        );

                        double chance = UserPrivateSession.ElimpChance;

                        DateTime now = DateTime.UtcNow;

                        double secondsSinceLast =
                            (now - UserPrivateSession.LastElimpTry).TotalSeconds;
                        string perfomanceMsg = secondsSinceLast <= 15 ? "(+%)" :
                            "(=%)";
                        UserPrivateSession.LastElimpTry = now;

                        if (secondsSinceLast < 10)
                        {
                            double penaltyFactor = (10 - secondsSinceLast) / 10.0;
                            chance -= 0.02 * penaltyFactor;
                        }
                        else if (secondsSinceLast <= 15)
                        {
                            double bonusFactor = (15 - secondsSinceLast) / 5.0;
                            chance += 0.02 * bonusFactor;
                        }
                        else
                        {
                            if (chance > BASE_CHANCE)
                                chance -= 0.005;
                            else if (chance < BASE_CHANCE)
                                chance += 0.005;
                        }

                        chance = Math.Clamp(chance, MIN_CHANCE, MAX_CHANCE);
                        UserPrivateSession.ElimpChance = chance;

                        if (!Commander.Cooldowns.TryUse(key2, TimeSpan.FromSeconds(10), out remaining))
                        {
                            await MessageManager.SendAsync(
                                ctx.Bot,
                                ctx.Update.Message.Chat.Id,
                                $"Ты устал! (-%) КД: {remaining.Seconds}с.\nГора Элимп остаётся пустой!",
                                3,
                                replyToMessageId: ctx.Update.Message.MessageId
                            );
                            return;
                        }
                        else if (rnd.NextDouble() < chance)
                        {
                            Memory.ElimpLeader.CurrentLeader = ctx.Update.Message.From.Id;
                            Memory.ElimpLeader.LeaderChat = ctx.Update.Message.Chat.Id;
                            var arg = (string)ctx.Args[0];
                            Memory.ElimpLeader.LeaderMsg =
                            string.IsNullOrEmpty(arg)
                                ? "отсутствует"
                                : arg.Length > 250
                                    ? "отсутствует"
                                    : $"\n\n«{arg}»".Replace("@", "");
                           await MessageManager.SendAsync(
                                ctx.Bot,
                                ctx.Update.Message.Chat.Id,
                                $"Гора Элимп покорена!\n\n" +
                                $"Новый лидер: {ctx.Update.Message.From.FirstName} {ctx.Update.Message.From.LastName}\n" +
                                $"Новый флаг чата: {ctx.Update.Message.Chat.Title}\n" +
                                $"Новый девиз лидера: {Memory.ElimpLeader.LeaderMsg}",
                                -1,
                                replyToMessageId: ctx.Update.Message.MessageId
                            );
                            Cooldowns.TryUse(key1, TimeSpan.FromSeconds(1), out remaining);

                            var ChatSession = SessionManager.Get(ctx.Session.ChatId, ctx.Session.ChatId, SessionType.Group);
                            UserPrivateSession.ConqestedElimp++;
                            ChatSession.ConqestedElimp++;
                            Memory.ElimpLeader.Cooldown = rnd.Next(3,7);
                            Memory.SaveAll();
                            return;
                        }
                        else
                        {
                            await MessageManager.SendAsync(
                                ctx.Bot,
                                ctx.Update.Message.Chat.Id,
                                $"Не удалось! {perfomanceMsg}\nГора Элимп остаётся пустой!",
                                3,
                                replyToMessageId: ctx.Update.Message.MessageId
                            );
                            return;
                        }
                    }
                },
                new CommandDefinition
                {
                    Name = "elimp_stat",
                    Description = "статистика Элимпа",
                    Arguments = new() { },
                    Execute = async ctx =>
                    {
                        var allSessions = Memory.Sessions.Values;
                        if (!allSessions.Any(s => s.ConqestedElimp > 0))
                        {
                            await MessageManager.SendAsync(
                                ctx.Bot,
                                ctx.Update.Message.Chat.Id,
                                "Пока что никто не покорял Элимп",
                                10,
                                replyToMessageId: ctx.Update.Message.MessageId
                            );
                            return;
                        }

                        var peopleEntries = allSessions
                            .Where(s => s.ConqestedElimp > 0 && s.SessionType == SessionType.Private)
                            .Select(s => new LeaderboardBuilder.Entry { Id = s.Id, Value = s.ConqestedElimp })
                            .ToList();

                        var chatEntries = allSessions
                            .Where(s => s.ConqestedElimp > 0 && s.SessionType == SessionType.Group)
                            .Select(s => new LeaderboardBuilder.Entry { Id = s.Id, Value = s.ConqestedElimp })
                            .ToList();

                        long uid = ctx.Update.Message.From.Id;

                        var sb = new StringBuilder();
                        sb.AppendLine(await LeaderboardBuilder.BuildSection(ctx.Bot, peopleEntries, "Топ 10 лидеров:", GetConqWord, uid, "Вы покорили Элимп"));

                        sb.AppendLine();

                        var currentChat = allSessions.FirstOrDefault(s =>
                            s.ChatId == ctx.Update.Message.Chat.Id &&
                            s.SessionType == SessionType.Group && s.ConqestedElimp > 0);

                        var topChats = chatEntries.OrderByDescending(e => e.Value).Take(10).ToList();
                        sb.AppendLine("Статистика Чатов:");
                        for (int i = 0; i < topChats.Count; i++)
                        {
                            try
                            {
                                var chat = await ctx.Bot.GetChatAsync(topChats[i].Id);
                                sb.AppendLine($"{i + 1}. {chat.Title} — {topChats[i].Value} {GetConqWord(topChats[i].Value)}");
                            }
                            catch { }
                        }

                        if (currentChat != null && !topChats.Any(e => e.Id == currentChat.ChatId))
                        {
                            sb.AppendLine();
                            sb.AppendLine($"Этот чат покорил Элимп {currentChat.ConqestedElimp} раз(а)");
                        }

                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            sb.ToString(),
                            10,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                    },
                },
                new CommandDefinition
                {
                    Name = "elimp_help",
                    Description = "узнайте как покорить Элимп",
                    Arguments = new() { },
                    Execute = async ctx =>
                    {
                        string text =
                            "Гора Элимп это священный пьедестал божеств, который один на все сущие чаты.\n\n" +
                            "Используйте /elimp, чтобы попытаться покорить её.\n" +
                            "После захвата, лидер Элимпа устанавливает на нём флаг своего чата и девиз. Спустя некоторое время гора опустеет и её можно будет покорить снова.\n\n" +
                            "Базовый шанс покорения: 10%.\n" +
                            "Если спамить попытками — шанс начнёт падать.\n" +
                            "Если ловить хороший ритм — шанс может вырасти вплоть до 20%.\n\n" +
                            "Идеальное время между попытками — сразу после окончания кулдауна.";
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            text,
                            5,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                    }
                }
            };
        }

        public static string GetConqWord(int n)
        {
            int n100 = n % 100;
            int n10 = n % 10;

            if (n100 >= 11 && n100 <= 14)
                return "покорений";

            return n10 switch
            {
                1 => "покорение",
                2 or 3 or 4 => "покорения",
                _ => "покорений"
            };
        }
    }
}
