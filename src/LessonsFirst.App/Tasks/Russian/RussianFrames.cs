namespace LessonsFirst.App.Tasks.RussianTasks;

/// <summary>
/// Проверенные конструкции предложений. Слоты и служебные слова фиксированы;
/// требования к совместимости (часть речи, падеж, семантика) заданы данными,
/// произвольное смешивание слов исключено.
/// </summary>
public static class RussianFrames
{
    // ————— R12-08: заглавная буква в начале предложения —————
    public sealed record CapFrame(string FrameId, string Lower, string Upper);

    public static readonly CapFrame[] CapSentences =
    {
        new("cap-kot", "кот спит", "Кот спит"),
        new("cap-dom", "дом стоит", "Дом стоит"),
        new("cap-mama", "мама читает", "Мама читает"),
        new("cap-solnce", "солнце светит", "Солнце светит"),
        new("cap-dozhd", "дождь идёт", "Дождь идёт"),
        new("cap-ptica", "птица поёт", "Птица поёт"),
        new("cap-sobaka", "собака лает", "Собака лает"),
        new("cap-cvetok", "цветок растёт", "Цветок растёт"),
        new("cap-mashina", "машина едет", "Машина едет"),
        new("cap-malchik", "мальчик рисует", "Мальчик рисует"),
        new("cap-devochka", "девочка гуляет", "Девочка гуляет"),
        new("cap-ryba", "рыба плывёт", "Рыба плывёт"),
    };

    // ————— R12-09: заглавная буква в имени —————
    public sealed record NameFrame(string FrameId, string Before, string After);

    public static readonly NameFrame[] NameSentences =
    {
        new("nm-segodnya", "Сегодня ", " читает книгу"),
        new("nm-vecherom", "Вечером ", " рисует кота"),
        new("nm-utrom", "Утром ", " пьёт молоко"),
        new("nm-dnem", "Днём ", " ест яблоко"),
        new("nm-vprazdnik", "В праздник ", " подарил цветы"),
        new("nm-osenyu", "Осенью ", " пошёл в школу"),
    };

    // ————— R12-10: предмет / действие / признак —————
    // Однозначная знакомая лексика; омонимы («печь») и названия действий («бег») исключены.
    public static readonly string[] ObjectNouns =
    {
        "мяч", "дом", "стол", "книга", "яблоко", "машина", "цветок", "окно", "портфель", "лопата", "берёза", "кастрюля",
    };

    public static readonly string[] ActionVerbs =
    {
        "бежит", "летит", "прыгает", "плывёт", "ползёт", "мчится", "шагает", "скачет",
    };

    public static readonly string[] QualityAdjectives =
    {
        "зелёный", "весёлый", "большой", "красный", "сладкий", "тёплый", "быстрый", "мягкий", "гладкий", "крепкий",
    };

    // ————— R34-05: части речи в предложении —————
    // Слова с контекстной разметкой части речи; FrameId — структурная конструкция.
    public sealed record PosSentence(string FrameId, string[] Words, PartOfSpeech[] Pos);

    public static readonly PosSentence[] PosSentences =
    {
        // F1: прилагательное + сущ. (подлежащее) + глагол + сущ. (дополнение)
        new("ps-f1a", new[] { "Маленький", "щенок", "грызёт", "кость" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Noun }),
        new("ps-f1b", new[] { "Рыжий", "кот", "лакает", "молоко" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Noun }),
        new("ps-f1c", new[] { "Зелёный", "попугай", "клюёт", "зерно" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Noun }),
        new("ps-f1d", new[] { "Весёлая", "девочка", "рисует", "дом" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Noun }),
        new("ps-f1e", new[] { "Любопытный", "мальчик", "читает", "журнал" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Noun }),
        new("ps-f1f", new[] { "Старая", "бабушка", "вяжет", "шарф" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Noun }),
        // F2: сущ. (подлежащее) + глагол + прилагательное + сущ. (дополнение)
        new("ps-f2a", new[] { "Мама", "читает", "интересную", "книгу" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Adjective, PartOfSpeech.Noun }),
        new("ps-f2b", new[] { "Папа", "чинит", "старый", "велосипед" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Adjective, PartOfSpeech.Noun }),
        new("ps-f2c", new[] { "Брат", "собирает", "деревянный", "конструктор" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Adjective, PartOfSpeech.Noun }),
        new("ps-f2d", new[] { "Сестра", "поливает", "комнатное", "растение" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Adjective, PartOfSpeech.Noun }),
        new("ps-f2e", new[] { "Дедушка", "рассказывает", "весёлую", "историю" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Adjective, PartOfSpeech.Noun }),
        // F3: прилагательное + сущ. (подлежащее) + глагол
        new("ps-f3a", new[] { "Голодный", "птенец", "пищит" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb }),
        new("ps-f3b", new[] { "Бурый", "медведь", "рычит" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb }),
        new("ps-f3c", new[] { "Пушистый", "цыплёнок", "пищит" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb }),
        // F4: сущ. (подлежащее) + глагол + сущ. (дополнение)
        new("ps-f4a", new[] { "Бабушка", "вяжет", "шарф" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Noun }),
        new("ps-f4b", new[] { "Девочка", "пьёт", "сок" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Noun }),
        new("ps-f4c", new[] { "Мальчик", "ест", "яблоко" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Noun }),
        new("ps-f4d", new[] { "Учитель", "проверяет", "тетради" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Noun }),
    };

