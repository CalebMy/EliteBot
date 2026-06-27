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
                GroupCommonCommandSet.Commands.Add(cmd);
            foreach (var cmd in GetElimpCommands())
                GroupCommonCommandSet.Commands.Add(cmd);
            foreach (var cmd in GetVapeCommands())
                GroupCommonCommandSet.Commands.Add(cmd);
        }
        private static readonly List<CommandDefinition> GroupCommandsCommon = new List<CommandDefinition>()
        {
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
        };
    }

}