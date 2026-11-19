namespace LessonsFirst.App.Tasks;

/// <summary>Задача с выбором варианта: текст задания + правильный ответ + дистракторы.</summary>
public sealed record ChoiceItem(string Prompt, string Correct, params string[] Wrongs);

/// <summary>Генератор задач по русскому языку: орфограммы, формы слов, тся/ться.</summary>
public sealed class RussianGenerator : ITaskGenerator
{
    private const string Subject = "Русский язык";

    private static readonly char[] VowelSwaps = { 'а', 'о', 'е', 'и', 'я', 'ё', 'у', 'ю' };

    private static readonly ChoiceItem[] Grades12Words =
    {
        new("Выбери букву: с_бака", "о", "а"),
        new("Выбери букву: к_рова", "о", "а"),
        new("Выбери букву: м_локо", "о", "а"),
        new("Выбери букву: б_рёза", "е", "и"),
        new("Выбери букву: в_рона", "о", "а"),
        new("Выбери букву: с_рока", "о", "а"),
        new("Выбери букву: п_нал", "е", "и"),
        new("Выбери букву: т_традь", "е", "и"),
        new("Выбери букву: к_рандаш", "а", "о"),
        new("Выбери букву: ябл_ко", "о", "а"),
        new("Выбери букву: хор_шо", "о", "а"),
        new("Выбери букву: г_род", "о", "а"),
        new("Выбери букву: д_ревня", "е", "и"),
        new("Выбери букву: м_дведь", "е", "и"),
        new("Выбери букву: л_сица", "и", "е"),
        new("Выбери букву: скв_рец", "о", "а"),
    };

    private static readonly ChoiceItem[] Grades34Words =
    {
        new("Выбери букву: акк_ратный", "у", "ю"),
        new("Выбери букву: басс_ейн", "е", "и"),
        new("Выбери букву: в_кзал", "о", "а"),
        new("Выбери букву: грамм_тика", "а", "о"),
        new("Выбери букву: дир_ктор", "е", "и"),
        new("Выбери букву: инж_нер", "е", "и"),
        new("Выбери букву: кил_метр", "о", "а"),
        new("Выбери букву: л_стница", "е", "и"),
        new("Выбери букву: р_сстояние", "а", "о"),
        new("Выбери букву: сам_лёт", "о", "а"),
        new("Выбери букву: т_левизор", "е", "и"),
        new("Выбери букву: к_стрюля", "а", "о"),
        new("Выбери букву: к_мбайн", "о", "а"),
        new("Выбери букву: м_роженое", "о", "а"),
        new("Выбери букву: р_мон", "е", "и"),
        new("Выбери букву: сал_ты", "а", "о"),
    };

    private static readonly ChoiceItem[] Grades56Words =
    {
        new("Выбери букву: ш_пот", "ё", "о"),
        new("Выбери букву: ш_лк", "ё", "о"),
        new("Выбери букву: ж_лтый", "ё", "о"),
        new("Выбери букву: ч_рный", "ё", "о"),
        new("Выбери букву: щ_чка", "ё", "о"),
        new("Выбери букву: под_езд", "ъ", "ь"),
        new("Выбери букву: с_езд", "ъ", "ь"),
        new("Выбери букву: в_юга", "ь", "ъ"),
        new("Он улыбает… — как правильно?", "ся", "ться"),
        new("Он учит… — как правильно?", "ся", "ться"),
        new("Она радует… — как правильно?", "ся", "ться"),
        new("Задача решает… — как правильно?", "ся", "ться"),
        new("Выбери букву: б_гаж", "а", "о"),
        new("Выбери букву: к_мбайнёр", "о", "а"),
        new("Выбери букву: прив_легия", "и", "е"),
        new("Выбери букву: арт_ллерия", "и", "е"),
    };

    private static readonly ChoiceItem[] WordForms =
    {
        new("Пять … (помидор)", "помидоров", "помидоры"),
        new("Много … (яблоки)", "яблок", "яблоков"),
        new("Пять … (карандаши)", "карандашей", "карандашов"),
        new("Много … (ботинки)", "ботинок", "ботинков"),
        new("Пара … (сапоги)", "сапог", "сапогов"),
        new("Много … (книги)", "книг", "книгов"),
        new("Пять … (стулья)", "стульев", "стулов"),
        new("Много … (дела)", "дел", "делов"),
    };

    private readonly GradeBand _band;

    public RussianGenerator(GradeBand band) => _band = band;

    public SchoolTask Generate(Random rng)
    {
        var words = _band switch
        {
            GradeBand.Grades12 => Grades12Words,
            GradeBand.Grades34 => Grades34Words,
            _ => Grades56Words,
        };

        // 70% — орфограммы, 30% — формы слов.
        if (rng.Next(10) < 7)
            return FromItem(words[rng.Next(words.Length)], rng);

        return FromItem(WordForms[rng.Next(WordForms.Length)], rng);
    }

    private static SchoolTask FromItem(ChoiceItem item, Random rng)
    {
        var options = BuildOptions(item, rng);
        return new SchoolTask
        {
            Subject = Subject,
            Prompt = item.Prompt,
            CorrectAnswer = item.Correct,
            Options = options,
            IsTextInput = false,
        };
    }

    private static IReadOnlyList<string> BuildOptions(ChoiceItem item, Random rng)
    {
        var set = new List<string> { item.Correct };
        set.AddRange(item.Wrongs);

        // Для однобуквенных вариантов добавим третий дистрактор из «парных» гласных,
        // чтобы кнопок было три и угадать было сложнее.
        if (item.Correct.Length == 1 && set.Count < 3 && char.ToLower(item.Correct[0]) is var c && VowelSwaps.Contains(c))
        {
            var swap = c switch
            {
                'о' => 'а',
                'а' => 'о',
                'е' => 'и',
                'и' => 'е',
                'я' => 'и',
                'ё' => 'о',
                'у' => 'ю',
                'ю' => 'у',
                _ => 'а',
            };
            set.Add(swap.ToString());
        }

        var distinct = set.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        Shuffle(distinct, rng);
        return distinct;
    }

    private static void Shuffle<T>(IList<T> list, Random rng)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
