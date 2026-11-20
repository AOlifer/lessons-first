namespace LessonsFirst.App.Tasks.RussianTasks;

/// <summary>
/// Проверенная парадигма существительного: 12 словоформ (6 падежей × 2 числа).
/// Формы хранятся как данные; ответ получается выбором формы, а не приклеиванием окончаний.
/// </summary>
public sealed record NounForms(
    string NomSg, string GenSg, string DatSg, string AccSg, string InsSg, string PrepSg,
    string NomPl, string GenPl, string DatPl, string AccPl, string InsPl, string PrepPl)
{
    public string Get(CaseRu c, NumberRu n) => (n, c) switch
    {
        (NumberRu.Singular, CaseRu.Nominative) => NomSg,
        (NumberRu.Singular, CaseRu.Genitive) => GenSg,
        (NumberRu.Singular, CaseRu.Dative) => DatSg,
        (NumberRu.Singular, CaseRu.Accusative) => AccSg,
        (NumberRu.Singular, CaseRu.Instrumental) => InsSg,
        (NumberRu.Singular, CaseRu.Prepositional) => PrepSg,
        (_, CaseRu.Nominative) => NomPl,
        (_, CaseRu.Genitive) => GenPl,
        (_, CaseRu.Dative) => DatPl,
        (_, CaseRu.Accusative) => AccPl,
        (_, CaseRu.Instrumental) => InsPl,
        _ => PrepPl,
    };
}

/// <summary>Существительное банка: лемма, грамматика, семантический класс, парадигма.</summary>
public sealed record NounLexeme(
    string LexemeId,
    string Lemma,
    Gender Gender,
    int Declension,
    bool Animate,
    string SemClass,
    NounForms Forms)
{
    public string Form(CaseRu c, NumberRu n) => Forms.Get(c, n);

    /// <summary>Винительный с учётом одушевлённости уже учтён в данных (AccSg/AccPl).</summary>
    public bool Is(CaseRu c, NumberRu n, string form) =>
        string.Equals(Forms.Get(c, n), form, StringComparison.Ordinal);
}

/// <summary>Тип основы прилагательного. Окончания — проверенные таблицы
/// (все прилагательные банка с безударными окончаниями, основы на согласный без чередований).</summary>
public enum AdjStemType
{
    Hard,
    Soft,
}

/// <summary>Прилагательное: лемма, основа и тип. Формы собираются из основы
/// и проверенной таблицы окончаний; нормативные варианты -ой/-ою, -ей/-ею принимаются оба.</summary>
public sealed record AdjectiveLexeme(string LexemeId, string Lemma, string Stem, AdjStemType Type)
{
    // Порядок окончаний — по enum CaseRu (Им, Род, Дат, Вин, Твор, Пред).
    // Одушевлённый винительный сводится к родительному падежу при сборке.
    private static readonly string[] HardM = { "ый", "ого", "ому", "ый", "ым", "ом" };
    private static readonly string[] HardF = { "ая", "ой", "ой", "ую", "ой", "ой" };
    private static readonly string[] HardFAlt = { "ая", "ой", "ой", "ую", "ою", "ою" }; // творительный -ою (норм.)
    private static readonly string[] HardN = { "ое", "ого", "ому", "ое", "ым", "ом" };
    private static readonly string[] HardPl = { "ые", "ых", "ым", "ые", "ыми", "ых" };

    private static readonly string[] SoftM = { "ий", "его", "ему", "ий", "им", "ем" };
    private static readonly string[] SoftF = { "яя", "ей", "ей", "юю", "ей", "ей" };
    private static readonly string[] SoftFAlt = { "яя", "ей", "ей", "юю", "ею", "ею" };
    private static readonly string[] SoftN = { "ее", "его", "ему", "ее", "им", "ем" };
    private static readonly string[] SoftPl = { "ие", "их", "им", "ие", "ими", "их" };

