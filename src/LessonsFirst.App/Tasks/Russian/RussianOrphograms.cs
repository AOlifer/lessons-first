namespace LessonsFirst.App.Tasks.RussianTasks;

/// <summary>
/// Орфограммный банк: проверенные слова и предложения с размеченной позицией
/// пропуска, правильным фрагментом, правилом и проверочным словом.
/// Каждая запись — выверенная языковая единица (см. контрольные примеры в тестах).
/// </summary>
public static class RussianOrphograms
{
    // ————— Простые слова для подсчёта букв/гласных/слогов и алфавита (1–2 класс) —————
    // Syllables — проверенное число слогов (совпадает с числом гласных).
    public sealed record SimpleWord(string WordId, string Text, int Syllables);

    public static readonly SimpleWord[] SimpleWords =
    {
        new("w-kot", "кот", 1), new("w-dom", "дом", 1), new("w-myach", "мяч", 1),
        new("w-kon", "конь", 1), new("w-stul", "стул", 1), new("w-stol", "стол", 1),
        new("w-mel", "мел", 1), new("w-shar", "шар", 1), new("w-les", "лес", 1),
        new("w-zhuk", "жук", 1), new("w-nos", "нос", 1), new("w-syr", "сыр", 1),
        new("w-yozh", "ёж", 1), new("w-flag", "флаг", 1), new("w-hleb", "хлеб", 1),
        new("w-chay", "чай", 1),
        new("w-aist", "аист", 2), new("w-ryba", "рыба", 2), new("w-lisa", "лиса", 2),
        new("w-mama", "мама", 2), new("w-koza", "коза", 2), new("w-luna", "луна", 2),
        new("w-roza", "роза", 2), new("w-okno", "окно", 2), new("w-utka", "утка", 2),
        new("w-igla", "игла", 2), new("w-etazh", "этаж", 2), new("w-yubka", "юбка", 2),
        new("w-koshka", "кошка", 2), new("w-mishka", "мишка", 2), new("w-vilka", "вилка", 2),
        new("w-marka", "марка", 2), new("w-parta", "парта", 2), new("w-shkola", "школа", 2),
        new("w-belka", "белка", 2), new("w-sumka", "сумка", 2), new("w-lampa", "лампа", 2),
        new("w-penal", "пенал", 2), new("w-divan", "диван", 2), new("w-topor", "топор", 2),
        new("w-zamok", "замок", 2), new("w-vagon", "вагон", 2), new("w-petuh", "петух", 2),
        new("w-garazh", "гараж", 2), new("w-cvetok", "цветок", 2), new("w-shenok", "щенок", 2),
        new("w-tetrad", "тетрадь", 2), new("w-platye", "платье", 2), new("w-pesok", "песок", 2),
        new("w-mashina", "машина", 3), new("w-vorona", "ворона", 3), new("w-doroga", "дорога", 3),
        new("w-soroka", "сорока", 3), new("w-sobaka", "собака", 3), new("w-korova", "корова", 3),
        new("w-moloko", "молоко", 3), new("w-ulica", "улица", 3), new("w-kurica", "курица", 3),
        new("w-malina", "малина", 3), new("w-yagoda", "ягода", 3), new("w-derevo", "дерево", 3),
        new("w-karandash", "карандаш", 3), new("w-uchebnik", "учебник", 3),
        new("w-yasherica", "ящерица", 4), new("w-televizor", "телевизор", 4),
        new("w-skovoroda", "сковорода", 4), new("w-velosiped", "велосипед", 4),
    };

    // ————— Правило ЖИ–ШИ: замаскированная гласная после ж/ш —————
    public sealed record MaskedWord(string EntryId, string Word, int MaskIndex, char Letter);

    public static readonly MaskedWord[] ZhiShi =
    {
        new("zhi-zhiraf", "жираф", 1, 'и'), new("zhi-mashina", "машина", 3, 'и'),
        new("zhi-ezhi", "ежи", 2, 'и'), new("zhi-lyzhi", "лыжи", 3, 'и'),
        new("zhi-myshi", "мыши", 3, 'и'), new("zhi-uzhi", "ужи", 2, 'и'),
        new("zhi-zhivot", "живот", 1, 'и'), new("zhi-shina", "шина", 1, 'и'),
        new("zhi-uzhin", "ужин", 2, 'и'), new("zhi-zhizn", "жизнь", 1, 'и'),
        new("zhi-shilo", "шило", 1, 'и'), new("zhi-zhir", "жир", 1, 'и'),
    };

