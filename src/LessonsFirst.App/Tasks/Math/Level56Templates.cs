using System.Globalization;
using static LessonsFirst.App.Tasks.RussianText;

namespace LessonsFirst.App.Tasks.MathTasks;

/// <summary>Уровень «5–6 класс — сложный»: C1–C12.</summary>
internal static class Level56Templates
{
    public static readonly IMathTemplate[] All =
    {
        new C1(), new C2(), new C3(), new C4(), new C5(),
        new C6(), new C7(), new C8(), new C9(), new C10(),
        new C11(), new C12(),
    };

    private const string Subject = "Математика";

    private sealed class C1 : MathTemplateBase
    {
        public override string Id => "C1";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 12, 99);
            var b = Int(rng, 12, 25);
            var made = Make(a, b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 12; a <= 99; a++)
            {
                for (var b = 12; b <= 25; b++)
                {
                    yield return Make(a, b);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "C1",
                Prompt = $"Вычисли: {a} × {b}",
                Hint = "Умножай в столбик или разложи на разряды",
                Form = AnswerForm.Integer,
                ExpectedInteger = a * b,
            }, $"{a}×{b}");
    }

    private sealed class C2 : MathTemplateBase
    {
        public override string Id => "C2";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var quotient = Int(rng, 12, 99);
            var divisor = Int(rng, 12, 25);
            var made = Make(quotient * divisor, divisor);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var quotient = 12; quotient <= 99; quotient++)
            {
                for (var divisor = 12; divisor <= 25; divisor++)
                {
                    yield return Make(quotient * divisor, divisor);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int dividend, int divisor) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "C2",
                Prompt = $"Вычисли: {dividend} ÷ {divisor}",
                Hint = "Дели уголком; остатка нет",
                Form = AnswerForm.Integer,
                ExpectedInteger = dividend / divisor,
            }, $"{dividend}:{divisor}");
    }

    private sealed class C3 : MathTemplateBase
    {
        public override string Id => "C3";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 1, 200);
            var b = Int(rng, 2, 20);
            var c = Int(rng, 2, 12);
            var made = Make(a, b, c);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 1; a <= 200; a++)
            {
                for (var b = 2; b <= 20; b++)
                {
                    for (var c = 2; c <= 12; c++)
                    {
                        yield return Make(a, b, c);
                    }
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b, int c) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "C3",
                Prompt = $"Вычисли: {a} + {b} × {c}",
                Hint = "Сначала умножение, потом сложение",
                Form = AnswerForm.Integer,
                ExpectedInteger = a + b * c,
            }, $"{a}+{b}×{c}");
    }

    private sealed class C4 : MathTemplateBase
    {
        public override string Id => "C4";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 2, 50);
            var b = Int(rng, 2, 50);
            var c = Int(rng, 2, 9);
            var made = Make(a, b, c);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 2; a <= 50; a++)
            {
                for (var b = 2; b <= 50; b++)
                {
                    for (var c = 2; c <= 9; c++)
                    {
                        yield return Make(a, b, c);
                    }
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b, int c) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "C4",
                Prompt = $"Вычисли: ({a} + {b}) × {c}",
                Hint = "Сначала действие в скобках",
                Form = AnswerForm.Integer,
                ExpectedInteger = (a + b) * c,
            }, $"{a}+{b}×{c}");
    }

    private sealed class C5 : MathTemplateBase
    {
        public override string Id => "C5";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 2, 12);
            var x = Int(rng, 1, 50);
            var b = Int(rng, 1, 100);
            var made = Make(a, x, b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 2; a <= 12; a++)
            {
                for (var x = 1; x <= 50; x++)
                {
                    for (var b = 1; b <= 100; b++)
                    {
                        yield return Make(a, x, b);
                    }
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int x, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "C5",
                Prompt = $"Реши уравнение: {a} × x + {b} = {a * x + b}",
                Hint = "Сначала вычти свободное слагаемое, потом раздели на коэффициент",
                Form = AnswerForm.Integer,
                ExpectedInteger = x,
            }, $"{a}x+{b}={a * x + b}");
    }

    private sealed class C6 : MathTemplateBase
    {
        public override string Id => "C6";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            // Ограниченные попытки подобрать знаменатели с НОК ≤ 60; перебор гарантирует завершение.
            for (var attempt = 0; attempt < 50; attempt++)
            {
                var den1 = Int(rng, 2, 12);
                var den2 = Int(rng, 2, 12);
                if (Lcm(den1, den2) > 60)
                {
                    continue;
                }

                var num1 = Int(rng, 1, den1 - 1);
                var num2 = Int(rng, 1, den2 - 1);
                var subtraction = rng.Next(2) == 1;
                if (subtraction && num1 * den2 <= num2 * den1)
                {
                    continue; // при вычитании первая дробь строго больше
                }

                var made = Make(num1, den1, num2, den2, subtraction);
                canonicalKey = made.Key;
                return made.Task;
            }

            // Гарантированный корректный вариант.
            var fallback = Make(1, 2, 1, 3, subtraction: false);
            canonicalKey = fallback.Key;
            return fallback.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var den1 = 2; den1 <= 12; den1++)
            {
                for (var den2 = 2; den2 <= 12; den2++)
                {
                    if (Lcm(den1, den2) > 60)
                    {
                        continue;
                    }

                    for (var num1 = 1; num1 < den1; num1++)
                    {
                        for (var num2 = 1; num2 < den2; num2++)
                        {
                            yield return Make(num1, den1, num2, den2, subtraction: false);
                            if (num1 * den2 > num2 * den1)
                            {
                                yield return Make(num1, den1, num2, den2, subtraction: true);
                            }
                        }
                    }
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int num1, int den1, int num2, int den2, bool subtraction)
        {
            var numerator = num1 * den2 + (subtraction ? -num2 * den1 : num2 * den1);
            var denominator = den1 * den2;
            var reduced = AnswerValidator.ReduceFraction(numerator, denominator);
            var op = subtraction ? "−" : "+";
            return (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "C6",
                Prompt = $"Вычисли: {num1}/{den1} {op} {num2}/{den2}",
                Hint = "Приведи дроби к общему знаменателю",
                Form = AnswerForm.Fraction,
                ExpectedFraction = reduced,
            }, $"{(subtraction ? "sub" : "add")}:{num1}/{den1}:{num2}/{den2}");
        }
    }

    private sealed class C7 : MathTemplateBase
    {
        public override string Id => "C7";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var n = Int(rng, 2, 12);
            var m = Int(rng, 1, n - 1);
            var k = Int(rng, 2, 30);
            var made = Make(m, n, k);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var n = 2; n <= 12; n++)
            {
                for (var m = 1; m < n; m++)
                {
                    for (var k = 2; k <= 30; k++)
                    {
                        yield return Make(m, n, k);
                    }
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int m, int n, int k) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "C7",
                Prompt = $"Найди {m}/{n} от числа {n * k}",
                Hint = "Раздели число на знаменатель и умножь на числитель",
                Form = AnswerForm.Integer,
                ExpectedInteger = m * k,
            }, $"{m}/{n}×{n * k}");
    }

    private sealed class C8 : MathTemplateBase
    {
        private static readonly int[] Percents = { 5, 10, 20, 25, 50, 75 };

        public override string Id => "C8";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var percent = Percents[rng.Next(Percents.Length)];
            var step = 100 / Gcd(percent, 100);
            var multiplier = Int(rng, (20 + step - 1) / step, 1000 / step);
            var made = Make(percent, multiplier * step);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            foreach (var percent in Percents)
            {
                var step = 100 / Gcd(percent, 100);
                for (var multiplier = (20 + step - 1) / step; multiplier <= 1000 / step; multiplier++)
                {
                    yield return Make(percent, multiplier * step);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int percent, int number) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "C8",
                Prompt = $"Найди {percent}% от числа {number}",
                Hint = "Процент — сотая часть числа",
                Form = AnswerForm.Integer,
                ExpectedInteger = number * percent / 100,
            }, $"{percent}%:{number}");

        private static int Gcd(int a, int b)
        {
            while (b != 0)
            {
                (a, b) = (b, a % b);
            }

            return a;
        }
    }

    private sealed class C9 : MathTemplateBase
    {
        /// <summary>Пространство вариантов велико (~10⁷); перебор не поддерживается — при достижении
        /// лимита попыток история сбрасывается (статистически недостижимо за сессию).</summary>
        public override string Id => "C9";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var subtraction = rng.Next(2) == 1;
            int firstCents;
            int secondCents;
            if (subtraction)
            {
                firstCents = NonRound(rng, 10, 9999);
                secondCents = NonRound(rng, 10, firstCents - 1);
            }
            else
            {
                firstCents = NonRound(rng, 10, 9999);
                secondCents = NonRound(rng, 10, 10000 - firstCents);
            }

            var made = Make(firstCents, secondCents, subtraction);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key)
        {
            task = null!;
            key = "";
            return false;
        }

        /// <summary>Целое число сотых с ненулевой дробной частью (не круглое).</summary>
        private static int NonRound(Random rng, int minCents, int maxCents)
        {
            var cents = Int(rng, minCents, maxCents);
            return cents % 100 == 0 ? cents + 1 : cents; // +1 сохраняет диапазон: край 9999/10000 не круглой задачи
        }

        private static (SchoolTask Task, string Key) Make(int firstCents, int secondCents, bool subtraction)
        {
            var first = ToDecimal(firstCents);
            var second = ToDecimal(secondCents);
            var resultCents = subtraction ? firstCents - secondCents : firstCents + secondCents;
            var op = subtraction ? "−" : "+";
            return (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "C9",
                Prompt = $"Вычисли: {ToText(first)} {op} {ToText(second)}",
                Hint = "Запиши в столбик, разряд под разрядом; запиши ответ через запятую",
                Form = AnswerForm.Decimal,
                ExpectedDecimalValue = ToDecimal(resultCents),
            }, $"{(subtraction ? "sub" : "add")}:{firstCents}:{secondCents}");
        }

        private static decimal ToDecimal(int cents) => cents / 100m;

        /// <summary>Русский формат: 12,5 или 3,75 (без лишних нулей).</summary>
        private static string ToText(decimal value) =>
            value.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');
    }

    private sealed class C10 : MathTemplateBase
    {
        public override string Id => "C10";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var cents = Int(rng, 10, 999);
            if (cents % 100 == 0)
            {
                cents++; // ненулевая дробная часть
            }

            var whole = Int(rng, 2, 9);
            var made = Make(cents, whole);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var cents = 10; cents <= 999; cents++)
            {
                if (cents % 100 == 0)
                {
                    continue;
                }

                for (var whole = 2; whole <= 9; whole++)
                {
                    yield return Make(cents, whole);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int cents, int whole)
        {
            var value = cents / 100m;
            return (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "C10",
                Prompt = $"Вычисли: {value.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',')} × {whole}",
                Hint = "Умножай, не обращая внимания на запятую, затем верни её на место",
                Form = AnswerForm.Decimal,
                ExpectedDecimalValue = cents * whole / 100m,
            }, $"{cents}×{whole}");
        }
    }

    private sealed class C11 : MathTemplateBase
    {
        public override string Id => "C11";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var packs = Int(rng, 2, 9);
            var perPack = Int(rng, 3, 20);
            var given = Int(rng, 1, packs * perPack - 1);
            var made = Make(packs, perPack, given, rng);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(rng), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable(Random rng)
        {
            for (var packs = 2; packs <= 9; packs++)
            {
                for (var perPack = 3; perPack <= 20; perPack++)
                {
                    for (var given = 1; given < packs * perPack; given++)
                    {
                        yield return Make(packs, perPack, given, rng);
                    }
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int packs, int perPack, int given, Random rng)
        {
            var item = rng.Next(2) == 0 ? Pencil : Sweet;
            return (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "C11",
                Prompt = $"Купили {Counted(packs, Set)} по {Counted(perPack, item)}. "
                    + $"{Counted(given, item)} отдали. Сколько {item.Many} осталось?",
                Hint = "Сначала найди общее количество, потом вычти отданное",
                Form = AnswerForm.Integer,
                ExpectedInteger = packs * perPack - given,
            }, $"{packs}×{perPack}-{given}");
        }
    }

    private sealed class C12 : MathTemplateBase
    {
        public override string Id => "C12";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var subtraction = rng.Next(2) == 1;
            var a = Int(rng, -50, 50);
            var b = Int(rng, -50, 50);
            if (a >= 0 && b >= 0)
            {
                a = -Int(rng, 1, 50); // хотя бы один операнд отрицательный
            }

            var made = Make(a, b, subtraction);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = -50; a <= 50; a++)
            {
                for (var b = -50; b <= 50; b++)
                {
                    if (a >= 0 && b >= 0)
                    {
                        continue;
                    }

                    yield return Make(a, b, subtraction: false);
                    yield return Make(a, b, subtraction: true);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b, bool subtraction)
        {
            // Отрицательный второй операнд при вычитании — в скобках.
            var secondText = subtraction && b < 0 ? $"({b})" : b.ToString();
            var op = subtraction ? "−" : "+";
            return (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "C12",
                Prompt = $"Вычисли: {a} {op} {secondText}",
                Hint = subtraction
                    ? "Минус на минус даёт плюс"
                    : "Вспомни правило сложения чисел с разными знаками",
                Form = AnswerForm.Integer,
                ExpectedInteger = subtraction ? a - b : a + b,
            }, $"{(subtraction ? "sub" : "add")}:{a}:{b}");
        }
    }

    private static int Lcm(int a, int b) => a / Gcd(a, b) * b;

    private static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }

        return a;
    }
}
