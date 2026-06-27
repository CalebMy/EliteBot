using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using static NotEliteBot.Commander;

namespace NotEliteBot
{
    public class VapeData
    {
        public int Streak { get; set; }
        public int Total { get; set; }
        public int Deaths { get; set; }
    }

    partial class Group
    {
        private static VapeData GetVapeData(long userId)
        {
            if (!Memory.VapeStats.TryGetValue(userId, out var data))
            {
                data = new VapeData();
                Memory.VapeStats[userId] = data;
            }
            return data;
        }

        private static readonly string[] VapeNormalMessages =
        {
            "|Вы курите /VAPE| |✅|",
            "|Вы парите| |💨|",
            "|Вы затягиваетесь| |🌬|",
            "|Вы курите| |🚬|",
        };

        private static readonly string[] VapeRareMessages =
        {
            "|Вы делаете затяжку века| |🏆|",
            "|Вы парите как паровоз| |🚂|",
            "|Облако пара накрыло всех| |☁️|",
            "|Вы — легенда вейпинга| |👑|",
        };

        public static List<CommandDefinition> GetVapeCommands()
        {
            return new List<CommandDefinition>
            {
                new CommandDefinition
                {
                    Name = "vape",
                    Description = "Легендарный /VAPE — Made by t.me/SimplyIgor",
                    Arguments = new() { },
                    Execute = async ctx =>
                    {
                        string key = $"vape_{ctx.Update.Message.From.Id}";
                        if (!Commander.Cooldowns.TryUse(key, TimeSpan.FromSeconds(5), out var remaining))
                        {
                            await MessageManager.SendAsync(
                                ctx.Bot,
                                ctx.Update.Message.Chat.Id,
                                $"Ты устал! Отдохни {remaining.Seconds}с.",
                                3,
                                replyToMessageId: ctx.Update.Message.MessageId
                            );
                            return;
                        }

                        var data = GetVapeData(ctx.Update.Message.From.Id);

                        Random rnd = new();
                        bool died = rnd.NextDouble() < 0.12;

                        string response;
                        if (died)
                        {
                            data.Streak = 0;
                            data.Deaths++;
                            response = "|Вы умерли от рака лёгких| |💀|";
                        }
                        else
                        {
                            data.Streak++;
                            data.Total++;

                            if (rnd.NextDouble() < 0.25)
                                response = VapeRareMessages[rnd.Next(VapeRareMessages.Length)];
                            else
                                response = VapeNormalMessages[rnd.Next(VapeNormalMessages.Length)];
                        }

                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            response,
                            5,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                    }
                },
                new CommandDefinition
                {
                    Name = "vape_stat",
                    Description = "Cтатистика /VAPE — Made by t.me/SimplyIgor",
                    Arguments = new() { },
                    Execute = async ctx =>
                    {
                        if (!Memory.VapeStats.Any(kv => kv.Value.Total > 0))
                        {
                            await MessageManager.SendAsync(
                                ctx.Bot,
                                ctx.Update.Message.Chat.Id,
                                "Пока что никто не вэйпил",
                                10,
                                replyToMessageId: ctx.Update.Message.MessageId
                            );
                            return;
                        }

                        var entries = Memory.VapeStats
                            .Where(kv => kv.Value.Total > 0)
                            .Select(kv => new LeaderboardBuilder.Entry { Id = kv.Key, Value = kv.Value.Streak })
                            .ToList();

                        var totalEntries = Memory.VapeStats
                            .Where(kv => kv.Value.Total > 0)
                            .Select(kv => new LeaderboardBuilder.Entry { Id = kv.Key, Value = kv.Value.Total })
                            .ToList();

                        var deathsEntries = Memory.VapeStats
                            .Where(kv => kv.Value.Total > 0)
                            .Select(kv => new LeaderboardBuilder.Entry { Id = kv.Key, Value = kv.Value.Deaths })
                            .ToList();

                        long uid = ctx.Update.Message.From.Id;

                        var sb = new StringBuilder();
                        sb.AppendLine(await LeaderboardBuilder.BuildSection(ctx.Bot, entries, "💨 Топ 10 по текущей серии:", GetVapeWord, uid, "Ваша серия"));
                        sb.AppendLine();
                        sb.AppendLine(await LeaderboardBuilder.BuildSection(ctx.Bot, totalEntries, "💨 Топ 10 по общему количеству затяжек:", GetVapeWord, uid, "Всего затяжек"));
                        sb.AppendLine();
                        sb.AppendLine(await LeaderboardBuilder.BuildSection(ctx.Bot, deathsEntries, "💀 Топ 10 по смертям:", GetDeathWord, uid, "Ваши смерти"));

                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            sb.ToString(),
                            10,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                    }
                }
            };
        }

        private static string GetVapeWord(int n)
        {
            int n100 = n % 100;
            int n10 = n % 10;

            if (n100 >= 11 && n100 <= 14)
                return "затяжек";

            return n10 switch
            {
                1 => "затяжка",
                2 or 3 or 4 => "затяжки",
                _ => "затяжек"
            };
        }

        private static string GetDeathWord(int n)
        {
            int n100 = n % 100;
            int n10 = n % 10;

            if (n100 >= 11 && n100 <= 14)
                return "смертей";

            return n10 switch
            {
                1 => "смерть",
                2 or 3 or 4 => "смерти",
                _ => "смертей"
            };
        }
    }
}