    // ————— Правило ЧА–ЩА —————
    public static readonly MaskedWord[] ChaSha =
    {
        new("cha-chayka", "чайка", 1, 'а'), new("cha-chashka", "чашка", 1, 'а'),
        new("cha-dacha", "дача", 1, 'а'), new("cha-rosha", "роща", 3, 'а'),
        new("cha-tucha", "туча", 3, 'а'), new("cha-kucha", "куча", 3, 'а'),
        new("cha-gusha", "гуща", 3, 'а'), new("cha-chasha", "чаща", 1, 'а'),
        new("cha-shavel", "щавель", 1, 'а'), new("cha-svecha", "свеча", 4, 'а'),
        new("cha-chay", "чай", 1, 'а'), new("cha-chasy", "часы", 1, 'а'),
    };

    // ————— Правило ЧУ–ЩУ —————
    public static readonly MaskedWord[] ChuShu =
    {
        new("chu-chudo", "чудо", 1, 'у'), new("chu-shuka", "щука", 1, 'у'),
        new("chu-chulok", "чулок", 1, 'у'), new("chu-chugun", "чугун", 1, 'у'),
        new("chu-tashu", "тащу", 3, 'у'), new("chu-krichu", "кричу", 4, 'у'),
        new("chu-molchu", "молчу", 4, 'у'), new("chu-ishu", "ищу", 2, 'у'),
        new("chu-grushu", "грущу", 4, 'у'), new("chu-ugoshu", "угощу", 4, 'у'),
        new("chu-vorchu", "ворчу", 4, 'у'), new("chu-chuvstvo", "чувство", 1, 'у'),
    };

    // ————— Проверяемая безударная гласная в корне (3–4 класс) —————
    // Предложение с одним пропуском; заполнение восстанавливает проверенную словоформу.
    // CheckWordStressed — проверочное слово с ударением (U+0301) на проверяемой гласной.
    public sealed record UnstressedEntry(
        string EntryId, string Sentence, char Letter, string CheckWord, string CheckWordStressed);

    public static readonly UnstressedEntry[] UnstressedVowels =
    {
        new("uv-lisa-pl", "Вдали видны густые л_са", 'е', "лес", "ле́с"),
        new("uv-trava", "На лугу зелёная тр_ва", 'а', "травы", "тра́вы"),
        new("uv-voda", "Дети пили холодную в_ду", 'о', "воды", "во́ды"),
        new("uv-svezhiy", "Суп сварили из св_жих грибов", 'е', "свежий", "све́жий"),
        new("uv-sosny", "На поляне растут высокие с_сны", 'о', "сосны", "со́сны"),
        new("uv-gora", "Мы поднялись на высокую г_ру", 'о', "горы", "го́ры"),
        new("uv-zvezdy", "В темноте зажглись яркие зв_зды", 'е', "звёзды", "звёзды"),
        new("uv-zolotoy", "В аквариуме плавают з_лотые рыбки", 'о', "золото", "зо́лото"),
        new("uv-slova", "В тетради записаны новые сл_ва", 'о', "слово", "сло́во"),
        new("uv-glaza", "У ребёнка красивые гл_за", 'а', "глаз", "гла́з"),
        new("uv-stoly", "В классе стоят новые ст_лы", 'о', "стол", "сто́л"),
        new("uv-shchuka", "Рыбак поймал большую щ_ку", 'у', "щуки", "щу́ки"),
        new("uv-zima", "Наступила холодная з_ма", 'и', "зимы", "зи́мы"),
        new("uv-snegovik", "Во дворе дети лепят сн_говика", 'е', "снег", "сне́г"),
        new("uv-cvety", "На клумбе растут красивые цв_ты", 'е', "цвет", "цве́т"),
        new("uv-dozhdi", "Осенью часто идут д_жди", 'о', "дождь", "до́ждь"),
    };

    // ————— Парная согласная в конце слова (3–4 класс) —————
    public sealed record PairedEntry(string EntryId, string Sentence, char Letter, string CheckForm);

    public static readonly PairedEntry[] PairedConsonants =
    {
        new("pc-dub", "На поляне растёт высокий ду_", 'б', "дубы"),
        new("pc-grib", "Под деревом нашли старый гри_", 'б', "грибы"),
        new("pc-moroz", "Ночью ударил сильный моро_", 'з', "морозы"),
        new("pc-arbuz", "На столе лежит сладкий арбу_", 'з', "арбузы"),
        new("pc-hleb", "К обеду испекли мягкий хле_", 'б', "хлебы"),
        new("pc-nozh", "Повар взял острый но_", 'ж', "ножи"),
        new("pc-yorsh", "В реке живёт колючий ёр_", 'ш', "ерши"),
        new("pc-karandash", "В пенале лежит новый каранда_", 'ш', "карандаши"),
        new("pc-sharf", "На шее тёплый шар_", 'ф', "шарфы"),
        new("pc-klubok", "Кот свернулся в клубо_", 'к', "клубки"),
        new("pc-zvuk", "Вдалеке послышался странный зву_", 'к', "звуки"),
        new("pc-bereg", "За рекой виднелся крутой бере_", 'г', "берега"),
        new("pc-etazh", "Мы поднялись на девятый эта_", 'ж', "этажи"),
        new("pc-ogorod", "Летом мы поливали огоро_", 'д', "огороды"),
    };

