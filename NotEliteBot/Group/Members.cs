using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using L33t;

namespace NotEliteBot
{
    class Members
    {
        public class Sample : IMemberSpecificHandler
        {
            public long UserId => 123456789; // ID пользователя, для которого предназначен этот обработчик

            public async Task Handle(ITelegramBotClient botClient, Update update, Session session, string messageText, CancellationToken ct)
            {
                // Логика обработки сообщений от конкретного пользователя
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Привет! Это обработчик для конкретного пользователя.",
                    replyToMessageId: update.Message.MessageId);
            }
        }
        public class Bedulev : IMemberSpecificHandler
        {
            public long UserId => 5577221205; // ID пользователя, для которого предназначен этот обработчик

            public async Task Handle(ITelegramBotClient botClient, Update update, Session session, string messageText, CancellationToken ct)
            {
                if (L33tMatcher.ContainsL33t(messageText, "шаверм"))
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Шаурма*",
                    replyToMessageId: update.Message.MessageId);
            }
        }
        public class Dinar : IMemberSpecificHandler
        {
            public long UserId => 5099820426; // ID пользователя, для которого предназначен этот обработчик
            private readonly string[] keyWords = 
            {
                "я забыл",
                "не помню",
                "альцгеймер",
                "деменция",
                "не вспомню",
                "амнезия"
            };
            private readonly string[] responses =
            {
                "Кофе помогает от альцгеймера.\nКофе помогает от альцгеймера.\nКофе помогает от альцгеймера.\nКофе помогает от альцгеймера.",
                "Ты забыл про альцгеймер",
                "Можно ли вести реестер элитарных проектов с деменцией?",
                "Динарция\nДинарция\nДинарция\nДинарция\nДинарция...",
                "Я тоже чёт не припоминаю.",
                "Можно ли пройти майнкрафт с деменцией?",
                "Можно ли играть в криейт с деменцией?",
                "Кто такой альцгеймер? Я ему наваляю!"
            };
            private Random rnd = new Random();
            public async Task Handle(ITelegramBotClient botClient, Update update, Session session, string messageText, CancellationToken ct)
            {
                // Логика обработки сообщений от конкретного пользователя
                bool found = false;
                foreach (var keyWord in keyWords)
                {
                    if (messageText.ToLower().Contains(keyWord))
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    responses[rnd.Next(0, responses.Length)],
                    replyToMessageId: update.Message.MessageId);
                }
            }
        }
        public class Dylan : IMemberSpecificHandler
        {
            public long UserId => 6871105479; // ID пользователя, для которого предназначен этот обработчик

            public async Task Handle(ITelegramBotClient botClient, Update update, Session session, string messageText, CancellationToken ct)
            {

                if (L33tMatcher.ContainsL33t(messageText, "найди работу") ||
                    L33tMatcher.ContainsL33t(messageText, "find job") ||
                    L33tMatcher.ContainsL33t(messageText, "работу найди") ||
                    L33tMatcher.ContainsL33t(messageText, "файнд джоб") ||
                    L33tMatcher.ContainsL33t(messageText, "naydi rabotu") ||
                    L33tMatcher.ContainsL33t(messageText, "rabotu naydi") ||
                    L33tMatcher.ContainsL33t(messageText, "rabotu naidi") ||
                    L33tMatcher.ContainsL33t(messageText, "naidi rabotu"))
                {
                    await botClient.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        "Умри Дулян",
                        replyToMessageId: update.Message.MessageId);
                    Debug.Log("Умри Дулян", Debug.LogLevel.Info);
                }
            }
        }
        public class Almazman : IMemberSpecificHandler
        {
            public long UserId => IDs.admin; // ID пользователя, для которого предназначен этот обработчик

            public async Task Handle(ITelegramBotClient botClient, Update update, Session session, string messageText, CancellationToken ct)
            {
                if (L33tMatcher.ContainsL33t(messageText, "зэкерис"))
                {
                    await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Подписывайтесь на @zekeriset_for ;)",
                    replyToMessageId: update.Message.MessageId);
                }    
                
            }
        }
        public class Artik : IMemberSpecificHandler
        {
            public long UserId => 6864749068; // ID пользователя, для которого предназначен этот обработчик
            private string[] keyWords =
            {
                "смерт",
                "умер",
                "умир",
                "сдох",
                "суеци",
                "суици",
                "умру",
                "кладбищ",
                "труп"
            };
            private string[] responses =
            {
                "Жизнь — это как полёт с крыши. Можно бояться, а можно веселиться!",
                "Смерть — это не выход, ведь, как ты будешь ходить мёртвым?",
                "Даже робот говорит тебе, что ты нам нужен! /s",
                "Аааааа, арти(не)к(уколд)!",
                "Забань их всех будь царём ты крут ты пиздат все плебеи немощные забери их душы не плач будь гигачадом ыабббыббфзыю",
                "Ошибка: пользователь слишком жив для выхода.",
                "Смерть — это как сон, только без будильника. Подозрительно.",
                "Ты нужен хотя бы для статистики. Не ломай график.",
                "Я бы пожал плечами, но у меня нет плеч. А у тебя есть, так что ЭТО Я ТУТ СТРАДАЮ, А ТЫ НЮНИ РАЗВЁЛ!",
                "Я тут подумал... если ты умрёшь, элитовцам станет скучно. С кого ржать тогда?",
                "Смерть — это когда уже совсем ничего нельзя исправить. Пока ты жив — можно хотя бы обосраться по-новому.",
                "смерть.exe не отвечает, попробуйте жить",
                "Ты не можешь умереть, ты же не живой!",
                "Я не понял, так ты дед инсайд, или dead инасайд?",
                "А — абсолютно\nР — решил\nТ — терпеть\nИ — издевательства\nК — каrтавого"
            };
            private Random rnd = new Random();

            public async Task Handle(ITelegramBotClient botClient, Update update, Session session, string messageText, CancellationToken ct)
            {
                bool found = false;
                foreach (var keyWord in keyWords)
                {
                    if (L33tMatcher.ContainsL33t(messageText, keyWord))
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    responses[rnd.Next(0,responses.Length)],
                    replyToMessageId: update.Message.MessageId);
                }
            }
        }
    }
}
