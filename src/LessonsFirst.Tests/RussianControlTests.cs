using System.Text.RegularExpressions;
using LessonsFirst.App.Tasks;
using LessonsFirst.App.Tasks.RussianTasks;
using Xunit;

namespace LessonsFirst.Tests;

/// <summary>
/// Независимо проверенные контрольные примеры всех 30 шаблонов и каждой ветки.
/// Таблицы ниже — «двойная бухгалтерия»: ожидаемые ответы записаны в тестах
/// вручную (по нормативным словарям и правилам) и сверяются с банком и генератором,
/// а не берутся из тех же данных, которыми пользуется продакшен-валидатор.
/// </summary>
public class RussianControlTests
{
    private static readonly AnswerPolicy LetterPolicy = new() { CaseSensitive = false };
    private static readonly AnswerPolicy WordPolicy = new() { CaseSensitive = false, YoDistinct = false, IgnoreStressMark = true };

    // ————— R12-01/02/03: подсчёты —————
    [Theory]
    [InlineData("конь", 4, 1, 1)]      // буквы, гласные, слоги
    [InlineData("машина", 6, 3, 3)]
    [InlineData("тетрадь", 7, 2, 2)]
    [InlineData("велосипед", 9, 4, 4)]
    [InlineData("ёж", 2, 1, 1)]
    [InlineData("сковорода", 9, 4, 4)]
    [InlineData("карандаш", 8, 3, 3)]
    public void R12_Counts_Control(string word, int letters, int vowels, int syllables)
    {
        var bank = RussianOrphograms.SimpleWords.Single(w => w.Text == word);
        Assert.Equal(letters, RussianTextUtil.CountLetters(word));
        Assert.Equal(vowels, RussianTextUtil.CountVowels(word));
        Assert.Equal(syllables, bank.Syllables);
    }

    [Fact]
    public void R12_04_AlphabetOrder_Control()
    {
        // ё стоит после е; сравнение — по явному алфавиту.
        Assert.True(RussianTextUtil.AlphabetIndex('а') < RussianTextUtil.AlphabetIndex('ё'));
        Assert.True(RussianTextUtil.AlphabetIndex('ё') < RussianTextUtil.AlphabetIndex('ж'));

        // «дом, аист, кот, мяч» → аист; «ёж, жук» → ёж; «дом, ёж» → дом.
        Assert.Equal("аист", new[] { "дом", "аист", "кот", "мяч" }
            .OrderBy(w => RussianTextUtil.AlphabetIndex(w[0])).First());
        Assert.Equal("ёж", new[] { "ёж", "жук" }.MinBy(w => RussianTextUtil.AlphabetIndex(w[0])));
        Assert.Equal("дом", new[] { "ёж", "дом" }.MinBy(w => RussianTextUtil.AlphabetIndex(w[0])));
    }

    // ————— R12-05/06/07: правила ЖИ-ШИ, ЧА-ЩА, ЧУ-ЩУ —————
    [Theory]
    [InlineData("жираф", 1, 'и')]
    [InlineData("машина", 3, 'и')]
    [InlineData("живот", 1, 'и')]
    [InlineData("ежи", 2, 'и')]
    public void R12_05_ZhiShi_Control(string word, int maskIndex, char letter)
    {
        var entry = RussianOrphograms.ZhiShi.Single(e => e.Word == word);
        Assert.Equal(maskIndex, entry.MaskIndex);
        Assert.Equal(letter, entry.Letter);
        Assert.Equal(AnswerCheckResult.Correct,
            AnswerValidator.CheckShortText("и", new[] { entry.Letter.ToString() }, LetterPolicy, 1));
        Assert.Equal(AnswerCheckResult.Wrong,
            AnswerValidator.CheckShortText("ы", new[] { entry.Letter.ToString() }, LetterPolicy, 1));
    }

    [Theory]
    [InlineData("чашка", 'а')]
    [InlineData("роща", 'а')]
    [InlineData("свеча", 'а')]
    public void R12_06_ChaSha_Control(string word, char letter)
    {
        var entry = RussianOrphograms.ChaSha.Single(e => e.Word == word);
        Assert.Equal(letter, entry.Letter);
        Assert.Equal(AnswerCheckResult.Wrong,
            AnswerValidator.CheckShortText("я", new[] { entry.Letter.ToString() }, LetterPolicy, 1));
    }

