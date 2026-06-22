using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace NotEliteBot
{
    public class Session
    {
        public long Id { get; set; }
        public long ChatId { get; set; }
        public SessionType SessionType { get; set; }
        public Mode Mode { get; set; }
        public bool PostingBan { get; set; }
        public List<long> AssociatedIDs { get; set; }
        public int ConqestedElimp { get; set; }
        public bool BotBan { get; set; }
        public int Testosterone { get; set; } // от 0 до 1000
        public List<string> SmokeActions { get; set; }
        public int CurrentSmokeDurability { get; set; }
        public string ShortAdress { get; set; }
        public double ElimpChance { get; set; } = 0.10;
        public DateTime LastElimpTry { get; set; }
        public int VAPEStreak { get; set; }
        public int TotalVAPE { get; set; }
        public int VAPEDeaths { get; set; }
    }
    public enum SessionType
    {
        Private,
        Chanel,
        Group
    }
    public enum Mode
    {
        Default,
        AnonymousPost
    }
    public class SessionManager
    {

        public static Session Get(long id, long chatId, SessionType type)
        {
            var key = $"{id}_{chatId}_{type}";

            if (!Memory.Sessions.TryGetValue(key, out var session))
            {
                session = new Session
                {
                    Id = id,
                    ChatId = chatId,
                    SessionType = type,
                    Mode = Mode.Default,
                    PostingBan = false,
                    AssociatedIDs = { },
                    ConqestedElimp = 0,
                    Testosterone = 200,
                    SmokeActions = new List<string>(),
                    CurrentSmokeDurability = 0,
                    BotBan = false,
                    ShortAdress = "",
                    ElimpChance = 0.10,
                    LastElimpTry = DateTime.Now,
                    VAPEStreak = 0,
                    TotalVAPE = 0,
                    VAPEDeaths = 0,
                };

                Memory.Sessions[key] = session;
            }

            return session;
        }
    }
}