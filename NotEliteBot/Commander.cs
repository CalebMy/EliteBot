using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Runtime.InteropServices;

namespace NotEliteBot
{
    class Commander
    {
        public static class CommandConfig
        {
            public static List<string> Prefixes = new() { "/", "!" };

            public static string BotUsername = "I_am_NotElite_bot";
        }
        public class CommandDefinition
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public List<ArgumentDefinition> Arguments { get; set; } = new();

            public List<long> AllowedUserIds { get; set; } = new();
            public List<ChatMemberStatus> AllowedStatuses { get; set; } = new();

            public Func<CommandContext, Task> Execute { get; set; }
        }
        public class ArgumentDefinition
        {
            public ArgumentType Type { get; set; }
            public bool Optional { get; set; }
        }

        public enum ArgumentType
        {
            Int,
            Long,
            Word,
            QuotedString,
            Rest
        }
        public class CommandContext
        {
            public ITelegramBotClient Bot { get; set; }
            public Update Update { get; set; }
            public Session Session { get; set; }
            public CancellationToken CancellationToken { get; set; }
            public List<object> Args { get; set; }
        }
        public class CommandSet
        {
            public List<CommandDefinition> Commands { get; set; } = new();
        }
        public static class CommandParser
        {
            public static (string command, string argsRaw)? Extract(string text)
            {
                foreach (var prefix in CommandConfig.Prefixes)
                {
                    if (!text.StartsWith(prefix))
                        continue;

                    var withoutPrefix = text.Substring(prefix.Length);

                    var split = withoutPrefix.Split(' ', 2);
                    var cmdPart = split[0];

                    // @bot поддержка
                    if (cmdPart.Contains("@"))
                    {
                        var parts = cmdPart.Split('@');

                        if (parts[1] != CommandConfig.BotUsername)
                            return null;

                        cmdPart = parts[0];
                    }

                    var args = split.Length > 1 ? split[1] : "";

                    return (cmdPart, args);
                }

                return null;
            }
            public static List<string> Tokenize(string input)
            {
                var result = new List<string>();
                var current = "";
                bool inQuotes = false;

                for (int i = 0; i < input.Length; i++)
                {
                    var c = input[i];

                    if (c == '"')
                    {
                        inQuotes = !inQuotes;
                        continue;
                    }

                    if (c == ' ' && !inQuotes)
                    {
                        if (current.Length > 0)
                        {
                            result.Add(current);
                            current = "";
                        }
                    }
                    else
                    {
                        current += c;
                    }
                }

                if (current.Length > 0)
                    result.Add(current);

                return result;
            }
        }
        public static class ArgumentParser
        {
            public static List<object> Parse(List<string> tokens, List<ArgumentDefinition> defs)
            {
                var result = new List<object>();
                int index = 0;

                foreach (var def in defs)
                {
                    if (def.Type == ArgumentType.Rest)
                    {
                        var rest = string.Join(" ", tokens.Skip(index));
                        result.Add(rest);
                        return result;
                    }

                    if (index >= tokens.Count)
                    {
                        if (def.Optional)
                        {
                            result.Add(null);
                            continue;
                        }

                        throw new Exception("Недостаточно аргументов");
                    }

                    var token = tokens[index];

                    switch (def.Type)
                    {
                        case ArgumentType.Int:
                            if (!int.TryParse(token, out var val))
                                throw new Exception($"Ожидалось число: {token}");
                            result.Add(val);
                            break;
                        case ArgumentType.Long:
                            if (!long.TryParse(token, out var longVal))
                                throw new Exception($"Ожидалось число: {token}");
                            result.Add(longVal);
                            break;
                        case ArgumentType.Word:
                            result.Add(token);
                            break;

                        case ArgumentType.QuotedString:
                            result.Add(token);
                            break;
                    }

                    index++;
                }

                return result;
            }
        }
        public static async Task CommandProcess(
            ITelegramBotClient botClient,
            Update update,
            Session session,
            CommandSet commandSet,
            CancellationToken ct)
                {
                    var msg = update.Message;
                    if (msg?.Text == null) return;
                    if (msg.From == null) return;

                    var extracted = CommandParser.Extract(msg.Text);
                    if (extracted == null) return;

                    var (cmdName, argsRaw) = extracted.Value;
                    // --- встроенный help ---
                    if (cmdName == "help")
                    {
                        await HandleHelp(botClient, update, commandSet);
                        return;
                    }
                    var command = commandSet.Commands
                        .FirstOrDefault(c => c.Name == cmdName);

                    if (command == null) return;

                    // --- userId фильтр ---
                    if (command.AllowedUserIds.Any() &&
                        !command.AllowedUserIds.Contains(msg.From.Id))
                        return;

                    // --- статус фильтр ---
                    if (command.AllowedStatuses.Any())
                    {
                        var member = await botClient.GetChatMemberAsync(
                            msg.Chat.Id,
                            msg.From.Id
                        );

                        var status = member switch
                        {
                            ChatMemberOwner => ChatMemberStatus.Creator,
                            ChatMemberAdministrator => ChatMemberStatus.Administrator,
                            ChatMemberMember => ChatMemberStatus.Member,
                            ChatMemberRestricted => ChatMemberStatus.Restricted,
                            ChatMemberLeft => ChatMemberStatus.Left,
                            ChatMemberBanned => ChatMemberStatus.Kicked,
                            _ => throw new Exception("Unknown ChatMember type")
                        };

                        if (!command.AllowedStatuses.Contains(status))
                            return;
                    }

                    // --- аргументы ---
                    var tokens = CommandParser.Tokenize(argsRaw);
                    var args = ArgumentParser.Parse(tokens, command.Arguments);

                    await command.Execute(new CommandContext
                    {
                        Bot = botClient,
                        Update = update,
                        Session = session,
                        Args = args,
                        CancellationToken = ct
                    });

                    Debug.Log($"Выполнена команда {msg.Text}", Debug.LogLevel.Info);
                }
        private static async Task HandleHelp(
            ITelegramBotClient botClient,
            Update update,
            CommandSet commandSet,
            int page = 0)
        {
            var msg = update.Message;
            if (msg?.From == null) return;

            var (text, totalPages) = await BuildHelpText(
                botClient,
                update,
                commandSet,
                page
            );

            InlineKeyboardMarkup? keyboard = null;

            if (totalPages > 1)
            {
                var buttons = new List<InlineKeyboardButton>();

                if (page > 0)
                {
                    buttons.Add(InlineKeyboardButton.WithCallbackData(
                        "⬅️",
                        $"help_{msg.From.Id}_{page - 1}"
                    ));
                }

                if (page < totalPages - 1)
                {
                    buttons.Add(InlineKeyboardButton.WithCallbackData(
                        "➡️",
                        $"help_{msg.From.Id}_{page + 1}"
                    ));
                }

                keyboard = new InlineKeyboardMarkup(new[] { buttons });
            }
            await MessageManager.SendAsync(
                botClient,
                msg.Chat.Id,
                text,
                10,
                replyMarkup: keyboard
            );
        }
        public static async Task<(string text, int totalPages)> BuildHelpText(
            ITelegramBotClient botClient,
            Update update,
            CommandSet commandSet,
            int page,
            int pageSize = 5)
        {
            var msg = update.Message ?? update.CallbackQuery?.Message;
            if (msg?.From == null) return ("Нет доступных команд", 1);

            var available = new List<CommandDefinition>();

            foreach (var cmd in commandSet.Commands)
            {
                // --- userId фильтр ---
                if (cmd.AllowedUserIds.Any() &&
                    !cmd.AllowedUserIds.Contains(msg.From.Id))
                    continue;

                // --- статус фильтр ---
                if (cmd.AllowedStatuses.Any())
                {
                    var member = await botClient.GetChatMemberAsync(
                        msg.Chat.Id,
                        msg.From.Id
                    );
                    var status = member switch
                    {
                        ChatMemberOwner => ChatMemberStatus.Creator,
                        ChatMemberAdministrator => ChatMemberStatus.Administrator,
                        ChatMemberMember => ChatMemberStatus.Member,
                        ChatMemberRestricted => ChatMemberStatus.Restricted,
                        ChatMemberLeft => ChatMemberStatus.Left,
                        ChatMemberBanned => ChatMemberStatus.Kicked,
                        _ => throw new Exception("Unknown ChatMember type")
                    };

                    if (!cmd.AllowedStatuses.Contains(status))
                        continue;
                }

                available.Add(cmd);
            }

            if (available.Count == 0)
                return ("Нет доступных команд", 1);

            int totalPages = (int)Math.Ceiling(available.Count / (double)pageSize);
            page = Math.Clamp(page, 0, totalPages - 1);

            var pageCommands = available
                .Skip(page * pageSize)
                .Take(pageSize);

            var lines = new List<string>();

            foreach (var cmd in pageCommands)
            {
                var signature = BuildArgsSignature(cmd.Arguments);

                var desc = string.IsNullOrEmpty(cmd.Description)
                    ? ""
                    : $" — {cmd.Description}";

                lines.Add($"/{cmd.Name}{signature}{desc}");
            }

            string text = string.Join("\n", lines);

            if (totalPages > 1)
                text += $"\n\nСтраница {page + 1}/{totalPages}";

            return (text, totalPages);
        }
        private static string BuildArgsSignature(List<ArgumentDefinition> args)
        {
            if (args == null || args.Count == 0)
                return "";

            var parts = new List<string>();

            foreach (var arg in args)
            {
                string name = arg.Type switch
                {
                    ArgumentType.Int => "int",
                    ArgumentType.Long => "long",
                    ArgumentType.Word => "word",
                    ArgumentType.QuotedString => "\"text\"",
                    ArgumentType.Rest => "text...",
                    _ => "arg"
                };

                if (arg.Optional)
                    name = $"[{name}]";
                else
                    name = $"<{name}>";

                parts.Add(name);
            }

            return " " + string.Join(" ", parts);
        }
        public static class Cooldowns
        {

            public static bool TryUse(string key, TimeSpan cooldown, out TimeSpan remaining)
            {
                var now = DateTime.UtcNow;

                if (Memory.CooldownDict.TryGetValue(key, out var last))
                {
                    var diff = now - last;

                    if (diff < cooldown)
                    {
                        remaining = cooldown - diff;
                        return false;
                    }
                }

                Memory.CooldownDict[key] = now;
                remaining = TimeSpan.Zero;
                return true;
            }
        }
    }
}