    // ————— R34-06: род существительного —————
    public sealed record GenderEntry(string LexemeRef, string Lemma, Gender Gender);

    public static readonly GenderEntry[] Genders =
    {
        new("g-okno", "окно", Gender.Neuter),
        new("g-kon", "конь", Gender.Masculine),
        new("g-noch", "ночь", Gender.Feminine),
        new("g-papa", "папа", Gender.Masculine),
        new("g-tetrad", "тетрадь", Gender.Feminine),
        new("g-solnce", "солнце", Gender.Neuter),
        new("g-morkov", "морковь", Gender.Feminine),
        new("g-ovosh", "овощ", Gender.Masculine),
        new("g-mebel", "мебель", Gender.Feminine),
        new("g-more", "море", Gender.Neuter),
        new("g-dedushka", "дедушка", Gender.Masculine),
        new("g-derevnya", "деревня", Gender.Feminine),
        new("g-stakan", "стакан", Gender.Masculine),
        new("g-ploshad", "площадь", Gender.Feminine),
        new("g-ogon", "огонь", Gender.Masculine),
        new("g-platie", "платье", Gender.Neuter),
    };

    // ————— R34-07: число существительного в контексте —————
    // Согласованный глагол исключает другое прочтение изолированной формы.
    public sealed record NumberSentence(string FrameId, string SentenceSg, string SentencePl, string NounSg, string NounPl);

    public static readonly NumberSentence[] NumberSentences =
    {
        new("ns-stol", "На столе лежит книга", "На столе лежат книги", "книга", "книги"),
        new("ns-sad", "В саду растёт яблоня", "В саду растут яблони", "яблоня", "яблони"),
        new("ns-samolet", "По небу летит самолёт", "По небу летят самолёты", "самолёт", "самолёты"),
        new("ns-dvor", "Во дворе стоит машина", "Во дворе стоят машины", "машина", "машины"),
        new("ns-vaza", "В вазе стоит цветок", "В вазе стоят цветы", "цветок", "цветы"),
        new("ns-penal", "В пенале лежит карандаш", "В пенале лежат карандаши", "карандаш", "карандаши"),
        new("ns-vetka", "На ветке сидит птица", "На ветке сидят птицы", "птица", "птицы"),
        new("ns-akvarium", "В аквариуме плавает рыбка", "В аквариуме плавают рыбки", "рыбка", "рыбки"),
        new("ns-nebo", "На небе светит звезда", "На небе светят звёзды", "звезда", "звёзды"),
        new("ns-korzina", "В корзине лежит яблоко", "В корзине лежат яблоки", "яблоко", "яблоки"),
        new("ns-dvor2", "Во дворе растёт дерево", "Во дворе растут деревья", "дерево", "деревья"),
        new("ns-komnata", "В комнате горит лампа", "В комнате горят лампы", "лампа", "лампы"),
    };

    // ————— R34-08: склонение существительного —————
    public sealed record DeclensionEntry(string LexemeRef, string Lemma, int Declension);

    public static readonly DeclensionEntry[] Declensions =
    {
        new("d-papa", "папа", 1),
        new("d-mama", "мама", 1),
        new("d-devochka", "девочка", 1),
        new("d-dedushka", "дедушка", 1),
        new("d-derevnya", "деревня", 1),
        new("d-shkola", "школа", 1),
        new("d-kon", "конь", 2),
        new("d-okno", "окно", 2),
        new("d-more", "море", 2),
        new("d-solnce", "солнце", 2),
        new("d-stakan", "стакан", 2),
        new("d-noch", "ночь", 3),
        new("d-tetrad", "тетрадь", 3),
        new("d-mysh", "мышь", 3),
        new("d-mebel", "мебель", 3),
        new("d-ploshad", "площадь", 3),
    };

    // ————— R34-10 / R56-01 / R56-02 / R56-10: конструкции с управлением —————
    // Падеж задаётся конструкцией однозначно; семантические классы ограничивают слоты.
    public sealed record GovFrame(
        string FrameId,
        string Prefix,
        string Suffix,
        CaseRu Case,
        string[] SemClasses,
        bool PluralOnly = false,
        bool SingularOnly = false);