    /// <summary>Мужской или средний род, единственное число.</summary>
    public string MasculineOrNeuterForm(Gender gender, CaseRu c, bool animate)
    {
        var endings = gender == Gender.Masculine
            ? (Type == AdjStemType.Hard ? HardM : SoftM)
            : (Type == AdjStemType.Hard ? HardN : SoftN);
        var i = (int)c;
        if (c == CaseRu.Accusative && animate)
            i = (int)CaseRu.Genitive;
        return Stem + endings[i];
    }

    public string PluralForm(CaseRu c, bool animate)
    {
        var endings = Type == AdjStemType.Hard ? HardPl : SoftPl;
        var i = (int)c;
        if (c == CaseRu.Accusative && animate)
            i = (int)CaseRu.Genitive;
        return Stem + endings[i];
    }

    public string FeminineForm(CaseRu c, bool alternative)
    {
        var endings = Type == AdjStemType.Hard ? HardF : SoftF;
        var alt = Type == AdjStemType.Hard ? HardFAlt : SoftFAlt;
        return Stem + (alternative ? alt[(int)c] : endings[(int)c]);
    }

    /// <summary>Все нормативно допустимые формы: женский творительный допускает -ой/-ою (-ей/-ею).</summary>
    public IReadOnlyList<string> Accepted(Gender gender, NumberRu number, CaseRu c, bool animate)
    {
        if (number == NumberRu.Plural)
            return new[] { PluralForm(c, animate) };

        if (gender == Gender.Feminine)
        {
            var main = FeminineForm(c, alternative: false);
            var alt = FeminineForm(c, alternative: true);
            return main == alt ? new[] { main } : new[] { main, alt };
        }

        return new[] { MasculineOrNeuterForm(gender, c, animate) };
    }
}

/// <summary>Глагол: инфинитив, спряжение (исключения помечены), настоящие формы
/// (6 личных форм), проверенное дополнение для конструкций.</summary>
public sealed record VerbLexeme(
    string LexemeId,
    string Infinitive,
    Conjugation Conj,
    bool IsException,
    string S1, string S2, string S3, string P1, string P2, string P3,
    string? Complement)
{
    /// <summary>Форма по лицу (1..3) и числу.</summary>
    public string Present(int person, NumberRu n)
    {
        return (person, n) switch
        {
            (1, NumberRu.Singular) => S1,
            (2, NumberRu.Singular) => S2,
            (3, NumberRu.Singular) => S3,
            (1, _) => P1,
            (2, _) => P2,
            _ => P3,
        };
    }
}

/// <summary>Проверенный банк русского языка. Все записи выверены по школьной норме;
/// источник выверки — орфографические словари и правила (см. контрольные примеры в тестах).</summary>
public static class RussianLexicon
{
    public const string Subject = "Русский язык";

