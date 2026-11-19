using static LessonsFirst.App.Tasks.RussianText;

namespace LessonsFirst.App.Tasks.MathTasks;

/// <summary>Уровень «3–4 класс — базовый»: математический диктант B1–B12.</summary>
internal static class Level34Templates
{
    public static readonly IMathTemplate[] All =
    {
        new B1(), new B2(), new B3(), new B4(), new B5(),
        new B6(), new B7(), new B8(), new B9(), new B10(),
        new B11(), new B12(),
    };

    private const string Subject = "Математика";

    private sealed class B1 : MathTemplateBase
    {
        public override string Id => "B1";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 2, 9);
            var b = Int(rng, 2, 9);
            var made = Make(a, b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 2; a <= 9; a++)
            {
                for (var b = 2; b <= 9; b++)
                {
                    yield return Make(a, b);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "B1",
                Prompt = $"Найди произведение чисел {a} и {b}",
                Hint = "Произведение — результат умножения",
                Form = AnswerForm.Integer,
                ExpectedInteger = a * b,
            }, $"{a}×{b}");
    }

    private sealed class B2 : MathTemplateBase
    {
        public override string Id => "B2";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var divisor = Int(rng, 2, 9);
            var quotient = Int(rng, 2, 9);
            var made = Make(divisor * quotient, divisor);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var divisor = 2; divisor <= 9; divisor++)
            {
                for (var quotient = 2; quotient <= 9; quotient++)
                {
                    yield return Make(divisor * quotient, divisor);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int dividend, int divisor) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "B2",
                Prompt = $"Найди частное чисел {dividend} и {divisor}",
                Hint = "Частное — результат деления",
                Form = AnswerForm.Integer,
                ExpectedInteger = dividend / divisor,
            }, $"{dividend}:{divisor}");
    }

    private sealed class B3 : MathTemplateBase
    {
        public override string Id => "B3";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 2, 9);
            var b = Int(rng, 2, 9);
            var made = Make(a * b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        /// <summary>Все различные произведения пар 2…9 × 2…9.</summary>
        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            var products = new SortedSet<int>();
            for (var a = 2; a <= 9; a++)
            {
                for (var b = 2; b <= 9; b++)
                {
                    products.Add(a * b);
                }
            }

            foreach (var product in products)
            {
                yield return Make(product);
            }
        }

        private static (SchoolTask Task, string Key) Make(int product)
        {
            var first = 0;
            for (var a = 2; a <= 9; a++)
            {
                if (product % a == 0 && product / a is >= 2 and <= 9)
                {
                    first = a;
                    break;
                }
            }

            return (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "B3",
                Prompt = $"Назови два числа от 2 до 9, произведение которых равно {product}",
                Hint = "Вспомни таблицу умножения; числа можно назвать в любом порядке",
                Form = AnswerForm.FactorPair,
                ExpectedFactors = (first, product / first),
            }, product.ToString());
        }
    }

    private sealed class B4 : MathTemplateBase
    {
        public override string Id => "B4";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 2, 9);
            var k = Int(rng, 2, 9);
            var made = Make(a, k);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(SharedEnumerable(), used, out task, out key);

        internal static IEnumerable<(SchoolTask Task, string Key)> SharedEnumerable()
        {
            for (var a = 2; a <= 9; a++)
            {
                for (var k = 2; k <= 9; k++)
                {
                    yield return (new SchoolTask
                    {
                        Subject = Subject,
                        TemplateId = "B4",
                        Prompt = $"Увеличь {a} в {k} {Raz(k)}",
                        Hint = "«Увеличить в несколько раз» — умножить",
                        Form = AnswerForm.Integer,
                        ExpectedInteger = a * k,
                    }, $"{a}×{k}");
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int k) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "B4",
                Prompt = $"Увеличь {a} в {k} {Raz(k)}",
                Hint = "«Увеличить в несколько раз» — умножить",
                Form = AnswerForm.Integer,
                ExpectedInteger = a * k,
            }, $"{a}×{k}");
    }

    private sealed class B5 : MathTemplateBase
    {
        public override string Id => "B5";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 1, 99);
            var b = Int(rng, 1, 100 - a);
            var made = Make(a, b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 1; a <= 99; a++)
            {
                for (var b = 1; a + b <= 100; b++)
                {
                    yield return Make(a, b);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "B5",
                Prompt = $"Увеличь {a} на {b}",
                Hint = "«Увеличить на» — прибавить",
                Form = AnswerForm.Integer,
                ExpectedInteger = a + b,
            }, $"{a}+{b}");
    }

    private sealed class B6 : MathTemplateBase
    {
        public override string Id => "B6";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var quotient = Int(rng, 2, 9);
            var k = Int(rng, 2, 9);
            var made = Make(quotient * k, k);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var quotient = 2; quotient <= 9; quotient++)
            {
                for (var k = 2; k <= 9; k++)
                {
                    yield return Make(quotient * k, k);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int k) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "B6",
                Prompt = $"Уменьши {a} в {k} {Raz(k)}",
                Hint = "«Уменьшить в несколько раз» — разделить",
                Form = AnswerForm.Integer,
                ExpectedInteger = a / k,
            }, $"{a}:{k}");
    }

    private sealed class B7 : MathTemplateBase
    {
        public override string Id => "B7";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 2, 99);
            var b = Int(rng, 1, a - 1);
            var made = Make(a, b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 2; a <= 99; a++)
            {
                for (var b = 1; b < a; b++)
                {
                    yield return Make(a, b);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "B7",
                Prompt = $"Уменьши {a} на {b}",
                Hint = "«Уменьшить на» — вычесть",
                Form = AnswerForm.Integer,
                ExpectedInteger = a - b,
            }, $"{a}-{b}");
    }

    private sealed class B8 : MathTemplateBase
    {
        public override string Id => "B8";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var a = Int(rng, 2, 9);
            var b = Int(rng, 2, 9);
            var made = Make(a, b);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var a = 2; a <= 9; a++)
            {
                for (var b = 2; b <= 9; b++)
                {
                    yield return Make(a, b);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int a, int b) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "B8",
                Prompt = $"Один множитель — {a}, другой — {b}. Найди произведение",
                Hint = "Перемножь множители",
                Form = AnswerForm.Integer,
                ExpectedInteger = a * b,
            }, $"{a}×{b}");
    }

    private sealed class B9 : MathTemplateBase
    {
        public override string Id => "B9";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var divisor = Int(rng, 2, 9);
            var quotient = Int(rng, 2, 9);
            var made = Make(divisor * quotient, divisor);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var divisor = 2; divisor <= 9; divisor++)
            {
                for (var quotient = 2; quotient <= 9; quotient++)
                {
                    yield return Make(divisor * quotient, divisor);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int dividend, int divisor) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "B9",
                Prompt = $"Делимое — {dividend}, делитель — {divisor}. Найди частное",
                Hint = "Делимое делят на делитель",
                Form = AnswerForm.Integer,
                ExpectedInteger = dividend / divisor,
            }, $"{dividend}:{divisor}");
    }

    private sealed class B10 : MathTemplateBase
    {
        public override string Id => "B10";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var divisor = Int(rng, 2, 9);
            var quotient = Int(rng, 2, 9);
            var made = Make(divisor * quotient, quotient);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var divisor = 2; divisor <= 9; divisor++)
            {
                for (var quotient = 2; quotient <= 9; quotient++)
                {
                    yield return Make(divisor * quotient, quotient);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int dividend, int quotient) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "B10",
                Prompt = $"На какое число нужно разделить {dividend}, чтобы получить {quotient}?",
                Hint = "Ищи неизвестный делитель: делимое дели на частное",
                Form = AnswerForm.Integer,
                ExpectedInteger = dividend / quotient,
            }, $"{dividend}:{quotient}");
    }

    private sealed class B11 : MathTemplateBase
    {
        public override string Id => "B11";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var small = Int(rng, 2, 9);
            var coefficient = Int(rng, 2, 9);
            var made = Make(small, coefficient);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var small = 2; small <= 9; small++)
            {
                for (var coefficient = 2; coefficient <= 9; coefficient++)
                {
                    yield return Make(small, coefficient);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int small, int coefficient)
        {
            var big = small * coefficient;
            return (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "B11",
                Prompt = $"В большой упаковке {Counted(big, Chocolate)}, а в маленькой — в {coefficient} {Raz(coefficient)} меньше. "
                    + $"Сколько {Chocolate.Many} в маленькой упаковке?",
                Hint = "«В несколько раз меньше» — деление",
                Form = AnswerForm.Integer,
                ExpectedInteger = small,
            }, $"{big}:{coefficient}");
        }
    }

    private sealed class B12 : MathTemplateBase
    {
        public override string Id => "B12";

        public override SchoolTask Build(Random rng, out string canonicalKey)
        {
            var small = Int(rng, 1, 99);
            var add = Int(rng, 1, 100 - small);
            var made = Make(small, add);
            canonicalKey = made.Key;
            return made.Task;
        }

        public override bool TryPickUnused(HashSet<string> used, Random rng, out SchoolTask task, out string key) =>
            FirstUnused(Enumerable(), used, out task, out key);

        private static IEnumerable<(SchoolTask Task, string Key)> Enumerable()
        {
            for (var small = 1; small <= 99; small++)
            {
                for (var add = 1; small + add <= 100; add++)
                {
                    yield return Make(small, add);
                }
            }
        }

        private static (SchoolTask Task, string Key) Make(int small, int add) =>
            (new SchoolTask
            {
                Subject = Subject,
                TemplateId = "B12",
                Prompt = $"В маленькой коробке {Counted(small, Toy)}, а в большой — на {Counted(add, Toy)} больше. "
                    + $"Сколько {Toy.Many} в большой коробке?",
                Hint = "«На несколько больше» — сложение",
                Form = AnswerForm.Integer,
                ExpectedInteger = small + add,
            }, $"{small}+{add}");
    }
}
