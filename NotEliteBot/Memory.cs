using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static NotEliteBot.Chanel;
using static NotEliteBot.Dto;
using static NotEliteBot.Memory;

namespace NotEliteBot
{
    class Memory
    {
        public static bool AllowCustomSignature = false;
        public class ElimpLeader
        {
            public static long CurrentLeader = 1;
            public static long LeaderChat = 0;
            public static string LeaderMsg = "отсутствует";
        }
        public static Dictionary<string, DateTime> CooldownDict = new();
        public static Dictionary<string, Session> Sessions = new();
        public static List<ChannelPostInfo> PostStorage = new();
        public static List<string> BanWords = new();
        public static void SaveAll(string dir = "memory")
        {
            Directory.CreateDirectory(dir);

            SaveToFile(Path.Combine(dir, "bools.json"), new BoolDto
            {
                AllowCustomSignature = AllowCustomSignature
            });

            SaveToFile(Path.Combine(dir, "elimpLeader.json"), new ElimpLeaderDto
            {
                CurrentLeader = ElimpLeader.CurrentLeader,
                LeaderChat = ElimpLeader.LeaderChat,
                LeaderMsg = ElimpLeader.LeaderMsg
            });

            SaveToFile(Path.Combine(dir, "cooldowns.json"), CooldownDict);
            SaveToFile(Path.Combine(dir, "sessions.json"), Sessions);
            SaveToFile(Path.Combine(dir, "posts.json"), PostStorage);
            SaveToFile(Path.Combine(dir, "banwords.json"), BanWords);

        }
        public static void LoadAll(string dir = "memory")
        {
            var bools = LoadFromFile<BoolDto>(Path.Combine(dir, "bools.json"));
            if (bools != null)
                AllowCustomSignature = bools.AllowCustomSignature;

            var leader = LoadFromFile<ElimpLeaderDto>(Path.Combine(dir, "elimpLeader.json"));
            if (leader != null)
            {
                ElimpLeader.CurrentLeader = leader.CurrentLeader;
                ElimpLeader.LeaderChat = leader.LeaderChat;
                ElimpLeader.LeaderMsg = leader.LeaderMsg ?? "отсутствует";
            }

            CooldownDict = LoadFromFile<Dictionary<string, DateTime>>(Path.Combine(dir, "cooldowns.json")) ?? new();
            Sessions = LoadFromFile<Dictionary<string, Session>>(Path.Combine(dir, "sessions.json")) ?? new();
            PostStorage = LoadFromFile<List<ChannelPostInfo>>(Path.Combine(dir, "posts.json")) ?? new();
            BanWords = LoadFromFile<List<string>>(Path.Combine(dir, "banwords.json")) ?? new();
        }

    }
    class Dto
    {
        public class BoolDto
        {
            public bool AllowCustomSignature { get; set; }
        }

        public class ElimpLeaderDto
        {
            public long CurrentLeader { get; set; }
            public long LeaderChat { get; set; }
            public string LeaderMsg { get; set; }
        }
        private static JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static void SaveToFile<T>(string path, T data)
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            File.WriteAllText(path, json);
        }

        public static T LoadFromFile<T>(string path)
        {
            if (!File.Exists(path))
                return default;

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json);
        }

    }
}