    public static readonly GovFrame[] GovernmentFrames =
    {
        new("gf-podoshli", "Мы подошли к ", "", CaseRu.Dative, new[] { "place", "plant", "nature", "person", "animal" }),
        new("gf-okolo", "Около ", " растут цветы", CaseRu.Genitive, new[] { "place", "nature", "object", "plant" }),
        new("gf-foto", "Мы сфотографировали ", "", CaseRu.Accusative, new[] { "place", "nature", "object", "plant" }),
        new("gf-lyubuemsya", "Мы долго любовались ", "", CaseRu.Instrumental, new[] { "nature", "object", "place", "plant" }),
        new("gf-govorili", "Мы рассказывали о ", "", CaseRu.Prepositional, new[] { "place", "nature", "object", "plant", "person", "animal" }),
        new("gf-rastut", "В саду растут ", "", CaseRu.Nominative, new[] { "plant" }, PluralOnly: true),
    };

    // ————— R34-10: подлежащее или сказуемое (TokenSelection, одно слово) —————
    public sealed record RoleSentence(string FrameId, string[] Words, PartOfSpeech[] Pos, int SubjectIndex, int PredicateIndex);

    public static readonly RoleSentence[] RoleSentences =
    {
        new("rs-kot", new[] { "Маленький", "кот", "спит" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb }, 1, 2),
        new("rs-mama", new[] { "Мама", "сварила", "вкусный", "суп" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Adjective, PartOfSpeech.Noun }, 0, 1),
        new("rs-sobaka", new[] { "Собака", "лает" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb }, 0, 1),
        new("rs-devochka", new[] { "Маленькая", "девочка", "рисует" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb }, 1, 2),
        new("rs-poezd", new[] { "Быстрый", "поезд", "мчится" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb }, 1, 2),
        new("rs-ptica", new[] { "Птица", "свистит" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb }, 0, 1),
        new("rs-malchik", new[] { "Мальчик", "читает", "интересную", "книгу" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Adjective, PartOfSpeech.Noun }, 0, 1),
        new("rs-cvetok", new[] { "Красивый", "цветок", "распустился" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb }, 1, 2),
        new("rs-dozhd", new[] { "Тёплый", "дождь", "идёт" },
            new[] { PartOfSpeech.Adjective, PartOfSpeech.Noun, PartOfSpeech.Verb }, 1, 2),
        new("rs-ryba", new[] { "Рыба", "плывёт" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb }, 0, 1),
        new("rs-vecher", new[] { "Вечер", "наступил" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb }, 0, 1),
        new("rs-korova", new[] { "Бурёнка", "жуёт", "траву" },
            new[] { PartOfSpeech.Noun, PartOfSpeech.Verb, PartOfSpeech.Noun }, 0, 1),
    };

    // ————— R56-08: однородные члены —————
    public sealed record HomoSet(string SetId, string[] Nouns, string[] Categories);

    public static readonly HomoSet[] HomogeneousSets =
    {
        new("hs-frukty", new[] { "яблоки", "груши", "сливы" }, new[] { "food" }),
        new("hs-frukty2", new[] { "вишни", "сливы", "абрикосы" }, new[] { "food" }),
        new("hs-ovoshi", new[] { "огурцы", "помидоры", "кабачки" }, new[] { "food" }),
        new("hs-igrushki", new[] { "кубики", "машинки", "куклы" }, new[] { "toys" }),
        new("hs-shkolnoe", new[] { "книги", "тетради", "карандаши" }, new[] { "school", "toys" }),
        new("hs-mebel", new[] { "столы", "стулья", "шкафы" }, new[] { "furniture" }),
        new("hs-pisanina", new[] { "тетради", "ручки", "линейки" }, new[] { "school" }),
    };

    public sealed record HomoFrame(string FrameId, string Prefix, string VerbPl, string[] Categories);

    public static readonly HomoFrame[] HomoFrames =
    {
        new("hf-korzina", "В корзине ", "лежат", new[] { "food", "toys", "school" }),
        new("hf-komnata", "В комнате ", "стоят", new[] { "furniture" }),
        new("hf-ryukzak", "В рюкзаке ", "лежат", new[] { "school", "toys" }),
        new("hf-stol", "На столе ", "лежат", new[] { "food", "school", "toys" }),
    };

    // ————— R56-09: запятая при обращении —————
    public sealed record AddressFrame(string FrameId, string Imperative);

    public static readonly AddressFrame[] AddressFrames =
    {
        new("af-otkroy", "открой книгу"),
        new("af-vozmi", "возьми ручку"),
        new("af-prochitay", "прочитай сказку"),
        new("af-narisuy", "нарисуй кота"),
        new("af-zakroy", "закрой дверь"),
        new("af-syesh", "съешь яблоко"),
        new("af-vypey", "выпей молоко"),
        new("af-naden", "надень шапку"),
        new("af-vychi", "выучи стихотворение"),
        new("af-polozhi", "положи книгу"),
        new("af-vytri", "вытри пыль"),
        new("af-posmotri", "посмотри фильм"),
    };
}
