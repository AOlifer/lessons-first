namespace LessonsFirst.App.Tasks;

/// <summary>Род существительного.</summary>
public enum Gender
{
    Masculine,
    Feminine,
    Neuter,
}

/// <summary>Число.</summary>
public enum NumberRu
{
    Singular,
    Plural,
}

/// <summary>Падеж.</summary>
public enum CaseRu
{
    Nominative,
    Genitive,
    Dative,
    Accusative,
    Instrumental,
    Prepositional,
}

/// <summary>Спряжение глагола.</summary>
public enum Conjugation
{
    First,
    Second,
}

/// <summary>Предметный режим сессии (выбирает родитель).</summary>
public enum SubjectMode
{
    /// <summary>Только математика.</summary>
    Math,

    /// <summary>Только русский язык.</summary>
    Russian,

    /// <summary>Чередование перемешанными парами [Математика, Русский].</summary>
    Mixed,
}

/// <summary>Грамматические термины для условий и вариантов ответа.</summary>
public static class RussianGrammar
{
    public static string Name(this CaseRu c) => c switch
    {
        CaseRu.Nominative => "именительный",
        CaseRu.Genitive => "родительный",
        CaseRu.Dative => "дательный",
        CaseRu.Accusative => "винительный",
        CaseRu.Instrumental => "творительный",
        _ => "предложный",
    };

    public static string Name(this Gender g) => g switch
    {
        Gender.Masculine => "мужской",
        Gender.Feminine => "женский",
        _ => "средний",
    };

    /// <summary>Форма родительного падежа для вопроса о роде: «мужского/женского/среднего».</summary>
    public static string GenitiveName(this Gender g) => g switch
    {
        Gender.Masculine => "мужского",
        Gender.Feminine => "женского",
        _ => "среднего",
    };

    public static string Name(this NumberRu n) => n == NumberRu.Singular ? "единственное" : "множественное";

    public static string Name(this Conjugation c) => c == Conjugation.First ? "I" : "II";
}

/// <summary>Утилиты русского текста: явный алфавит из 33 букв, гласные, подсчёты, маски.</summary>
public static class RussianTextUtil
{
    /// <summary>Русский алфавит из 33 букв, ё — сразу после е. Индексы используются
    /// для алфавитного сравнения: сортировки, объединяющие е/ё, не допускаются.</summary>
    public static readonly string Alphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";

    private static readonly HashSet<char> Vowels = new() { 'а', 'е', 'ё', 'и', 'о', 'у', 'ы', 'э', 'ю', 'я' };

    public static bool IsVowel(char c) => Vowels.Contains(char.ToLowerInvariant(c));

    /// <summary>Количество букв: только буквы (ь и ъ — буквы), без пробелов, дефисов и цифр.</summary>
    public static int CountLetters(string word) => word.Count(char.IsLetter);

    /// <summary>Количество гласных букв (а, е, ё, и, о, у, ы, э, ю, я).</summary>
    public static int CountVowels(string word) => word.Count(IsVowel);

    /// <summary>Индекс буквы в явном алфавите; -1 — не русская буква алфавита.</summary>
    public static int AlphabetIndex(char c)
    {
        var index = Alphabet.IndexOf(char.ToLowerInvariant(c));
        return index;
    }

    /// <summary>Замена буквы на знак пропуска: маскируется ровно одна позиция.</summary>
    public static string MaskAt(string word, int index) =>
        word.Length <= index || index < 0 ? word : string.Concat(word.AsSpan(0, index), "_", word.AsSpan(index + 1));

    /// <summary>Слово с одной замаскированной позицией: «жираф» → «ж_раф».</summary>
    public static string MaskedWord(string word, int index) => MaskAt(word, index);
}