    // ————— Существительные (полные парадигмы) —————
    public static readonly NounLexeme[] Nouns =
    {
        new("n-muzey", "музей", Gender.Masculine, 2, false, "place",
            new NounForms("музей", "музея", "музею", "музей", "музеем", "музее",
                "музеи", "музеев", "музеям", "музеи", "музеями", "музеях")),
        new("n-gorod", "город", Gender.Masculine, 2, false, "place",
            new NounForms("город", "города", "городу", "город", "городом", "городе",
                "города", "городов", "городам", "города", "городами", "городах")),
        new("n-dom", "дом", Gender.Masculine, 2, false, "place",
            new NounForms("дом", "дома", "дому", "дом", "домом", "доме",
                "дома", "домов", "домам", "дома", "домами", "домах")),
        new("n-park", "парк", Gender.Masculine, 2, false, "place",
            new NounForms("парк", "парка", "парку", "парк", "парком", "парке",
                "парки", "парков", "паркам", "парки", "парками", "парках")),
        new("n-les", "лес", Gender.Masculine, 2, false, "nature",
            new NounForms("лес", "леса", "лесу", "лес", "лесом", "лесе",
                "леса", "лесов", "лесам", "леса", "лесами", "лесах")),
        new("n-shkola", "школа", Gender.Feminine, 1, false, "place",
            new NounForms("школа", "школы", "школе", "школу", "школой", "школе",
                "школы", "школ", "школам", "школы", "школами", "школах")),
        new("n-derevnya", "деревня", Gender.Feminine, 1, false, "place",
            new NounForms("деревня", "деревни", "деревне", "деревню", "деревней", "деревне",
                "деревни", "деревень", "деревням", "деревни", "деревнями", "деревнях")),
        new("n-mama", "мама", Gender.Feminine, 1, true, "person",
            new NounForms("мама", "мамы", "маме", "маму", "мамой", "маме",
                "мамы", "мам", "мамам", "мам", "мамами", "мамах")),
        new("n-papa", "папа", Gender.Masculine, 1, true, "person",
            new NounForms("папа", "папы", "папе", "папу", "папой", "папе",
                "папы", "пап", "папам", "пап", "папами", "папах")),
        new("n-devochka", "девочка", Gender.Feminine, 1, true, "person",
            new NounForms("девочка", "девочки", "девочке", "девочку", "девочкой", "девочке",
                "девочки", "девочек", "девочкам", "девочек", "девочками", "девочках")),
        new("n-dedushka", "дедушка", Gender.Masculine, 1, true, "person",
            new NounForms("дедушка", "дедушки", "дедушке", "дедушку", "дедушкой", "дедушке",
                "дедушки", "дедушек", "дедушкам", "дедушек", "дедушками", "дедушках")),
        new("n-sestra", "сестра", Gender.Feminine, 1, true, "person",
            new NounForms("сестра", "сестры", "сестре", "сестру", "сестрой", "сестре",
                "сёстры", "сестёр", "сёстрам", "сестёр", "сёстрами", "сёстрах")),
        new("n-brat", "брат", Gender.Masculine, 2, true, "person",
            new NounForms("брат", "брата", "брату", "брата", "братом", "брате",
                "братья", "братьев", "братьям", "братьев", "братьями", "братьях")),
        new("n-kon", "конь", Gender.Masculine, 2, true, "animal",
            new NounForms("конь", "коня", "коню", "коня", "конём", "коне",
                "кони", "коней", "коням", "коней", "конями", "конях")),
        new("n-noch", "ночь", Gender.Feminine, 3, false, "time",
            new NounForms("ночь", "ночи", "ночи", "ночь", "ночью", "ночи",
                "ночи", "ночей", "ночам", "ночи", "ночами", "ночах")),
        new("n-tetrad", "тетрадь", Gender.Feminine, 3, false, "object",
            new NounForms("тетрадь", "тетради", "тетради", "тетрадь", "тетрадью", "тетради",
                "тетради", "тетрадей", "тетрадям", "тетради", "тетрадями", "тетрадях")),
        new("n-okno", "окно", Gender.Neuter, 2, false, "object",
            new NounForms("окно", "окна", "окну", "окно", "окном", "окне",
                "окна", "окон", "окнам", "окна", "окнами", "окнах")),
        new("n-more", "море", Gender.Neuter, 2, false, "nature",
            new NounForms("море", "моря", "морю", "море", "морем", "море",
                "моря", "морей", "морям", "моря", "морями", "морях")),
        new("n-derevo", "дерево", Gender.Neuter, 2, false, "plant",
            new NounForms("дерево", "дерева", "дереву", "дерево", "деревом", "дереве",
                "деревья", "деревьев", "деревьям", "деревья", "деревьями", "деревьях")),
        new("n-cvetok", "цветок", Gender.Masculine, 2, false, "plant",
            new NounForms("цветок", "цветка", "цветку", "цветок", "цветком", "цветке",
                "цветы", "цветов", "цветам", "цветы", "цветами", "цветах")),
        new("n-kartina", "картина", Gender.Feminine, 1, false, "object",
            new NounForms("картина", "картины", "картине", "картину", "картиной", "картине",
                "картины", "картин", "картинам", "картины", "картинами", "картинах")),
        new("n-ryukzak", "рюкзак", Gender.Masculine, 2, false, "object",
            new NounForms("рюкзак", "рюкзака", "рюкзаку", "рюкзак", "рюкзаком", "рюкзаке",
                "рюкзаки", "рюкзаков", "рюкзакам", "рюкзаки", "рюкзаками", "рюкзаках")),
        new("n-zont", "зонт", Gender.Masculine, 2, false, "object",
            new NounForms("зонт", "зонта", "зонту", "зонт", "зонтом", "зонте",
                "зонты", "зонтов", "зонтам", "зонты", "зонтами", "зонтах")),
        new("n-zakat", "закат", Gender.Masculine, 2, false, "nature",
            new NounForms("закат", "заката", "закату", "закат", "закатом", "закате",
                "закаты", "закатов", "закатам", "закаты", "закатами", "закатах")),
    };

