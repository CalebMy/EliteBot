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
    class Smoke
    {
        public interface ISmokeAction
        {
            string Id { get; }  // Айдишник в CallbackData
            string ButtonText { get; }  // Имя для отображения
            int Cost { get; }   // Стоимость действия
            double SuccsessBaseChance { get; }   // Базовый шанс успешного действия
            double TestosteroneMaxBonus { get; } // Бонус к успеху от тестостерона (может быть отрицательным)
            int MinimalTestosterone { get; } // Минимальный тестостерон для успеха

            Task Execute(SmokeContext Context) // Единая точка входа для проверки условий и выполнения действия
            {
                Random rnd = new();
                // Проверяем условия и выбираем результат
                if (Context == null) throw new Exception("Контекст пуст!");
                if (Context.Session.Testosterone < MinimalTestosterone)
                {
                    return FailureAction(Context);
                }
                // Считаем итоговый шанс успеха
                double finalChance = SuccsessBaseChance + (Context.Session.Testosterone / 1000.0) * TestosteroneMaxBonus;
                if (rnd.NextDouble() <= finalChance)
                {
                    return SuccsessAction(Context);
                }
                else
                {
                    return FailureAction(Context);
                }
            }
            Task SuccsessAction(SmokeContext Context); // Действия при успехе
            Task FailureAction(SmokeContext Context); // Действия при неудаче
        }

        public class SmokeContext
        {
            public ITelegramBotClient Bot { get; set; }
            public Update Update { get; set; }
            public Session Session { get; set; }

        }
    } 
}