    // ————— Непроизносимая согласная (3–4 класс) —————
    public sealed record SilentEntry(string EntryId, string Sentence, char Letter, string CheckWord);

    public static readonly SilentEntry[] SilentConsonants =
    {
        new("sc-mestnyy", "Это мес_ный житель", 'т', "место"),
        new("sc-serdce", "В груди быстро стучит сер_це", 'д', "сердечный"),
        new("sc-solnce", "Небо освещает яркое со_нце", 'л', "солнечный"),
        new("sc-grustnyy", "Вчера был очень грус_ный день", 'т', "грусть"),
        new("sc-zvezdnoe", "Ночью мы наблюдали звёз_ное небо", 'д', "звёзды"),
        new("sc-chestnyy", "Он дал чес_ное слово", 'т', "честь"),
        new("sc-schastlivyy", "У ребёнка счас_ливая улыбка", 'т', "счастье"),
        new("sc-radostnyy", "На лицах гостей радос_ные улыбки", 'т', "радость"),
        new("sc-nenastnyy", "Осенью часто ненас_ная погода", 'т', "ненастье"),
        new("sc-kapustnyy", "Мама приготовила капус_ный салат", 'т', "капуста"),
        new("sc-yarostnyy", "Ветер был ярос_ным", 'т', "ярость"),
        new("sc-pozdno", "Мы вернулись домой поз_но", 'д', "поздний"),
        new("sc-mestnost", "Мы гуляли по живописной мес_ности", 'т', "место"),
        new("sc-zdravstvuy", "Здра_ствуй, дедушка!", 'в', "здравие"),
    };

    // ————— Разделительные Ь и Ъ (3–4 класс) —————
    public sealed record SeparatorEntry(string EntryId, string Sentence, char Letter);

    public static readonly SeparatorEntry[] SoftSeparators =
    {
        new("sp-vyuga", "Началась сильная в_юга", 'ь'),
        new("sp-platie", "Мама сшила новое плат_е", 'ь'),
        new("sp-pechene", "Дети пили горячее печен_е", 'ь'),
        new("sp-listya", "Осенью с деревьев падают лист_я", 'ь'),
        new("sp-semya", "Большая сем_я собралась дома", 'ь'),
        new("sp-ruchi", "С горы бегут весенние руч_и", 'ь'),
    };

    public static readonly SeparatorEntry[] HardSeparators =
    {
        new("hp-vezd", "Мы в_ехали во двор", 'ъ'),
        new("hp-vezd2", "В_езд на мост закрыт", 'ъ'),
        new("hp-obyavlenie", "Мы прочитали об_явление", 'ъ'),
        new("hp-podezd", "Мы живём в третьем под_езде", 'ъ'),
        new("hp-seemka", "С_ёмка фильма началась", 'ъ'),
        new("hp-obyasnit", "Учитель об_яснил задачу", 'ъ'),
    };

    // ————— З/С в приставке (5–6 класс): без-/бес-, раз-/рас-, из-/ис-, воз-/вос- —————
    public sealed record PrefixEntry(string EntryId, string Masked, char Letter, string FullWord);

    public static readonly PrefixEntry[] PrefixZS =
    {
        new("pz-besshumniy", "бе_шумный", 'с', "бесшумный"),
        new("pz-bezvrednyy", "бе_вредный", 'з', "безвредный"),
        new("pz-bezdomnyy", "бе_домный", 'з', "бездомный"),
        new("pz-bezzhalostnyy", "бе_жалостный", 'з', "безжалостный"),
        new("pz-bessmyslennyy", "бе_смысленный", 'с', "бессмысленный"),
        new("pz-bezradostnyy", "бе_радостный", 'з', "безрадостный"),
        new("pz-rasskaz", "ра_сказ", 'с', "рассказ"),
        new("pz-razbudit", "ра_будить", 'з', "разбудить"),
        new("pz-rassvet", "ра_свет", 'с', "рассвет"),
        new("pz-razgadat", "ра_гадать", 'з', "разгадать"),
        new("pz-rasstavit", "ра_ставить", 'с', "расставить"),
        new("pz-istratit", "и_тратить", 'с', "истратить"),
        new("pz-izdanie", "и_дание", 'з', "издание"),
        new("pz-isgnat", "и_гнать", 'з', "изгнать"),
        new("pz-ispugat", "и_пугать", 'с', "испугать"),
        new("pz-voshod", "во_ход", 'с', "восход"),
        new("pz-vozrast", "во_раст", 'з', "возраст"),
        new("pz-vozglavit", "во_главить", 'з', "возглавить"),
        new("pz-vozbudit", "во_будить", 'з', "возбудить"),
    };