    public static NounLexeme Noun(string lexemeId) =>
        Nouns.Single(n => n.LexemeId == lexemeId);

    // ————— Прилагательные (основа + проверенный тип) —————
    public static readonly AdjectiveLexeme[] Adjectives =
    {
        new("a-novyi", "новый", "нов", AdjStemType.Hard),
        new("a-staryi", "старый", "стар", AdjStemType.Hard),
        new("a-krasnyi", "красный", "красн", AdjStemType.Hard),
        new("a-ogromnyi", "огромный", "огромн", AdjStemType.Hard),
        new("a-krasivyi", "красивый", "красив", AdjStemType.Hard),
        new("a-zelenyi", "зелёный", "зелён", AdjStemType.Hard),
        new("a-teplyi", "тёплый", "тёпл", AdjStemType.Hard),
        new("a-veselyi", "весёлый", "весёл", AdjStemType.Hard),
        new("a-umnyi", "умный", "умн", AdjStemType.Hard),
        new("a-dobryi", "добрый", "добр", AdjStemType.Hard),
        new("a-svetlyi", "светлый", "светл", AdjStemType.Hard),
        new("a-chistyi", "чистый", "чист", AdjStemType.Hard),
        new("a-osennii", "осенний", "осенн", AdjStemType.Soft),
        new("a-zimnii", "зимний", "зимн", AdjStemType.Soft),
        new("a-letnii", "летний", "летн", AdjStemType.Soft),
        new("a-sinii", "синий", "син", AdjStemType.Soft),
        new("a-utrennii", "утренний", "утренн", AdjStemType.Soft),
        new("a-vechernnii", "вечерний", "вечерн", AdjStemType.Soft),
        new("a-rannii", "ранний", "ранн", AdjStemType.Soft),
        new("a-dalnii", "дальний", "дальн", AdjStemType.Soft),
    };

