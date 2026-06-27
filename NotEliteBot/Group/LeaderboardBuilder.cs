using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace NotEliteBot
{
    public static class LeaderboardBuilder
    {
        public class Entry
        {
            public long Id { get; set; }
            public int Value { get; set; }
        }

        public static async Task<string> BuildSection(
            ITelegramBotClient bot,
            List<Entry> items,
            string title,
            Func<int, string> wordForm,
            long currentUserId,
            string yourLabel)
        {
            var top = items.OrderByDescending(e => e.Value).Take(10).ToList();
            var sb = new StringBuilder();

            sb.AppendLine(title);

            for (int i = 0; i < top.Count; i++)
            {
                try
                {
                    var user = await bot.GetChatAsync(top[i].Id);
                    string name = $"{user.FirstName} {user.LastName}";
                    sb.AppendLine($"{i + 1}. {name} — {top[i].Value} {wordForm(top[i].Value)}");
                }
                catch { }
            }

            var current = items.FirstOrDefault(e => e.Id == currentUserId);
            if (current != null && !top.Any(e => e.Id == currentUserId))
            {
                sb.AppendLine();
                sb.AppendLine($"{yourLabel}: {current.Value} {wordForm(current.Value)}");
            }

            return sb.ToString();
        }
    }
}