    [Theory]
    [InlineData("чудо", 'у')]
    [InlineData("щука", 'у')]
    [InlineData("ищу", 'у')]
    public void R12_07_ChuShu_Control(string word, char letter)
    {
        var entry = RussianOrphograms.ChuShu.Single(e => e.Word == word);
        Assert.Equal(letter, entry.Letter);
        Assert.Equal(AnswerCheckResult.Wrong,
            AnswerValidator.CheckShortText("ю", new[] { entry.Letter.ToString() }, LetterPolicy, 1));
    }

    // ————— R12-08/09: заглавная буква —————
    [Fact]
    public void R12_08_CapitalLetter_CaseSensitive()
    {
        var frame = RussianFrames.CapSentences.Single(f => f.FrameId == "cap-kot");
        Assert.Equal("Кот спит", frame.Upper);
        Assert.Equal("кот спит", frame.Lower);
        // Регистр — часть ответа: выбор строчного варианта неверен.
        Assert.Equal(AnswerCheckResult.Wrong, AnswerValidator.CheckChoice("cap-low", "cap-up"));
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckChoice("cap-up", "cap-up"));
    }

    [Fact]
    public void R12_09_NameCapital_CaseSensitive()
    {
        Assert.Contains("Маша", RussianLexicon.Names);
        // Политика регистровых упражнений: сравнение с учётом регистра.
        var policy = new AnswerPolicy { CaseSensitive = true };
        Assert.Equal("маша", AnswerValidator.NormalizeRussian("Маша", policy with { CaseSensitive = false }));
        Assert.Equal("Маша", AnswerValidator.NormalizeRussian("Маша", policy));
    }

    // ————— R12-10: предмет / действие / признак —————
    [Theory]
    [InlineData("предмет", "мяч")]
    [InlineData("действие", "бежит")]
    [InlineData("признак", "зелёный")]
    public void R12_10_WordClasses_Control(string branch, string expected)
    {
        Assert.Contains(expected, branch switch
        {
            "предмет" => RussianFrames.ObjectNouns,
            "действие" => RussianFrames.ActionVerbs,
            _ => RussianFrames.QualityAdjectives,
        });
    }

    // ————— R34-01: безударная гласная —————
    [Theory]
    [InlineData("Вдали видны густые л_са", 'е', "лес")]
    [InlineData("На лугу зелёная тр_ва", 'а', "травы")]
    [InlineData("Дети пили холодную в_ду", 'о', "воды")]
    [InlineData("На поляне растут высокие с_сны", 'о', "сосны")]
    [InlineData("Наступила холодная з_ма", 'и', "зимы")]
    [InlineData("Осенью часто идут д_жди", 'о', "дождь")]
    public void R34_01_Unstressed_Control(string sentence, char letter, string checkWord)
    {
        var entry = RussianOrphograms.UnstressedVowels.Single(e => e.Sentence == sentence);
        Assert.Equal(letter, entry.Letter);
        Assert.Equal(checkWord, entry.CheckWord);
        // Заполнение пропуска восстанавливает проверенную словоформу.
        Assert.Equal(RussianOracles.RestoredGapTexts[sentence], sentence.Replace("_", letter.ToString()));
        Assert.Equal(AnswerCheckResult.Correct,
            AnswerValidator.CheckShortText(letter.ToString(), new[] { entry.Letter.ToString() }, LetterPolicy, 1));
    }

    [Theory]
    [InlineData("На поляне растёт высокий ду_", 'б', "дубы")]
    [InlineData("Повар взял острый но_", 'ж', "ножи")]
    [InlineData("На шее тёплый шар_", 'ф', "шарфы")]
    [InlineData("За рекой виднелся крутой бере_", 'г', "берега")]
    [InlineData("Мы поднялись на девятый эта_", 'ж', "этажи")]
    public void R34_02_Paired_Control(string sentence, char letter, string checkForm)
    {
        var entry = RussianOrphograms.PairedConsonants.Single(e => e.Sentence == sentence);
        Assert.Equal(letter, entry.Letter);
        Assert.Equal(checkForm, entry.CheckForm);
    }

    [Theory]
    [InlineData("Это мес_ный житель", 'т', "место")]
    [InlineData("В груди быстро стучит сер_це", 'д', "сердечный")]
    [InlineData("Небо освещает яркое со_нце", 'л', "солнечный")]
    [InlineData("Здра_ствуй, дедушка!", 'в', "здравие")]
    public void R34_03_Silent_Control(string sentence, char letter, string checkWord)
    {
        var entry = RussianOrphograms.SilentConsonants.Single(e => e.Sentence == sentence);
        Assert.Equal(letter, entry.Letter);
        Assert.Equal(checkWord, entry.CheckWord);
    }

    [Theory]
    [InlineData("Началась сильная в_юга", 'ь')]
    [InlineData("Мама сшила новое плат_е", 'ь')]
    [InlineData("Осенью с деревьев падают лист_я", 'ь')]
    [InlineData("Мы в_ехали во двор", 'ъ')]
    [InlineData("Мы прочитали об_явление", 'ъ')]
    public void R34_04_Separators_Control(string sentence, char letter)
    {
        var bank = letter == 'ь' ? RussianOrphograms.SoftSeparators : RussianOrphograms.HardSeparators;
        var entry = bank.Single(e => e.Sentence == sentence);
        Assert.Equal(letter, entry.Letter);
    }

    // ————— R34-05: части речи —————
    [Fact]
    public void R34_05_Pos_Control()
    {
        var s = RussianFrames.PosSentences.Single(x => x.FrameId == "ps-f1a");
        Assert.Equal(new[] { "Маленький", "щенок", "грызёт", "кость" }, s.Words);
        // Существительные: щенок и кость; прилагательное: маленький; глагол: грызёт.
        Assert.Equal(new[] { 1, 3 }, s.Words.Select((w, i) => (w, i)).Where(x => s.Pos[x.i] == PartOfSpeech.Noun).Select(x => x.i));
        Assert.Equal(new[] { 0 }, s.Words.Select((w, i) => (w, i)).Where(x => s.Pos[x.i] == PartOfSpeech.Adjective).Select(x => x.i));
        Assert.Equal(new[] { 2 }, s.Words.Select((w, i) => (w, i)).Where(x => s.Pos[x.i] == PartOfSpeech.Verb).Select(x => x.i));
    }

    // ————— R34-06/07/08: род, число, склонение —————
    [Theory]
    [InlineData("окно", Gender.Neuter)]
    [InlineData("конь", Gender.Masculine)]
    [InlineData("ночь", Gender.Feminine)]
    [InlineData("дедушка", Gender.Masculine)]
    [InlineData("площадь", Gender.Feminine)]
    public void R34_06_Gender_Control(string lemma, Gender gender)
    {
        Assert.Equal(gender, RussianFrames.Genders.Single(g => g.Lemma == lemma).Gender);
    }

    [Fact]
    public void R34_07_NumberInContext_Control()
    {
        // Изолированное «книги» не спрашивается: только в согласованном предложении.
        var entry = RussianFrames.NumberSentences.Single(n => n.FrameId == "ns-stol");
        Assert.Equal("На столе лежит книга", entry.SentenceSg);
        Assert.Equal("На столе лежат книги", entry.SentencePl);
        Assert.Equal("книга", entry.NounSg);
        Assert.Equal("книги", entry.NounPl);
    }

    [Theory]
    [InlineData("папа", 1)]
    [InlineData("дедушка", 1)]
    [InlineData("конь", 2)]
    [InlineData("окно", 2)]
    [InlineData("ночь", 3)]
    [InlineData("тетрадь", 3)]
    public void R34_08_Declension_Control(string lemma, int declension)
    {
        Assert.Equal(declension, RussianFrames.Declensions.Single(d => d.Lemma == lemma).Declension);
    }

    // ————— R34-09: падеж в контексте —————
    [Theory]
    [InlineData("Мы подошли к школе", "школе", CaseRu.Dative)]
    [InlineData("Мы долго любовались лесом", "лесом", CaseRu.Instrumental)]
    [InlineData("Мы рассказывали о музее", "музее", CaseRu.Prepositional)]
    [InlineData("Мы сфотографировали закат", "закат", CaseRu.Accusative)]
    [InlineData("Около дома растут цветы", "дома", CaseRu.Genitive)]
    public void R34_09_CaseInContext_Control(string sentence, string form, CaseRu c)
    {
        var noun = RussianLexicon.Nouns.Single(n => n.Form(c, NumberRu.Singular) == form);
        Assert.Contains(form, sentence);
        Assert.Contains(noun.LexemeId, RussianLexicon.Nouns.Select(n => n.LexemeId));
    }

    [Fact]
    public void R34_10_SubjectPredicate_Control()
    {
        var s = RussianFrames.RoleSentences.Single(r => r.FrameId == "rs-kot");
        Assert.Equal("кот", s.Words[s.SubjectIndex]);
        Assert.Equal("спит", s.Words[s.PredicateIndex]);
    }

    // ————— R56-01: падежные формы ————— (независимая таблица словоформ)
    [Theory]
    [InlineData("музей", NumberRu.Plural, CaseRu.Dative, "музеям")]
    [InlineData("конь", NumberRu.Singular, CaseRu.Instrumental, "конём")]
    [InlineData("дерево", NumberRu.Plural, CaseRu.Nominative, "деревья")]
    [InlineData("сестра", NumberRu.Plural, CaseRu.Dative, "сёстрам")]
    [InlineData("брат", NumberRu.Plural, CaseRu.Accusative, "братьев")]
    [InlineData("ночь", NumberRu.Singular, CaseRu.Instrumental, "ночью")]
    [InlineData("окно", NumberRu.Plural, CaseRu.Genitive, "окон")]
    public void R56_01_CaseForms_Control(string lemma, NumberRu number, CaseRu c, string expected)
    {
        var noun = RussianLexicon.Nouns.Single(n => n.Lemma == lemma);
        Assert.Equal(expected, noun.Form(c, number));
        // Нормативная вариативность печати ё принимается.
        Assert.Equal(AnswerCheckResult.Correct,
            AnswerValidator.CheckShortText(expected, new[] { expected, expected.Replace('ё', 'е') }, WordPolicy, 24));
    }

    [Fact]
    public void R56_01_YoVariants_AllAccepted()
    {
        var kon = RussianLexicon.Nouns.Single(n => n.Lemma == "конь");
        var form = kon.Form(CaseRu.Instrumental, NumberRu.Singular);
        Assert.Equal("конём", form);
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckShortText("конем", new[] { "конём", "конем" }, WordPolicy, 24));
        Assert.Equal(AnswerCheckResult.Correct, AnswerValidator.CheckShortText("КОНЁМ", new[] { "конём", "конем" }, WordPolicy, 24));
    }

    // ————— R56-02: согласование прилагательных —————
    [Fact]
    public void R56_02_AdjectiveAgreement_Control()
    {
        var cases = new (string Lemma, Gender Gender, NumberRu Number, CaseRu Case, string Expected)[]
        {
            ("осенний", Gender.Masculine, NumberRu.Singular, CaseRu.Instrumental, "осенним"),
            ("осенний", Gender.Masculine, NumberRu.Singular, CaseRu.Genitive, "осеннего"),
            ("синий", Gender.Feminine, NumberRu.Singular, CaseRu.Dative, "синей"),
            ("новый", Gender.Feminine, NumberRu.Singular, CaseRu.Accusative, "новую"),
            ("зелёный", Gender.Neuter, NumberRu.Plural, CaseRu.Nominative, "зелёные"),
            ("тёплый", Gender.Neuter, NumberRu.Singular, CaseRu.Prepositional, "тёплом"),
            ("красный", Gender.Masculine, NumberRu.Singular, CaseRu.Genitive, "красного"),
            ("ранний", Gender.Feminine, NumberRu.Singular, CaseRu.Instrumental, "ранней"),
            ("чистый", Gender.Neuter, NumberRu.Plural, CaseRu.Instrumental, "чистыми"),
            ("дальний", Gender.Masculine, NumberRu.Singular, CaseRu.Prepositional, "дальнем"),
        };

        foreach (var (lemma, gender, number, c, expected) in cases)
        {
            var adj = RussianLexicon.Adjectives.Single(a => a.Lemma == lemma);
            Assert.Equal(expected, adj.Accepted(gender, number, c, animate: false)[0]);
        }
    }

    [Fact]
    public void R56_02_FeminineInstrumentalBothVariants()
    {
        var adj = RussianLexicon.Adjectives.Single(a => a.Lemma == "зимний");
        var accepted = adj.Accepted(Gender.Feminine, NumberRu.Singular, CaseRu.Instrumental, animate: false);
        Assert.Contains("зимней", accepted);
        Assert.Contains("зимнею", accepted); // нормативный вариант -ею
    }

    // ————— R56-03: личные формы глагола —————
    [Theory]
    [InlineData("строить", 2, NumberRu.Singular, "строишь")]
    [InlineData("строить", 3, NumberRu.Plural, "строят")]
    [InlineData("читать", 1, NumberRu.Singular, "читаю")]
    [InlineData("любить", 3, NumberRu.Plural, "любят")]
    [InlineData("писать", 1, NumberRu.Singular, "пишу")]
    [InlineData("смотреть", 1, NumberRu.Plural, "смотрим")]
    [InlineData("держать", 2, NumberRu.Plural, "держите")]
    [InlineData("гладить", 1, NumberRu.Singular, "глажу")]
    public void R56_03_PersonalForms_Control(string infinitive, int person, NumberRu number, string expected)
    {
        var verb = RussianLexicon.Verbs.Single(v => v.Infinitive == infinitive);
        Assert.Equal(expected, verb.Present(person, number));
    }

    // ————— R56-04: тся/ться —————
    [Theory]
    [InlineData("Он хорошо учи", true)]
    [InlineData("Он хочет хорошо учи", false)]
    [InlineData("Пора ложи", false)]
    [InlineData("Вечером он рано ложи", true)]
    [InlineData("Зритель тихо сади", true)]
    public void R56_04_TsaTsya_Control(string before, bool isTsa)
    {
        var entry = RussianOrphograms.TsaWords.Single(e => e.Before == before);
        Assert.Equal(isTsa, entry.IsTsa);
    }

    // ————— R56-05: спряжение —————
    [Theory]
    [InlineData("читать", Conjugation.First)]
    [InlineData("строить", Conjugation.Second)]
    [InlineData("брить", Conjugation.First)]       // исключение I
    [InlineData("стелить", Conjugation.First)]     // исключение I
    [InlineData("держать", Conjugation.Second)]    // исключение II
    [InlineData("дышать", Conjugation.Second)]     // исключение II
    [InlineData("слышать", Conjugation.Second)]    // исключение II
    [InlineData("видеть", Conjugation.Second)]     // исключение II
    public void R56_05_Conjugation_Control(string infinitive, Conjugation conj)
    {
        Assert.Equal(conj, RussianLexicon.Verbs.Single(v => v.Infinitive == infinitive).Conj);
    }

    // ————— R56-06: з/с в приставке —————
    [Theory]
    [InlineData("бе_шумный", 'с')]
    [InlineData("бе_вредный", 'з')]
    [InlineData("ра_сказ", 'с')]
    [InlineData("ра_будить", 'з')]
    [InlineData("во_ход", 'с')]
    [InlineData("во_будить", 'з')]
    [InlineData("и_тратить", 'с')]
    [InlineData("и_гнать", 'з')]
    public void R56_06_PrefixZS_Control(string masked, char letter)
    {
        var entry = RussianOrphograms.PrefixZS.Single(e => e.Masked == masked);
        Assert.Equal(letter, entry.Letter);
        Assert.Equal(entry.FullWord, masked.Replace("_", letter.ToString()));
    }

    // ————— R56-07: н/нн —————
    [Theory]
    [InlineData("стекля_ый стакан", "нн")]
    [InlineData("песча_ый берег", "н")]
    [InlineData("кожа_ый мяч", "н")]
    [InlineData("деревя_ый дом", "нн")]
    [InlineData("серебря_ый браслет", "н")]
    [InlineData("весе_ий дождь", "нн")]
    public void R56_07_DoubleN_Control(string maskedPhrase, string fragment)
    {
        var entry = RussianOrphograms.DoubleN.Single(e => e.Phrase.EndsWith(maskedPhrase));
        Assert.Equal(fragment, entry.Fragment);
        // Фрагменты разной длины: «н» и «нн» — разные ответы.
        Assert.Equal(AnswerCheckResult.Wrong,
            AnswerValidator.CheckShortText(fragment == "н" ? "нн" : "н", new[] { fragment }, LetterPolicy, 2));
    }

    // ————— R56-08: однородные члены —————
    [Fact]
    public void R56_08_Homogeneous_Control()
    {
        // Без союза: две запятые; с одиночным «и» — одна, перед «и» не ставится.
        var engine = new RussianEngine(new Random(11), GradeBand.Grades56);
        var noConj = Enumerable.Range(0, 500).Select(_ => engine.Next())
            .First(t => t.TemplateId == "Russian.R56-08" && !t.Tokens!.Any(x => x.Text == "и"));
        Assert.Equal(2, noConj.ExpectedBoundaryIds!.Count);

        var withConj = Enumerable.Range(0, 500).Select(_ => engine.Next())
            .First(t => t.TemplateId == "Russian.R56-08" && t.Tokens!.Any(x => x.Text == "и"));
        Assert.Equal(1, withConj.ExpectedBoundaryIds!.Count);

        // Граница перед «и» не входит в ответ.
        var conjunctionIndex = withConj.Tokens!.Select((t, i) => (t, i)).Single(x => x.t.Text == "и").i;
        Assert.DoesNotContain($"b{conjunctionIndex - 1}", withConj.ExpectedBoundaryIds!);
    }

    // ————— R56-09: обращение —————
    [Fact]
    public void R56_09_Address_Control()
    {
        var engine = new RussianEngine(new Random(12), GradeBand.Grades56);
        var tasks = Enumerable.Range(0, 500).Select(_ => engine.Next())
            .Where(t => t.TemplateId == "Russian.R56-09").ToList();

        Assert.Contains(tasks, t => t.ExpectedBoundaryIds!.Single() == "b0");             // имя в начале
        Assert.Contains(tasks, t => t.ExpectedBoundaryIds!.Single() != "b0");             // имя в конце
        // Обращение размечено ролью address и не является подлежащим.
        Assert.All(tasks, t => Assert.Contains(t.Tokens!, x => x.Role == "address"));
        Assert.DoesNotContain(tasks, t => t.Tokens!.Any(x => x.Role == "address" && x.Pos == PartOfSpeech.Noun));
    }

    // ————— R56-10: набор признаков —————
    [Fact]
    public void R56_10_GrammarFeatures_Control()
    {
        var engine = new RussianEngine(new Random(13), GradeBand.Grades56);
        var task = Enumerable.Range(0, 500).Select(_ => engine.Next())
            .First(t => t.TemplateId == "Russian.R56-10");

        // Ровно четыре разных набора, один правильный.
        Assert.Equal(4, task.Options!.Select(o => o.Text).Distinct().Count());
        Assert.Equal(1, task.Options!.Count(o => o.Id == task.CorrectOptionId));

        var correct = task.Options!.Single(o => o.Id == task.CorrectOptionId).Text;
        Assert.Matches(new Regex("род, .* число, .* падеж"), correct);
    }

    // ————— Пропуски: заполнение восстанавливает выверенный текст —————
    [Fact]
    public void Gaps_FillingRestoresVerifiedText()
    {
        // Все банки, где текст с пропуском записан вручную и может разойтись с ответом.
        var gaps = new List<(string Template, string Masked, string Filled)>();
        foreach (var e in RussianOrphograms.UnstressedVowels)
        {
            gaps.Add(("R34-01", e.Sentence, e.Sentence.Replace("_", e.Letter.ToString())));
        }

        foreach (var e in RussianOrphograms.PairedConsonants)
        {
            gaps.Add(("R34-02", e.Sentence, e.Sentence.Replace("_", e.Letter.ToString())));
        }

        foreach (var e in RussianOrphograms.SilentConsonants)
        {
            gaps.Add(("R34-03", e.Sentence, e.Sentence.Replace("_", e.Letter.ToString())));
        }

        foreach (var e in RussianOrphograms.SoftSeparators.Concat(RussianOrphograms.HardSeparators))
        {
            gaps.Add(("R34-04", e.Sentence, e.Sentence.Replace("_", e.Letter.ToString())));
        }

        foreach (var e in RussianOrphograms.TsaWords)
        {
            gaps.Add(("R56-04", e.Before + "___" + e.After, e.Before + (e.IsTsa ? "тся" : "ться") + e.After));
        }

        foreach (var e in RussianOrphograms.PrefixZS)
        {
            gaps.Add(("R56-06", e.Masked, e.Masked.Replace("_", e.Letter.ToString())));
        }

        foreach (var e in RussianOrphograms.DoubleN)
        {
            gaps.Add(("R56-07", e.Phrase, e.Phrase.Replace("_", e.Fragment)));
        }

        // Расхождения собираются все сразу: одна запись — одна строка отчёта.
        var problems = new List<string>();
        foreach (var (template, masked, filled) in gaps)
        {
            if (!RussianOracles.RestoredGapTexts.TryGetValue(masked, out var verified))
            {
                problems.Add($"{template}: «{masked}» нет в оракуле");
            }
            else if (filled != verified)
            {
                problems.Add($"{template}: «{masked}» заполняется как «{filled}», должно быть «{verified}»");
            }
        }

        // В оракуле не осталось устаревших записей.
        problems.AddRange(RussianOracles.RestoredGapTexts.Keys
            .Except(gaps.Select(g => g.Masked))
            .Select(stale => $"оракул: «{stale}» — такой записи в банке нет"));

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine + Environment.NewLine, problems));
    }
}
