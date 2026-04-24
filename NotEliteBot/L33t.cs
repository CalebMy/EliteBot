using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L33t
{
    public static class L33tMatcher
    {
        private static readonly Dictionary<char, int[]> Groups = new()
        {
            // =========================
            // 0: i / l / 1 / ! / |
            // =========================
            ['i'] = new[] { 0 },
            ['і'] = new[] { 0 },
            ['l'] = new[] { 0, 16},
            ['1'] = new[] { 0 },
            ['!'] = new[] { 0 },
            ['|'] = new[] { 0 },
            ['ì'] = new[] { 0 },
            ['í'] = new[] { 0 },
            ['î'] = new[] { 0 },
            ['ï'] = new[] { 0 },
            ['и'] = new[] { 0 },
            ['n'] = new[] { 0 },
            ['й'] = new[] { 0, 15, 4 },


            // =========================
            // 1: a (lat + kir + leet)
            // =========================
            ['a'] = new[] { 1 },
            ['а'] = new[] { 1 }, // кириллица
            ['@'] = new[] { 1 },
            ['4'] = new[] { 1 },
            ['ά'] = new[] { 1 },
            ['α'] = new[] { 1 },
            ['à'] = new[] { 1 },
            ['á'] = new[] { 1 },
            ['â'] = new[] { 1 },
            ['ä'] = new[] { 1 },
            

            // =========================
            // 2: e / ë / 3
            // =========================
            ['e'] = new[] { 2 },
            ['є'] = new[] { 2 },
            ['э'] = new[] { 2 },
            ['е'] = new[] { 2 }, // кириллица
            ['з'] = new[] { 2, 7 },
            ['3'] = new[] { 2, 7 },
            ['€'] = new[] { 2 },
            ['è'] = new[] { 2 },
            ['é'] = new[] { 2 },
            ['ê'] = new[] { 2 },
            ['ë'] = new[] { 2 },


            // =========================
            // 3: o / 0 / о
            // =========================
            ['o'] = new[] { 3 },
            ['о'] = new[] { 3 }, // кириллица
            ['0'] = new[] { 3 },
            ['θ'] = new[] { 3 },
            ['ø'] = new[] { 3 },
            ['ö'] = new[] { 3 },

            // =========================
            // 4: u / v / w / uu / vv
            // =========================
            ['u'] = new[] { 4 },
            ['v'] = new[] { 4 },
            ['w'] = new[] { 4 },
            ['у'] = new[] { 4 }, // кириллица
            ['ü'] = new[] { 4 },
            ['ù'] = new[] { 4 },
            ['ú'] = new[] { 4 },

            // =========================
            // 5: f / ph / p̶h / ƒ
            // =========================
            ['f'] = new[] { 5 },
            ['ƒ'] = new[] { 5 },
            ['ф'] = new[] { 5 },
            ['p'] = new[] { 5 }, // слабое пересечение через ph->f
            ['φ'] = new[] { 5 },

            // =========================
            // 6: k / c / q / ck / х (иногда подменяют)
            // =========================
            ['k'] = new[] { 6 },
            ['c'] = new[] { 6 },
            ['q'] = new[] { 6 },
            ['к'] = new[] { 6 }, // кириллица
            ['x'] = new[] { 6 },
            ['х'] = new[] { 6 }, // кириллица

            // =========================
            // 7: s / z / $ / 5 / š
            // =========================
            ['s'] = new[] { 7 },
            ['z'] = new[] { 7, 2 },
            ['з'] = new[] { 7, 2 },
            ['$'] = new[] { 7 },
            ['5'] = new[] { 7 },
            ['ś'] = new[] { 7 },
            ['š'] = new[] { 7 },
            ['c'] = new[] { 7 },
            ['с'] = new[] { 7 }, // кириллица
            ['ѕ'] = new[] { 7 }, // старослав.

            // =========================
            // 8: t / + / 7 / †
            // =========================
            ['t'] = new[] { 8 },
            ['+'] = new[] { 8 },
            ['7'] = new[] { 8 },
            ['†'] = new[] { 8 },
            ['т'] = new[] { 8 },

            // =========================
            // 9: b / 8
            // =========================
            ['b'] = new[] { 9 },
            ['8'] = new[] { 9 },
            ['б'] = new[] { 9 },

            // =========================
            // 10: d / cl / 0|
            // =========================
            ['d'] = new[] { 10 },
            ['д'] = new[] { 10 },

            // =========================
            // 11: m / nn / rn / rn≈m
            // =========================
            ['m'] = new[] { 11 },
            ['м'] = new[] { 11 },
            ['n'] = new[] { 11, 15, 12 , 0}, // частично (rn / nn обфускации)

            // =========================
            // 12: h / #-like
            // =========================
            ['h'] = new[] { 12 },
            ['н'] = new[] { 12 },
            ['ң'] = new[] { 12 },
            


            // =========================
            // 13: g / 9
            // =========================
            ['g'] = new[] { 13 },
            ['9'] = new[] { 13 },
            ['г'] = new[] { 13 },

            // =========================
            // 14: r / я (иногда как визуальный трюк)
            // =========================
            ['r'] = new[] { 14, 13 },
            ['я'] = new[] { 14 },
            ['р'] = new[] { 14 },
            ['p'] = new[] { 14 },


            // =========================
            // 15: y / j / у
            // =========================
            ['y'] = new[] { 15 },
            ['j'] = new[] { 15 },
            ['у'] = new[] { 15 },



            ['п'] = new[] { 16, 0 },
            ['л'] = new[] { 16, },
            ['^'] = new[] { 16 },
        };

        // мультисимвольные замены (делаем ДО основной обработки)
        private static readonly (string from, string to)[] MultiRules =
        {// базовые диграфы
            ("vv", "w"),
            ("3️⃣", "3"), 
            ("uu", "u"),
            ("ph", "f"),
            ("ck", "k"),
            ("ck", "k"),
            ("cк", "k"),
            ("ск", "k"),
            ("sh", "s"),
            ("ch", "c"),
            ("th", "t"),
            ("wh", "w"),
            ("/\\", "л"),
            // шумовые пробелы/разделители
            (" ", ""),
            ("́", ""),
            (".", ""),
            ("'", ""),
            ("\"", ""),
            (",", ""),
            ("_", ""),
            ("-", ""),
            ("|", ""),
            ("/", ""),
            ("\\", ""),
            ("*", ""),
            ("\n", "")
    };

        public static bool ContainsL33t(string text, string substring)
        {
            if (string.IsNullOrEmpty(substring)) return true;
            if (string.IsNullOrEmpty(text)) return false;

            text = Normalize(text);
            substring = Normalize(substring);

            var t = ToTokens(text);
            var p = ToTokens(substring);

            if (p.Count > t.Count) return false;

            for (int i = 0; i <= t.Count - p.Count; i++)
            {
                bool ok = true;

                for (int j = 0; j < p.Count; j++)
                {
                    if (!Intersects(t[i + j], p[j]))
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok) return true;
            }

            return false;
        }

        private static string Normalize(string s)
        {
            s = s.ToLowerInvariant();

            foreach (var r in MultiRules)
                s = s.Replace(r.from, r.to);

            // пробелы считаем несущественными
            s = new string(s.Where(c => !char.IsWhiteSpace(c)).ToArray());

            return s;
        }

        private static List<HashSet<int>> ToTokens(string s)
        {
            var result = new List<HashSet<int>>();

            foreach (var c in s)
            {
                var set = new HashSet<int>();

                if (Groups.TryGetValue(c, out var g))
                {
                    foreach (var id in g)
                        set.Add(id);
                }
                else
                {
                    // если символ не описан — он сам себе группа
                    set.Add(c);
                }

                result.Add(set);
            }

            return result;
        }

        private static bool Intersects(HashSet<int> a, HashSet<int> b)
        {
            if (a.Count > b.Count)
            {
                foreach (var x in b)
                    if (a.Contains(x)) return true;
            }
            else
            {
                foreach (var x in a)
                    if (b.Contains(x)) return true;
            }
            return false;
        }
    }
}
