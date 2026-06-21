using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NotEliteBot.Commander;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.Xml.Linq;

namespace NotEliteBot
{
    partial class Group
    {
        private static CommandSet GroupCommonCommandSet = new CommandSet();
        public static void RegisterGroupCommonCommands()
        {
            foreach (var cmd in GroupCommandsCommon)
            {
                GroupCommonCommandSet.Commands.Add(cmd);
            }
        }
        const double BASE_CHANCE = 0.10;
        const double MIN_CHANCE = 0.00;
        const double MAX_CHANCE = 0.20;
        private static readonly List<CommandDefinition> GroupCommandsCommon = new List<CommandDefinition>()
        {
            // Гора Элимп
            new CommandDefinition
            {
                Name = "elimp",
                Description = "покорите гору Элимп и защитите честь своего Чата!",
                Arguments = new() { new ArgumentDefinition { Type = ArgumentType.Rest, Optional = true } },
                Execute = async ctx =>
                {
                    MessageManager.AddMessage(ctx.Update.Message.Chat.Id, ctx.Update.Message.MessageId, 3);
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

                        // отправка в текущий чат
                        await MessageManager.SendAsync(ctx.Bot, currentChatId, text, -1);

                        // отправка в остальные
                        foreach (var chatId in NotifyChats)
                        {
                            if (chatId == currentChatId)
                                continue;

                            try
                            {
                                  await MessageManager.SendAsync(ctx.Bot, chatId, text, -1);
                            }
                            catch
                            {
                                // чат может быть недоступен, бот кикнут и т.д.
                            }
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
                    // Краткое сообщение о том, как справляется пользователь
                    string perfomanceMsg = secondsSinceLast <= 15 ? "(+%)" :
                        "(=%)";
                    // обновляем время попытки
                    UserPrivateSession.LastElimpTry = now;

                    // -----------------------------
                    // ШТРАФ
                    // -----------------------------

                    if (secondsSinceLast < 10)
                    {
                        // чем ближе к 0 сек, тем сильнее штраф
                        double penaltyFactor = (10 - secondsSinceLast) / 10.0;

                        // максимум ~ -2%
                        chance -= 0.02 * penaltyFactor;
                    }

                    // -----------------------------
                    // БОНУС
                    // -----------------------------

                    else if (secondsSinceLast <= 15)
                    {
                        // окно бонуса:
                        // 10с = максимальный бонус
                        // 15с = бонус исчезает

                        double bonusFactor = (15 - secondsSinceLast) / 5.0;

                        // максимум ~ +2%
                        chance += 0.02 * bonusFactor;
                    }

                    // -----------------------------
                    // ВОССТАНОВЛЕНИЕ К БАЗЕ
                    // -----------------------------

                    else
                    {
                        // плавный возврат к 10%

                        if (chance > BASE_CHANCE)
                            chance -= 0.005;
                        else if (chance < BASE_CHANCE)
                            chance += 0.005;
                    }

                    chance = Math.Clamp(
                        chance,
                        MIN_CHANCE,
                        MAX_CHANCE
                    );

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
                    else if (rnd.NextDouble() < chance) // динамический шанс покорить гору
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
                Arguments = new()
                {
                },

                Execute = async ctx =>
                {
                    var allSessions = Memory.Sessions.Values;
                    if (!allSessions.Any(s => s.ConqestedElimp > 0))
{
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            "Пока что никто не покорял Элимп",
                            3,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                        return;
                    }
                    var people = allSessions
                        .Where(s => s.ConqestedElimp > 0 && s.SessionType == SessionType.Private)
                        .OrderByDescending(s => s.ConqestedElimp)
                        .ToList();

                    var chats = allSessions
                        .Where(s => s.ConqestedElimp > 0 && s.SessionType == SessionType.Group)
                        .OrderByDescending(s => s.ConqestedElimp)
                        .ToList();

                    var topPeople = people.Take(10).ToList();
                    var topChats = chats.Take(10).ToList();

                    var sb = new StringBuilder();

                    // --- ТОП ЛЮДЕЙ ---
                    sb.AppendLine("Топ 10 лидеров:");

                    for (int i = 0; i < topPeople.Count; i++)
                    {
                        try
                        {
                            var s = topPeople[i];
                            var user = await ctx.Bot.GetChatAsync(s.Id);
                            string name = $"{user.FirstName} {user.LastName}";
                            sb.AppendLine($"{i + 1}. {name} — {s.ConqestedElimp} {GetConqWord(s.ConqestedElimp)}");
                        }
                        catch
                        {

                        }
                    }

                    // --- ПРОВЕРКА ТЕКУЩЕГО ЮЗЕРА ---
                    var currentUser = allSessions.FirstOrDefault(s =>
                        s.Id == ctx.Update.Message.From.Id &&
                        s.SessionType == SessionType.Private);

                    if (currentUser != null && !topPeople.Any(s => s.Id == currentUser.Id))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"Вы покорили Элимп {currentUser.ConqestedElimp} раз(а)");
                    }

                    sb.AppendLine();

                    // --- ТОП ЧАТОВ ---
                    sb.AppendLine("Статистика Чатов:");

                    for (int i = 0; i < topChats.Count; i++)
                    {

                        try
                        {
                            var s = topChats[i];
                            var chat = await ctx.Bot.GetChatAsync(s.Id);

                            string name = chat.Title; // можно заменить на Title если хранишь
                            sb.AppendLine($"{i + 1}. {name} — {s.ConqestedElimp} {GetConqWord(s.ConqestedElimp)}");
                        }
                        catch
                        {

                        }
                    }

                    // --- ПРОВЕРКА ТЕКУЩЕГО ЧАТА ---
                    var currentChat = allSessions.FirstOrDefault(s =>
                        s.ChatId == ctx.Update.Message.Chat.Id &&
                        s.SessionType == SessionType.Group);

                    if (currentChat != null && !topChats.Any(s => s.ChatId == currentChat.ChatId))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"Этот чат покорил Элимп {currentChat.ConqestedElimp} раз(а)");
                    }
                    MessageManager.Tick(ctx.Bot, ctx.Update, 5);
                    await MessageManager.SendAsync(
                        ctx.Bot,
                        ctx.Update.Message.Chat.Id,
                        sb.ToString(),
                        10,
                        replyToMessageId: ctx.Update.Message.MessageId
                    );
                },
            },
            // Информация о том, как играть в покорение Элимпа
            new CommandDefinition
            {
                Name = "elimp_help",
                Description = "узнайте как покорить Элимп",
                Arguments = new()
                {
                },
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
            },
            // Друлетка
            new CommandDefinition
            {
                Name = "droulette",
                Description = "сделайте что-нибудь с мамой Дилана... Или Нижона? Или Дилана?",
                Arguments = new()
                {
                },

                Execute = async ctx =>
                {
                    string key = $"droulette_{ctx.Update.Message.Chat.Id}_{ctx.Update.Message.From.Id}"; 
                    // можно сделать глобально или по юзеру, см. ниже

                    if (!Commander.Cooldowns.TryUse(key, TimeSpan.FromMinutes(5), out var remaining))
                    {
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            $"Рано. Подожди {remaining.Minutes}м. {remaining.Seconds}с.",
                            3,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                        return;
                    }

                    List<string> Phrases = new()
                    {
                        "Мать {name} хороша ;)",
                        "Мамаша {name} жирная. Она умрёт и т.д.",
                        "У {name} нет мамы, он родился от говна и помидора.",
                        "Мама {name} шлюхенция, продаёт свои услуги в 5 метрах от дома.",
                        "Мама {name} настолько жирная, что когда она прыгает с парашютом, нейросети не могут придумать нормальный каламбур.",
                        "Ебали маму {name} всем селом!",
                        "Почему деревню поразил сифилис? Ах да, мама {name}...",
                        "Папа {name} настолько ту... Стоп, это же его мама!",
                        "Твоя мать мертва.",
                        "Мать {name} мертва.",
                        "Мать {name} занялась благотворительностью, поэтому трахнула бомжа и понесла вышеупомянутую персону.",
                        "Пока папа на фронте, маму {name} ебёт чурка.",
                        "Что жирнее, Влад или мама {name}?",
                        "Кто-нибудь видел свиноматку {name}?",
                        "Под задницей мамы {name} обнаружили... Читать далее...",
                        "Вставьте сюда креативное оскорбление мамы {name}.",
                        "Мама {name} настолько тупая, что был создан этот бот для напоминания.",
                        "Когда мама {name} гладит трусы, все мужчины в радиусе километра получают спидорак левой почки.",
                        "Интересный факт: шутки про мамочек изобрели сразу после того, как мама {name} опоросятилась.",
                        "Нельзя трахать маму {name}! У неё онкогенный ВПЧ.",
                        "Признаюсь честно – я боюсь маму {name}. Я слышал, что она раздавила 20 человек, когда споткнулась в пустом открытом поле.",
                        "Венерические заболевания изобрела мама {name}.",
                        "Чурки знают 1000 и 1 способ трахать маму {name}.",
                        "Вчера в автошколе проходили новый дорожный знак «Осторожно – мама {name}».",
                        "🎵 {name} ма-а-а-ааать,\r\nЕё любим еба-а-а-ааать!\r\nИ ночью и днём,\r\nРаком и в рот,\r\nВсе дружно ебёёёём! 🎵",
                        "Ребята, переходим в MAX, там хотя бы нет мамы {name}.",
                        "– Давай будем убивать.\r\n– Кого убивать-то?\r\n– Маму. {name}.",
                        "Ребята, ну что вы ведётесь на фейки? Мама {name} святая женщина! Вчера гостевал у неё, она меня чаем угостила (пошляки рот закройте). Мне кажется, вам нужно всего лишь с ней познакомиться и вы поймёте, что она годится в матери всем нам!\r\n\r\n(Текст не редактировать, информацию в скобках - удалить. Оплата по ранее указанными реквизитам в течение 15 минут после публикации)",
                    };
                    List<string> RareNames = new()
                    {
                        "Алмаза",
                        "Грейдая",
                        "Артика",
                        "Максона",
                        "Сглыпы",
                        "Дунярляна",
                        "Максома",
                        "Флафина",
                        "Саида",
                        "Майнжме",
                        "Алмазана",
                        "Никитоса",
                        "Админа",
                        "Аметиста",
                        "Сраметиста",
                        "Алехандро",
                        "Слайка",
                        "Прометея",
                        "Влада",
                        "Эггмана",
                        "Саши из мт",
                        "Динара",
                        "Ниона",
                        "Нийона",
                        "Nijon'а"
                    };
                    string Dinar = "Нижона";
                    string Dylan = "Дилана";
                    string theName = "";

                    Random rnd = new ();
                    if (rnd.NextDouble() < 0.5) theName = Dinar;
                    else theName = Dylan;
                    if (rnd.NextDouble() < 0.1) theName = RareNames[rnd.Next(RareNames.Count)];

                    string response = "";
                    var phrase = Phrases[rnd.Next(Phrases.Count)];
                    var msg = ctx.Update.Message;
                    int replyId = msg.ReplyToMessage?.MessageId ?? msg.MessageId;
                    response = phrase.Replace("{name}", theName);

                    await MessageManager.SendAsync(
                        ctx.Bot,
                        msg.Chat.Id,
                        response,
                        -1,
                        replyToMessageId: replyId
                    );
                }
            },
            // SVO
            new CommandDefinition
            {
                Name = "svo",
                Description = "узнайте, сколько осталось до возвращения Максона из армии",
                Arguments = new()
                {
                },

                Execute = async ctx =>
                {
                    string timeTilMakson = "";
                    DateTime now = DateTime.Now;
                    DateTime target = new DateTime(2026, 11, 27);

                    if (now >= target)
                        timeTilMakson = "МАКСОН ВЕРНУЛСЯ!";
                    else
                    {
                        int months = 0;
                        DateTime temp = now;

                        // считаем месяцы отдельно (они не фиксированные)
                        while (temp.AddMonths(1) <= target)
                        {
                            temp = temp.AddMonths(1);
                            months++;
                        }

                        // остаток через TimeSpan
                        TimeSpan diff = target - temp;

                        timeTilMakson = $"Максон вернётся из армии через: {months} мес. {diff.Days} дн. {diff.Hours} ч. {diff.Minutes} м. {diff.Seconds} с.";
                    }

                    await MessageManager.SendAsync(
                        ctx.Bot,
                        ctx.Update.Message.Chat.Id,
                        timeTilMakson,
                        10,
                        replyToMessageId: ctx.Update.Message.MessageId
                    );
                }
            },
            // !gay
            new CommandDefinition
            {
                Name = "gay",
                Description = "сформировать гея",
                Arguments = { },
                Execute = async ctx =>
                {
                    Random rnd = new();
                    string[] responses =
                    {
                        "`Готово`",
                        "`Гей сформирован`",
                        "`Гомофикация завершена`",
                        "`Опидорение завершено`",
                        "`Задача завершена`",
                        "`Запрос отправлен`",
                        "`Ошибка\\: пользователь слишком тестостероновый качок`",
                        "`Ошибка\\: пользователь уже гей`",
                        "`Ничего не изменилось`",
                        "`Ошибка: у пользователя нет мужских гениталий`",
                        "`Голубизация выполнена`",
                        "`Успех: пользователь гей`",
                        "`Успех`",
                        "`Успешно`",
                    };

                    string response = responses[rnd.Next(0, responses.Length)];
                    var msg = ctx.Update.Message;
                    int replyId = msg.ReplyToMessage?.MessageId ?? msg.MessageId;
                    await MessageManager.SendAsync(
                        ctx.Bot,
                        msg.Chat.Id,
                        response,
                        -1,
                        replyToMessageId: replyId,
                        parseMode: ParseMode.MarkdownV2
                    );
                }
            },
            // Выключить сглыпу
            new CommandDefinition
            {
                Name = "blender",
                Description = "засунуть Сглыпу в блендер (на время в минутах)",

                Arguments = new()
                {
                   new ArgumentDefinition {Type = ArgumentType.Int, Optional = true}
                },

                Execute = async ctx =>
                {
                    // Проверяем, указал ли пользователь время, если нет - ставим 60 секунд
                    int time = ctx.Args[0] != null ? (int)ctx.Args[0] : 1;
                    if (time < 1) time = 1;
                    var until = DateTimeOffset.UtcNow
                        .AddMinutes(time).AddSeconds(1)
                        .ToUnixTimeSeconds();
                    await ctx.Bot.RestrictChatMemberAsync(
                        ctx.Update.Message.Chat.Id,
                        IDs.Sglypa,
                        new ChatPermissions
                        {
                            CanSendMessages = false,
                            CanSendPhotos = false,
                            CanSendPolls = false,
                            CanSendOtherMessages = false,
                            CanAddWebPagePreviews = false,
                            CanChangeInfo = false,
                            CanInviteUsers = false,
                            CanPinMessages = false,
                            CanSendAudios = false,
                            CanManageTopics = false,
                            CanSendDocuments = false,
                            CanSendVideoNotes = false,
                            CanSendVideos = false,
                            CanSendVoiceNotes = false
                        },
                        untilDate: DateTimeOffset.FromUnixTimeSeconds(until).UtcDateTime
                    );
                    await MessageManager.SendAsync(
                        ctx.Bot,
                        ctx.Update.Message.Chat.Id,
                        $"Сглыпа в блендере на {time}м. 👌",
                        3,
                        replyToMessageId: ctx.Update.Message.MessageId
                    );
                }
            },
            // Досрочно включить сглыпу
            new CommandDefinition
            {
                Name = "free_sglypa",
                Description = "досрочно освободить Сглыпу из блендера",

                Arguments = new()
                {
                },

                Execute = async ctx =>
                {
                    await ctx.Bot.RestrictChatMemberAsync(
                        ctx.Update.Message.Chat.Id,
                        IDs.Sglypa,
                        new ChatPermissions
                        {
                            CanSendMessages = true,
                            CanSendPhotos = true,
                            CanSendPolls = true,
                            CanSendOtherMessages = true,
                            CanAddWebPagePreviews = true,
                            CanChangeInfo = false,
                            CanInviteUsers = false,
                            CanPinMessages = false,
                            CanSendAudios = true,
                            CanManageTopics = true,
                            CanSendDocuments = true,
                            CanSendVideoNotes = true,
                            CanSendVideos = true,
                            CanSendVoiceNotes = true
                        },
                        untilDate: DateTime.UtcNow.AddSeconds(1) // Ставим минимально возможное время, чтобы ограничения снялись
                    );
                    await MessageManager.SendAsync(
                        ctx.Bot,
                        ctx.Update.Message.Chat.Id,
                        $"Сглыпа освобождён из блендера 👌",
                        3,
                        replyToMessageId: ctx.Update.Message.MessageId
                    );
                }
            },
            // Ивент чата
            new CommandDefinition
            {
                Name = "event",
                Description = "изменить текущий ивент чата (только для админов)",
                AllowedStatuses = { ChatMemberStatus.Administrator, ChatMemberStatus.Creator },

                Arguments = new()
                {
                    new ArgumentDefinition { Type = ArgumentType.Rest, Optional = true }
                },

                Execute = async ctx =>
                {
                    string rest = "";
                    if (!string.IsNullOrEmpty((string)ctx.Args[0]))
                    {
                        rest = (string)ctx.Args[0];
                    }
                    string origChatName = "";
                    long chatId = 0;
                    switch (ctx.Update.Message.Chat.Id)
                    {
                        case -1001850405088: //Вставить сюда ЭЛИТКУ
                            origChatName = "Элитарный Чат";
                            chatId = IDs.ElitkaChat;
                            break;
                        case -1002706252827:
                            origChatName = "Альтернативный Чат";
                            chatId = IDs.AlitkaChat;
                            break;
                        default:
                            return;
                    }
                    // Добавляем в название чата его ивент
                    if (!string.IsNullOrEmpty(rest)) await ctx.Bot.SetChatTitleAsync(chatId, $"{origChatName}: {rest}");
                    else await ctx.Bot.SetChatTitleAsync(chatId, origChatName);

                }
            },
            // Отправить телеграмму в другой чат
            new CommandDefinition
            {
                Name = "telegraph",
                Description = "отправить телеграмму в другой чат (только для админов чата)",
                AllowedStatuses = { ChatMemberStatus.Administrator, ChatMemberStatus.Creator },
                Arguments = new()
                {
                    new ArgumentDefinition { Type = ArgumentType.QuotedString },
                    new ArgumentDefinition { Type = ArgumentType.Rest }
                },
                Execute = async ctx =>
                {
                    // Проверяем кд
                    string key = $"telegraph_{ctx.Update.Message.From.Id}_{ctx.Update.Message.Chat.Id}";
                    TimeSpan remaining;
                    bool isOnCooldown = !Commander.Cooldowns.TryUse(key, TimeSpan.FromHours(6), out remaining);
                    if (isOnCooldown)
                    {
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            $"Пока что нельзя отправить телеграмму. Подождите {remaining.Hours}ч. {remaining.Minutes}м. {remaining.Seconds}с.",
                            3,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                        return;
                    }
                    // Проверяем адрес чата и не пытается ли юзер отправить телеграмму в свой же чат
                    string address = (string)ctx.Args[0];
                    if (!Memory.ChatAddresses.TryGetValue(address, out var targetChatId))
                    {
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            $"Чат с таким адресом не найден",
                            3,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                        Cooldowns.Reset(key);
                        return;
                    }
                    else if (targetChatId == ctx.Update.Message.Chat.Id)
                    {
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            $"Нельзя отправить телеграмму в этот же чат",
                            3,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                        Cooldowns.Reset(key);
                        return;
                    }
                    // Элементы форматирования телеграммы
                    // Полное имя отправителя
                    string SenderName = $"{ctx.Update.Message.From.FirstName} {ctx.Update.Message.From.LastName}".Trim();
                    string Head = $"Телеграмма от {SenderName} из {ctx.Update.Message.Chat.Title}!";
                    string Tail = $"С уважением, {SenderName} от государства {ctx.Update.Message.Chat.Title}.";
                    string Body = (string)ctx.Args[1];

                    // Отправляем
                    try
                    {
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            targetChatId,
                            $"{Head}\n\n{Body}\n\n{Tail}",
                            -1
                        );
                        await MessageManager.SendAsync
                        (
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            $"Телеграмма отправлена в {ctx.Bot.GetChatAsync(targetChatId).Result.Title}",
                            -1
                        );
                        Cooldowns.TryUse(key, TimeSpan.FromMilliseconds(1), out remaining);
                    }
                    catch (Exception ex)
                    {
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            $"Не удалось отправить телеграмму: {ex.Message}",
                            3,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                        Cooldowns.Reset(key);
                    }
                }
            },
            // Вкл/выкл кастомные подписи в канале
            new CommandDefinition
            {
                Name = "switchcs",
                Description = "переключить кастомные подписи в Элитке (только для разработчика бота)",
                AllowedUserIds = { IDs.admin },
                Arguments = { },
                Execute = async ctx =>
                {
                    Memory.AllowCustomSignature = !Memory.AllowCustomSignature;
                    if (Memory.AllowCustomSignature)
                    {
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            $"Включены кастомные подписи в Элитке",
                            3,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                    }
                    else
                    {
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            $"Выключены кастомные подписи в Элитке",
                            3,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                    }
                }
            },
            // Добавить краткий адрес для чата
            new CommandDefinition
            {
                Name = "add_adress",
                Description = "добавить краткий адрес для чата (только для разработчика бота)",
                AllowedUserIds = { IDs.admin },
                Arguments = { new ArgumentDefinition { Type = ArgumentType.QuotedString } },
                Execute = async ctx =>
                {
                    var chatSession = SessionManager.Get(ctx.Session.ChatId, ctx.Session.ChatId, SessionType.Group);
                    string newAddress = (string)ctx.Args[0];
                    if (Memory.ChatAddresses.ContainsKey(newAddress))
                    {
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            $"Этот адрес уже занят",
                            3,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                        return;
                    }
                    // Удаляем старый адрес, если он есть
                    if (!string.IsNullOrEmpty(chatSession.ShortAdress))
                    {
                        Memory.ChatAddresses.Remove(chatSession.ShortAdress);
                    }
                    // Добавляем новый адрес
                    chatSession.ShortAdress = newAddress;
                    Memory.ChatAddresses[newAddress] = ctx.Session.ChatId;
                    // Сообщаем об успешном добавлении
                    await MessageManager.SendAsync(
                        ctx.Bot,
                        ctx.Update.Message.Chat.Id,
                        $"Добавлен адрес для этого чата: {newAddress}",
                        3,
                        replyToMessageId: ctx.Update.Message.MessageId
                    );
                    Memory.SaveAll();
                }
            },
            // БотБан пользователя
            new CommandDefinition
            {
                Name = "botban",
                Description = "отключить весь функционал бота пользователю",
                AllowedUserIds = { IDs.admin },
                Arguments = { },
                Execute = async ctx =>
                {
                    if (ctx.Update.Message.ReplyToMessage == null)
                    {
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            $"Эту команду нужно использовать в ответ на сообщение пользователя, которого вы хотите забанить",
                            3,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                        return;
                    }
                    else if (ctx.Update.Message.ReplyToMessage.From.Id != IDs.admin)
                        {
                        long userIdToBan = ctx.Update.Message.ReplyToMessage.From.Id;
                        var session = SessionManager.Get(userIdToBan, userIdToBan, SessionType.Private);
                        session.BotBan = true;
                        Memory.SaveAll();
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            $"БотБан выдан",
                            3,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                    }
                    else throw new Exception("Нельзя выдать БотБан создателю бота!");
                }

            },
            // Пардон БотБана
                new CommandDefinition
                                {
                    Name = "botpardon",
                    Description = "убрать БотБан у пользователя",
                    AllowedUserIds = { IDs.admin },
                    Arguments = { },
                    Execute = async ctx =>
                    {
                        if (ctx.Update.Message.ReplyToMessage == null)
                        {
                            await MessageManager.SendAsync(
                                ctx.Bot,
                                ctx.Update.Message.Chat.Id,
                                $"Эту команду нужно использовать в ответ на сообщение пользователя, у которого вы хотите убрать БотБан",
                                3,
                                replyToMessageId: ctx.Update.Message.MessageId
                            );
                            return;
                        }
                        else if (ctx.Update.Message.ReplyToMessage.From.Id != IDs.admin)
                        {
                            long userIdToUnban = ctx.Update.Message.ReplyToMessage.From.Id;
                            var session = SessionManager.Get(userIdToUnban, userIdToUnban, SessionType.Private);
                            session.BotBan = false;
                            Memory.SaveAll();
                            await MessageManager.SendAsync(
                                ctx.Bot,
                                ctx.Update.Message.Chat.Id,
                                $"БотБан убран",
                                3,
                                replyToMessageId: ctx.Update.Message.MessageId
                            );
                        }
                }
            },
            // Вэйп
            new CommandDefinition
            {
                Name = "vape",
                Description = "Легендарный /VAPE",
                Arguments = new() { },
                Execute = async ctx =>
                {
                    string key = $"swim_{ctx.Update.Message.From.Id}";
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

                    var user = SessionManager.Get(
                        ctx.Update.Message.From.Id,
                        ctx.Update.Message.From.Id,
                        SessionType.Private
                    );

                    Random rnd = new();
                    bool drowned = rnd.NextDouble() < 0.12;

                    List<string> normalMessages = new()
                    {
                        "|Вы курите /VAPE| |✅|",
                        "|Вы парите| |💨|",
                        "|Вы затягиваетесь| |🌬|",
                        "|Вы курите| |🚬|",
                    };
                    List<string> rareMessages = new()
                    {
                        "|Вы курите /VAPE| |✅|",
                        "|Вы парите| |💨|",
                        "|Вы затягиваетесь| |🌬|",
                        "|Вы курите| |🚬|",
                    };

                    string response;
                    if (drowned)
                    {
                        user.VAPEStreak = 0;
                        user.VAPEDeaths++;
                        response = "|Вы умерли| |💀|";
                    }
                    else
                    {
                        user.VAPEStreak++;
                        user.TotalVAPE++;

                        if (rnd.NextDouble() < 0.25)
                            response = rareMessages[rnd.Next(rareMessages.Count)];
                        else
                            response = normalMessages[rnd.Next(normalMessages.Count)];
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
            // Статистика плавания
            new CommandDefinition
            {
                Name = "vape_stat",
                Description = "Cтатистика /VAPE",
                Arguments = new() { },
                Execute = async ctx =>
                {
                    var allSessions = Memory.Sessions.Values;
                    if (!allSessions.Any(s => s.TotalVAPE > 0))
                    {
                        await MessageManager.SendAsync(
                            ctx.Bot,
                            ctx.Update.Message.Chat.Id,
                            "Пока что никто не вэйпал",
                            10,
                            replyToMessageId: ctx.Update.Message.MessageId
                        );
                        return;
                    }

                    var vapers = allSessions
                        .Where(s => s.TotalVAPE > 0 && s.SessionType == SessionType.Private)
                        .ToList();

                    var topStreak = vapers
                        .OrderByDescending(s => s.VAPEStreak)
                        .Take(10)
                        .ToList();

                    var topTotal = vapers
                        .OrderByDescending(s => s.TotalVAPE)
                        .Take(10)
                        .ToList();

                    var topDeaths = vapers
                        .OrderByDescending(s => s.VAPEDeaths)
                        .Take(10)
                        .ToList();

                    var sb = new StringBuilder();

                    sb.AppendLine("💨 Топ 10 по текущей серии:");
                    for (int i = 0; i < topStreak.Count; i++)
                    {
                        try
                        {
                            var s = topStreak[i];
                            var user = await ctx.Bot.GetChatAsync(s.Id);
                            string name = $"{user.FirstName} {user.LastName}";
                            sb.AppendLine($"{i + 1}. {name} — {s.VAPEStreak} {GetSwimWord(s.VAPEStreak)}");
                        }
                        catch { }
                    }

                    var currentUser = vapers.FirstOrDefault(s =>
                        s.Id == ctx.Update.Message.From.Id);

                    if (currentUser != null && !topStreak.Any(s => s.Id == currentUser.Id))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"Ваша серия: {currentUser.VAPEStreak} {GetSwimWord(currentUser.VAPEStreak)}");
                    }

                    sb.AppendLine();
                    sb.AppendLine("💨 Топ 10 по общему количеству заплывов:");
                    for (int i = 0; i < topTotal.Count; i++)
                    {
                        try
                        {
                            var s = topTotal[i];
                            var user = await ctx.Bot.GetChatAsync(s.Id);
                            string name = $"{user.FirstName} {user.LastName}";
                            sb.AppendLine($"{i + 1}. {name} — {s.TotalVAPE} {GetSwimWord(s.TotalVAPE)}");
                        }
                        catch { }
                    }

                    if (currentUser != null && !topTotal.Any(s => s.Id == currentUser.Id))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"Всего затяжек: {currentUser.TotalVAPE} {GetSwimWord(currentUser.TotalVAPE)}");
                    }

                    sb.AppendLine();
                    sb.AppendLine("💀 Топ 10 по смертям:");
                    for (int i = 0; i < topDeaths.Count; i++)
                    {
                        try
                        {
                            var s = topDeaths[i];
                            var user = await ctx.Bot.GetChatAsync(s.Id);
                            string name = $"{user.FirstName} {user.LastName}";
                            sb.AppendLine($"{i + 1}. {name} — {s.VAPEDeaths} {GetDeathWord(s.VAPEDeaths)}");
                        }
                        catch { }
                    }

                    if (currentUser != null && !topDeaths.Any(s => s.Id == currentUser.Id))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"Ваши смерти: {currentUser.VAPEDeaths} {GetDeathWord(currentUser.VAPEDeaths)}");
                    }

                    await MessageManager.SendAsync(
                        ctx.Bot,
                        ctx.Update.Message.Chat.Id,
                        sb.ToString(),
                        15,
                        replyToMessageId: ctx.Update.Message.MessageId
                    );
                }
            }

        };
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
        public static string GetSwimWord(int n)
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
        public static string GetDeathWord(int n)
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