    // ————— Н/НН в полных прилагательных, образованных от существительных (5–6 класс) —————
    public sealed record DoubleNEntry(string EntryId, string Phrase, string Fragment, string Rule);

    public static readonly DoubleNEntry[] DoubleN =
    {
        new("nn-steklyannyy", "Вставь н или нн: стекля_ый стакан", "нн", "суффикс -янн- (стекло)"),
        new("nn-derevyannyy", "Вставь н или нн: деревя_ый дом", "нн", "суффикс -янн- (дерево)"),
        new("nn-olovyannyy", "Вставь н или нн: оловя_ая ложка", "нн", "суффикс -янн- (олово)"),
        new("nn-solomennyy", "Вставь н или нн: соломе_ая шляпа", "нн", "суффикс -енн-"),
        new("nn-kamennyy", "Вставь н или нн: каме_ая стена", "нн", "суффикс -енн- (камень)"),
        new("nn-tumannyy", "Вставь н или нн: тума_ое утро", "нн", "основа на нн (туман)"),
        new("nn-sonnyy", "Вставь н или нн: со_ый ребёнок", "нн", "основа на нн (сон)"),
        new("nn-vesenniy", "Вставь н или нн: весе_ий дождь", "нн", "суффикс -нн- (весна)"),
        new("nn-osenniy", "Вставь н или нн: осе_ий лес", "нн", "суффикс -нн- (осень)"),
        new("nn-plamennyy", "Вставь н или нн: пламе_ая речь", "нн", "суффикс -енн-"),
        new("nn-kozhanyy", "Вставь н или нн: кожа_ый мяч", "н", "суффикс -ан- (кожа)"),
        new("nn-peschanyy", "Вставь н или нн: песча_ый берег", "н", "суффикс -ан- (песок)"),
        new("nn-serebryanyy", "Вставь н или нн: серебря_ый браслет", "н", "суффикс -ян- (серебро)"),
        new("nn-glinyanyy", "Вставь н или нн: глиня_ый кувшин", "н", "суффикс -ян- (глина)"),
        new("nn-lnyanoy", "Вставь н или нн: льня_ое полотенце", "н", "суффикс -ян- (лён)"),
    };

    // ————— -ТСЯ/-ТЬСЯ (5–6 класс): однозначный контекст —————
    // Prompt = Before + «…» + After. IsTsa: вопрос «что делает?» → тся; иначе «что делать?» → ться.
    // Before/After выверены так, что замаскирован ровно фрагмент тся/ться.
    public sealed record TsaEntry(string EntryId, string Before, string After, bool IsTsa, string FullWord);

    public static readonly TsaEntry[] TsaWords =
    {
        // 3-е лицо настоящего времени → тся (что делает?)
        new("ts-uchitsya", "Он хорошо учи", "", true, "учится"),
        new("ts-ulybaetsya", "Она всегда улыбае", "", true, "улыбается"),
        new("ts-smeyutsya", "Они весело смею", "", true, "смеются"),
        new("ts-zanimayetsya", "Брат давно занима" + "е", "", true, "занимается"),
        new("ts-kupayetsya", "Утром он купае", " в реке", true, "купается"),
        new("ts-odevaetsya", "Малыш сам одевае", "", true, "одевается"),
        new("ts-umyvaetsya", "Перед сном ребёнок умыва" + "е", "", true, "умывается"),
        new("ts-kataetsya", "Весь день он ката" + "е", " на коньках", true, "катается"),
        new("ts-lozhitsya", "Вечером он рано ложи", " спать", true, "ложится"),
        new("ts-saditsya", "Зритель тихо сади", " в зале", true, "садится"),
        new("ts-toropitsya", "Автобус скоро уходит, он торопи", "", true, "торопится"),
        // инфинитив после модального слова → ться (что делать?)
        new("ts-uchitsya-inf", "Он хочет хорошо учи", "", false, "учиться"),
        new("ts-katatsya-inf", "Она хочет ката", " на лыжах", false, "кататься"),
        new("ts-kupatsya-inf", "Он любит купа", " в реке", false, "купаться"),
        new("ts-zanimatsya-inf", "Мы начали занима", " спортом", false, "заниматься"),
        new("ts-toropitsya-inf", "Нельзя торопи", " на уроке", false, "торопиться"),
        new("ts-odevatsya-inf", "Она умеет красиво одева", "", false, "одеваться"),
        new("ts-lozhitsya-inf", "Пора ложи", " спать", false, "ложиться"),
        new("ts-umytsya-inf", "Не забудь умы", " перед сном", false, "умыться"),
        new("ts-ulybatsya-inf", "Он старается чаще улыб" + "а", "", false, "улыбаться"),
    };
}