    // ————— Глаголы: настоящее время, вид несовершенный —————
    public static readonly VerbLexeme[] Verbs =
    {
        // I спряжение
        new("v-chitat", "читать", Conjugation.First, false,
            "читаю", "читаешь", "читает", "читаем", "читаете", "читают", "книгу"),
        new("v-gulyat", "гулять", Conjugation.First, false,
            "гуляю", "гуляешь", "гуляет", "гуляем", "гуляете", "гуляют", "в парке"),
        new("v-risovat", "рисовать", Conjugation.First, false,
            "рисую", "рисуешь", "рисует", "рисуем", "рисуете", "рисуют", "картину"),
        new("v-slushat", "слушать", Conjugation.First, false,
            "слушаю", "слушаешь", "слушает", "слушаем", "слушаете", "слушают", "музыку"),
        new("v-igrat", "играть", Conjugation.First, false,
            "играю", "играешь", "играет", "играем", "играете", "играют", "в футбол"),
        new("v-polivat", "поливать", Conjugation.First, false,
            "поливаю", "поливаешь", "поливает", "поливаем", "поливаете", "поливают", "цветы"),
        new("v-ubirat", "убирать", Conjugation.First, false,
            "убираю", "убираешь", "убирает", "убираем", "убираете", "убирают", "комнату"),
        new("v-myt", "мыть", Conjugation.First, false,
            "мою", "моешь", "моет", "моем", "моете", "моют", "посуду"),
        new("v-pisat", "писать", Conjugation.First, false,
            "пишу", "пишешь", "пишет", "пишем", "пишете", "пишут", "письмо"),
        new("v-tancevat", "танцевать", Conjugation.First, false,
            "танцую", "танцуешь", "танцует", "танцуем", "танцуете", "танцуют", "вальс"),
        new("v-obeshat", "обещать", Conjugation.First, false,
            "обещаю", "обещаешь", "обещает", "обещаем", "обещаете", "обещают", "помочь"),
        new("v-kopat", "копать", Conjugation.First, false,
            "копаю", "копаешь", "копает", "копаем", "копаете", "копают", "грядку"),
        // I спряжение — исключения (на -ить)
        new("v-brit", "брить", Conjugation.First, true,
            "брею", "бреешь", "бреет", "бреем", "бреете", "бреют", null),
        new("v-stelit", "стелить", Conjugation.First, true,
            "стелю", "стелешь", "стелет", "стелем", "стелете", "стелют", null),

        // II спряжение
        new("v-stroit", "строить", Conjugation.Second, false,
            "строю", "строишь", "строит", "строим", "строите", "строят", "дом"),
        new("v-lyubit", "любить", Conjugation.Second, false,
            "люблю", "любишь", "любит", "любим", "любите", "любят", "конфеты"),
        new("v-govorit", "говорить", Conjugation.Second, false,
            "говорю", "говоришь", "говорит", "говорим", "говорите", "говорят", "правду"),
        new("v-kormit", "кормить", Conjugation.Second, false,
            "кормлю", "кормишь", "кормит", "кормим", "кормите", "кормят", "кота"),
        new("v-sidet", "сидеть", Conjugation.Second, false,
            "сижу", "сидишь", "сидит", "сидим", "сидите", "сидят", "за партой"),
        new("v-hodit", "ходить", Conjugation.Second, false,
            "хожу", "ходишь", "ходит", "ходим", "ходите", "ходят", "в школу"),
        new("v-stoyat", "стоять", Conjugation.Second, false,
            "стою", "стоишь", "стоит", "стоим", "стоите", "стоят", "у доски"),
        new("v-druzhit", "дружить", Conjugation.Second, false,
            "дружу", "дружишь", "дружит", "дружим", "дружите", "дружат", "с соседом"),
        new("v-prosit", "просить", Conjugation.Second, false,
            "прошу", "просишь", "просит", "просим", "просите", "просят", "книгу"),
        new("v-varit", "варить", Conjugation.Second, false,
            "варю", "варишь", "варит", "варим", "варите", "варят", "суп"),
        new("v-gladit", "гладить", Conjugation.Second, false,
            "глажу", "гладишь", "гладит", "гладим", "гладите", "гладят", "кошку"),
        new("v-uchit", "учить", Conjugation.Second, false,
            "учу", "учишь", "учит", "учим", "учите", "учат", "стихотворение"),
        // II спряжение — исключения (на -ать/-еть)
        new("v-smotret", "смотреть", Conjugation.Second, true,
            "смотрю", "смотришь", "смотрит", "смотрим", "смотрите", "смотрят", "фильм"),
        new("v-videt", "видеть", Conjugation.Second, true,
            "вижу", "видишь", "видит", "видим", "видите", "видят", "сон"),
        new("v-slyshat", "слышать", Conjugation.Second, true,
            "слышу", "слышишь", "слышит", "слышим", "слышите", "слышат", "музыку"),
        new("v-derzhat", "держать", Conjugation.Second, true,
            "держу", "держишь", "держит", "держим", "держите", "держат", "пакет"),
        new("v-dyshat", "дышать", Conjugation.Second, true,
            "дышу", "дышишь", "дышит", "дышим", "дышите", "дышат", "воздухом"),
        new("v-terpet", "терпеть", Conjugation.Second, true,
            "терплю", "терпишь", "терпит", "терпим", "терпите", "терпят", null),
        new("v-zaviset", "зависеть", Conjugation.Second, true,
            "завишу", "зависишь", "зависит", "зависим", "зависите", "зависят", null),
    };

    // ————— Личные имена (формы из банка, не выводятся правилом) —————
    public static readonly string[] Names =
    {
        "Маша", "Оля", "Соня", "Катя", "Даша", "Лена", "Галя", "Вера",
        "Нина", "Миша", "Петя", "Никита", "Серёжа", "Юра", "Игорь", "Ира",
    };
}
