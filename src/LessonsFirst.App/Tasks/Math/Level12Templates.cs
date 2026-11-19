using static LessonsFirst.App.Tasks.RussianText;

namespace LessonsFirst.App.Tasks.MathTasks;

/// <summary>Уровень «1–2 класс — простой»: A1–A10. Только сложение/вычитание в пределах 20/100 и сравнение.</summary>
internal static class Level12Templates
{
    public static readonly IMathTemplate[] All =
    {
        new A1(), new A2(), new A3(), new A4(), new A5(),
        new A6(), new A7(), new A8(), new A9(), new A10(),
    };

    private const string Subject = "Математика";

    private sealed class A1 : MathTemplateBase
    {
        public override string Id => "A1";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var sum = Int(rng, 1, 20);
            var a = Int(rng, 0, sum);
            var b = sum - a;
            var made = Make(a, b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var sum = 1; sum <= 20; sum++)
            {
                for (var a = 0; a <= sum; a++)
                {
                    yield return Make(a, sum - a);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "A1",
                Prompt = $"Вычисли: {a} + {b}",
                Hint = "Сложи числа",
                Form = AnswerForm.Integer,
                ExpectedInteger = a + b,
            }, $"{a}+{b}");
    }

    private sealed class A2 : MathTemplateBase
    {
        public override string Id => "A2";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 0, 20);
            var b = Int(rng, 0, a);
            var made = Make(a, b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 0; a <= 20; a++)
            {
                for (var b = 0; b <= a; b++)
                {
                    yield return Make(a, b);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "A2",
                Prompt = $"Вычисли: {a} − {b}",
                Hint = "Из большего вычти меньшее",
                Form = AnswerForm.Integer,
                ExpectedInteger = a - b,
            }, $"{a}-{b}");
    }

    private sealed class A3 : MathTemplateBase
    {
        public override string Id => "A3";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var sum = Int(rng, 2, 20);
            var a = Int(rng, 1, sum - 1);
            var made = Make(a, sum - a);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 1; a <= 19; a++)
            {
                for (var b = 1; a + b <= 20; b++)
                {
                    yield return Make(a, b);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "A3",
                Prompt = $"Увеличь {a} на {b}",
                Hint = "«Увеличить на» — значит прибавить",
                Form = AnswerForm.Integer,
                ExpectedInteger = a + b,
            }, $"{a}+{b}");
    }

    private sealed class A4 : MathTemplateBase
    {
        public override string Id => "A4";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 1, 20);
            var b = Int(rng, 1, a);
            var made = Make(a, b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 1; a <= 20; a++)
            {
                for (var b = 1; b <= a; b++)
                {
                    yield return Make(a, b);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "A4",
                Prompt = $"Уменьши {a} на {b}",
                Hint = "«Уменьшить на» — значит вычесть",
                Form = AnswerForm.Integer,
                ExpectedInteger = a - b,
            }, $"{a}-{b}");
    }

    private sealed class A5 : MathTemplateBase
    {
        public override string Id => "A5";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var sum = Int(rng, 1, 20);
            var a = Int(rng, 1, sum);
            var made = Make(a, sum);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var sum = 1; sum <= 20; sum++)
            {
                for (var a = 1; a <= sum; a++)
                {
                    yield return Make(a, sum);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int sum) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "A5",
                Prompt = $"Какое число пропущено: □ + {a} = {sum}?",
                Hint = "Из суммы вычти известное слагаемое",
                Form = AnswerForm.Integer,
                ExpectedInteger = sum - a,
            }, $"{a}={sum}");
    }

    private sealed class A6 : MathTemplateBase
    {
        public override string Id => "A6";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            // Отношение равновероятно, затем — подходящие числа.
            var relation = (ComparisonRelation)Int(rng, 0, 2);
            int a, b;
            switch (relation)
            {
                case ComparisonRelation.Equal:
                    a = b = Int(rng, 0, 100);
                    break;
                case ComparisonRelation.Less:
                    a = Int(rng, 0, 99);
                    b = Int(rng, a + 1, 100);
                    break;
                default:
                    b = Int(rng, 0, 99);
                    a = Int(rng, b + 1, 100);
                    break;
            }

            var made = Make(relation, a, b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 0; a <= 100; a++)
            {
                for (var b = 0; b <= 100; b++)
                {
                    var relation = a < b ? ComparisonRelation.Less
                        : a > b ? ComparisonRelation.Greater
                        : ComparisonRelation.Equal;
                    yield return Make(relation, a, b);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(ComparisonRelation relation, int a, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "A6",
                Prompt = $"Выбери знак: {a} □ {b}",
                Hint = "Сравни числа по разрядам",
                Form = AnswerForm.Comparison,
                ExpectedRelation = relation,
            }, Branch(relation) + $":{a}:{b}");

        private static string Branch(ComparisonRelation relation) => relation switch
        {
            ComparisonRelation.Less => "L",
            ComparisonRelation.Equal => "E",
            _ => "G",
        };
    }

    private sealed class A7 : MathTemplateBase
    {
        public override string Id => "A7";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 1, 9) * 10;
            var b = Int(rng, 1, (100 - a) / 10) * 10;
            var made = Make(a, b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 10; a <= 90; a += 10)
            {
                for (var b = 10; a + b <= 100; b += 10)
                {
                    yield return Make(a, b);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "A7",
                Prompt = $"Вычисли: {a} + {b}",
                Hint = "Складывай десятки",
                Form = AnswerForm.Integer,
                ExpectedInteger = a + b,
            }, $"{a}+{b}");
    }

    private sealed class A8 : MathTemplateBase
    {
        public override string Id => "A8";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 1, 10) * 10;
            var b = Int(rng, 1, a / 10) * 10;
            var made = Make(a, b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 10; a <= 100; a += 10)
            {
                for (var b = 10; b <= a; b += 10)
                {
                    yield return Make(a, b);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "A8",
                Prompt = $"Вычисли: {a} − {b}",
                Hint = "Вычитай десятки",
                Form = AnswerForm.Integer,
                ExpectedInteger = a - b,
            }, $"{a}-{b}");
    }

    private sealed class A9 : MathTemplateBase
    {
        public override string Id => "A9";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var sum = Int(rng, 2, 20);
            var a = Int(rng, 1, sum - 1);
            var made = Make(a, sum - a, rng);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(rng), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable(Random rng)
        {
            for (var a = 1; a <= 19; a++)
            {
                for (var b = 1; a + b <= 20; b++)
                {
                    yield return Make(a, b, rng);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b, Random rng)
        {
            var name = RandomGirl(rng);
            var item = RandomItem(rng);
            return (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "A9",
                Prompt = $"У {name} {Counted(a, item)}. Ей подарили ещё {Counted(b, item)}. "
                    + $"Сколько {item.Many} стало?",
                Hint = "Задача на сложение",
                Form = AnswerForm.Integer,
                ExpectedInteger = a + b,
            }, $"{a}+{b}");
        }
    }

    private sealed class A10 : MathTemplateBase
    {
        public override string Id => "A10";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 2, 20);
            var b = Int(rng, 1, a - 1);
            var made = Make(a, b, rng);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(rng), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable(Random rng)
        {
            for (var a = 2; a <= 20; a++)
            {
                for (var b = 1; b < a; b++)
                {
                    yield return Make(a, b, rng);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b, Random rng)
        {
            var item = RandomItem(rng);
            var first = Counted(b, item);
            return (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "A10",
                Prompt = $"Было {Counted(a, item)}. {char.ToUpper(first[0])}{first[1..]} отдали. "
                    + $"Сколько {item.Many} осталось?",
                Hint = "Задача на вычитание",
                Form = AnswerForm.Integer,
                ExpectedInteger = a - b,
            }, $"{a}-{b}");
        }
    }
}
