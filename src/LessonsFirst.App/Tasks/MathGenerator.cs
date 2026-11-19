namespace LessonsFirst.App.Tasks;

/// <summary>Генератор задач по математике: сложность зависит от класса.</summary>
public sealed class MathGenerator : ITaskGenerator
{
    private const string Subject = "Математика";

    private readonly GradeBand _band;

    public MathGenerator(GradeBand band) => _band = band;

    public SchoolTask Generate(Random rng) => _band switch
    {
        GradeBand.Grades12 => GenerateGrades12(rng),
        GradeBand.Grades34 => GenerateGrades34(rng),
        _ => GenerateGrades56(rng),
    };

    private static SchoolTask GenerateGrades12(Random rng) => rng.Next(3) switch
    {
        0 => Addition(rng),
        1 => Subtraction(rng),
        _ => MissingAddend(rng),
    };

    private static SchoolTask Addition(Random rng)
    {
        var a = rng.Next(2, 60);
        var b = rng.Next(2, Math.Min(60, 100 - a + 1));
        return new SchoolTask
        {
            Subject = Subject,
            Prompt = $"Сколько будет {a} + {b}?",
            CorrectAnswer = (a + b).ToString(),
            IsTextInput = true,
        };
    }

    private static SchoolTask Subtraction(Random rng)
    {
        var a = rng.Next(11, 100);
        var b = rng.Next(2, a);
        return new SchoolTask
        {
            Subject = Subject,
            Prompt = $"Сколько будет {a} − {b}?",
            CorrectAnswer = (a - b).ToString(),
            IsTextInput = true,
        };
    }

    private static SchoolTask MissingAddend(Random rng)
    {
        var a = rng.Next(2, 40);
        var b = rng.Next(2, 40);
        return new SchoolTask
        {
            Subject = Subject,
            Prompt = $"{a} + ⬜ = {a + b}. Какое число спряталось?",
            CorrectAnswer = b.ToString(),
            IsTextInput = true,
            Hint = "Подбери число, которое прибавили",
        };
    }

    private static SchoolTask GenerateGrades34(Random rng) => rng.Next(4) switch
    {
        0 => Multiplication(rng),
        1 => Division(rng),
        2 => TwoStepExpression(rng),
        _ => MissingFactor(rng),
    };

    private static SchoolTask Multiplication(Random rng)
    {
        var a = rng.Next(2, 11);
        var b = rng.Next(2, 11);
        return new SchoolTask
        {
            Subject = Subject,
            Prompt = $"Сколько будет {a} × {b}?",
            CorrectAnswer = (a * b).ToString(),
            IsTextInput = true,
        };
    }

    private static SchoolTask Division(Random rng)
    {
        var b = rng.Next(2, 11);
        var q = rng.Next(2, 11);
        return new SchoolTask
        {
            Subject = Subject,
            Prompt = $"Сколько будет {b * q} : {b}?",
            CorrectAnswer = q.ToString(),
            IsTextInput = true,
        };
    }

    private static SchoolTask TwoStepExpression(Random rng)
    {
        var a = rng.Next(2, 11);
        var b = rng.Next(2, 11);
        if (rng.Next(2) == 0)
        {
            var c = rng.Next(2, 20);
            // Сначала умножение, потом сложение — порядок действий.
            return new SchoolTask
            {
                Subject = Subject,
                Prompt = $"Сколько будет {c} + {a} × {b}?",
                CorrectAnswer = (c + a * b).ToString(),
                IsTextInput = true,
                Hint = "Сначала умножение, потом сложение",
            };
        }

        // Скобки меняют порядок действий.
        var big = rng.Next(a + 1, a + 11);
        return new SchoolTask
        {
            Subject = Subject,
            Prompt = $"Сколько будет ({big} − {a}) × {b}?",
            CorrectAnswer = ((big - a) * b).ToString(),
            IsTextInput = true,
            Hint = "Сначала посчитай в скобках",
        };
    }

    private static SchoolTask MissingFactor(Random rng)
    {
        var factor = rng.Next(2, 11);
        var product = factor * rng.Next(2, 11);
        return new SchoolTask
        {
            Subject = Subject,
            Prompt = $"⬜ × {factor} = {product}. Какое число спряталось?",
            CorrectAnswer = (product / factor).ToString(),
            IsTextInput = true,
            Hint = "Вспомни таблицу умножения",
        };
    }

    private static SchoolTask GenerateGrades56(Random rng) => rng.Next(4) switch
    {
        0 => Percent(rng),
        1 => FractionAddition(rng),
        2 => ThreeStepExpression(rng),
        _ => LongDivision(rng),
    };

    private static SchoolTask Percent(Random rng)
    {
        var percent = new[] { 10, 20, 25, 50, 75 }[rng.Next(5)];
        // Числа, кратные 20: любой из процентов даёт целый ответ.
        var number = rng.Next(2, 21) * 20;
        return new SchoolTask
        {
            Subject = Subject,
            Prompt = $"Найди {percent}% от {number}. Запиши число.",
            CorrectAnswer = (number * percent / 100).ToString(),
            IsTextInput = true,
            Hint = "1% — это число, делённое на 100",
        };
    }

    private static SchoolTask FractionAddition(Random rng)
    {
        var denominator = rng.Next(3, 10);
        var a = rng.Next(1, denominator);
        var b = rng.Next(1, denominator);
        return new SchoolTask
        {
            Subject = Subject,
            Prompt = $"Сколько будет {a}/{denominator} + {b}/{denominator}? Запиши дробь, например 3/4.",
            CorrectAnswer = $"{a + b}/{denominator}",
            IsTextInput = true,
            IsFraction = true,
            Hint = "Знаменатели одинаковые — складывай числители",
        };
    }

    private static SchoolTask ThreeStepExpression(Random rng)
    {
        var a = rng.Next(2, 13);
        var b = rng.Next(2, 10);
        var c = rng.Next(2, 50);
        var d = rng.Next(1, a * b);
        return new SchoolTask
        {
            Subject = Subject,
            Prompt = $"Сколько будет {c} + {a} × {b} − {d}?",
            CorrectAnswer = (c + a * b - d).ToString(),
            IsTextInput = true,
            Hint = "Сначала умножение",
        };
    }

    private static SchoolTask LongDivision(Random rng)
    {
        var divisor = rng.Next(3, 13);
        var quotient = rng.Next(3, 13);
        return new SchoolTask
        {
            Subject = Subject,
            Prompt = $"Сколько будет {divisor * quotient} : {divisor}?",
            CorrectAnswer = quotient.ToString(),
            IsTextInput = true,
        };
    }
}
