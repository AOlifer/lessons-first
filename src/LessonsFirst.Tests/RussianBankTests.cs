using LessonsFirst.App.Tasks;
using LessonsFirst.App.Tasks.RussianTasks;
using Xunit;
using Xunit.Abstractions;

namespace LessonsFirst.Tests;

/// <summary>
/// Целостность русского банка: ссылки лексем/форм/орфограмм/конструкций,
/// связность орфограмм с проверочными словами, полнота парадигм
/// и минимальное покрытие каждого шаблона/ветки с отчётом.
/// </summary>
public class RussianBankTests
{
    private readonly ITestOutputHelper _output;

    public RussianBankTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Nouns_UniqueIds_NonEmptyParadigms()
    {
        Assert.Equal(RussianLexicon.Nouns.Length, RussianLexicon.Nouns.Select(n => n.LexemeId).Distinct().Count());
        foreach (var noun in RussianLexicon.Nouns)
        {
            for (var n = 0; n < 2; n++)
            {
                foreach (CaseRu c in Enum.GetValues<CaseRu>())
                {
                    var form = noun.Form(c, (NumberRu)n);
                    Assert.False(string.IsNullOrWhiteSpace(form), $"{noun.LexemeId}: пустая форма {c}/{(NumberRu)n}");
                    Assert.True(form.All(char.IsLetter), $"{noun.LexemeId}: форма «{form}» содержит не-буквы");
                }
            }
        }
    }

    [Fact]
    public void Adjectives_UniqueIds_HaveBothStemTypes()
    {
        Assert.Equal(RussianLexicon.Adjectives.Length, RussianLexicon.Adjectives.Select(a => a.LexemeId).Distinct().Count());
        Assert.Contains(RussianLexicon.Adjectives, a => a.Type == AdjStemType.Hard);
        Assert.Contains(RussianLexicon.Adjectives, a => a.Type == AdjStemType.Soft);
    }

    [Fact]
    public void Verbs_UniqueIds_ImperfectivePresentForms()
    {
        Assert.Equal(RussianLexicon.Verbs.Length, RussianLexicon.Verbs.Select(v => v.LexemeId).Distinct().Count());
        foreach (var verb in RussianLexicon.Verbs)
        {
            foreach (var (person, number) in new[] { (1, NumberRu.Singular), (2, NumberRu.Singular), (3, NumberRu.Singular), (1, NumberRu.Plural), (2, NumberRu.Plural), (3, NumberRu.Plural) })
            {
                Assert.False(string.IsNullOrWhiteSpace(verb.Present(person, number)),
                    $"{verb.LexemeId}: пустая форма {person}/{number}");
            }
        }
    }

    [Fact]
    public void SimpleWords_SyllablesEqualVowels_LetterBounds()
    {
        foreach (var word in RussianOrphograms.SimpleWords)
        {
            Assert.InRange(RussianTextUtil.CountLetters(word.Text), 2, 10);
            Assert.InRange(word.Syllables, 1, 4);
            Assert.Equal(RussianTextUtil.CountVowels(word.Text), word.Syllables);
            Assert.True(word.Text.All(AnswerValidator.IsCyrillicLetter));
        }

        Assert.Equal(RussianOrphograms.SimpleWords.Length, RussianOrphograms.SimpleWords.Select(w => w.WordId).Distinct().Count());
    }

    [Fact]
    public void Alphabet_Explicit33Letters_YoAfterYe()
    {
        Assert.Equal(33, RussianTextUtil.Alphabet.Length);
        Assert.Equal(RussianTextUtil.Alphabet.Distinct().Count(), 33);
        Assert.True(RussianTextUtil.Alphabet.IndexOf('е') < RussianTextUtil.Alphabet.IndexOf('ё'));
        Assert.True(RussianTextUtil.Alphabet.IndexOf('ё') < RussianTextUtil.Alphabet.IndexOf('ж'));
        // Сортировка, объединяющая е/ё, запрещена: индексы должны различаться.
        Assert.NotEqual(RussianTextUtil.AlphabetIndex('е'), RussianTextUtil.AlphabetIndex('ё'));
    }

    [Theory]
    [InlineData(nameof(RussianOrphograms.ZhiShi), "и")]
    [InlineData(nameof(RussianOrphograms.ChaSha), "а")]
    [InlineData(nameof(RussianOrphograms.ChuShu), "у")]
    public void MaskedBanks_LetterIsAtMaskedPosition(string bankName, string expectedLetter)
    {
        var bank = bankName switch
        {
            nameof(RussianOrphograms.ZhiShi) => RussianOrphograms.ZhiShi,
            nameof(RussianOrphograms.ChaSha) => RussianOrphograms.ChaSha,
            _ => RussianOrphograms.ChuShu,
        };

        Assert.All(bank, entry =>
        {
            Assert.InRange(entry.MaskIndex, 0, entry.Word.Length - 1);
            Assert.Equal(expectedLetter[0], entry.Word[entry.MaskIndex]);
            var masked = RussianTextUtil.MaskedWord(entry.Word, entry.MaskIndex);
            Assert.Contains('_', masked);
            Assert.Equal(entry.Word.Length, masked.Length);
        });
    }

    [Fact]
    public void UnstressedVowels_SingleMask_CheckWordCarriesStressedLetter()
    {
        foreach (var entry in RussianOrphograms.UnstressedVowels)
        {
            Assert.Equal(1, entry.Sentence.Count(c => c == '_'));
            // Ударная гласная проверочного слова: U+0301 после буквы или буква ё
            // (ё — ударное е). Ударная гласная совпадает с ответом орфограммы.
            var stressIndex = entry.CheckWordStressed.IndexOf('\u0301');
            char stressedVowel;
            if (stressIndex > 0)
            {
                stressedVowel = entry.CheckWordStressed[stressIndex - 1];
            }
            else
            {
                var yoIndex = entry.CheckWordStressed.IndexOf('ё');
                Assert.True(yoIndex >= 0, $"{entry.EntryId}: нет ударения в {entry.CheckWordStressed}");
                stressedVowel = 'е'; // ё = ударное е
            }

            var normalized = stressedVowel == 'ё' ? 'е' : char.ToLowerInvariant(stressedVowel);
            Assert.Equal(entry.Letter, normalized);
        }
    }

    [Fact]
    public void SentenceBanks_SingleMask()
    {
        Assert.All(RussianOrphograms.PairedConsonants, e =>
        {
            Assert.Equal(1, e.Sentence.Count(c => c == '_'));
            Assert.True(e.Sentence.TrimEnd().EndsWith('_'), "парная согласная — в конце слова");
        });
        Assert.All(RussianOrphograms.SilentConsonants, e => Assert.Equal(1, e.Sentence.Count(c => c == '_')));
        Assert.All(RussianOrphograms.SoftSeparators.Concat(RussianOrphograms.HardSeparators), e =>
        {
            Assert.Equal(1, e.Sentence.Count(c => c == '_'));
            Assert.Contains(e.Letter, new[] { 'ь', 'ъ' });
        });
    }

    [Fact]
    public void PrefixZS_MaskReconstructsFullWord()
    {
        foreach (var entry in RussianOrphograms.PrefixZS)
        {
            Assert.Equal(1, entry.Masked.Count(c => c == '_'));
            Assert.Equal(entry.FullWord, entry.Masked.Replace("_", entry.Letter.ToString()));
            Assert.Contains(entry.Letter, new[] { 'з', 'с' });
        }
    }

    [Fact]
    public void DoubleN_FragmentIsNOrNN()
    {
        foreach (var entry in RussianOrphograms.DoubleN)
        {
            Assert.Contains(entry.Fragment, new[] { "н", "нн" });
            Assert.Equal(1, entry.Phrase.Count(c => c == '_'));
            Assert.Contains("Вставь н или нн:", entry.Phrase);
        }
    }

    [Fact]
    public void TsaWords_TailReconstructsFullWord()
    {
        foreach (var entry in RussianOrphograms.TsaWords)
        {
            var tail = entry.IsTsa ? "тся" : "ться";
            Assert.Contains(entry.FullWord, entry.Before + tail);
        }

        // Обе ветки заполнены.
        Assert.True(RussianOrphograms.TsaWords.Count(e => e.IsTsa) >= 6);
        Assert.True(RussianOrphograms.TsaWords.Count(e => !e.IsTsa) >= 6);
    }

    [Fact]
    public void Frames_PosLengthsMatch_RolesConsistent()
    {
        foreach (var sentence in RussianFrames.PosSentences)
        {
            Assert.Equal(sentence.Words.Length, sentence.Pos.Length);
            Assert.Contains(PartOfSpeech.Noun, sentence.Pos);
            Assert.Contains(PartOfSpeech.Verb, sentence.Pos);
        }

        foreach (var sentence in RussianFrames.RoleSentences)
        {
            Assert.Equal(sentence.Words.Length, sentence.Pos.Length);
            Assert.Equal(PartOfSpeech.Noun, sentence.Pos[sentence.SubjectIndex]);
            Assert.Equal(PartOfSpeech.Verb, sentence.Pos[sentence.PredicateIndex]);
        }

        // Каждая конструкция управления совместима минимум с двумя существительными.
        foreach (var frame in RussianFrames.GovernmentFrames)
        {
            var compatible = RussianLexicon.Nouns.Count(n => frame.SemClasses.Contains(n.SemClass));
            Assert.True(compatible >= 2, $"{frame.FrameId}: совместимых существительных {compatible}");
        }

        Assert.All(RussianFrames.AddressFrames, f => Assert.Equal(2, f.Imperative.Split(' ').Length));
        Assert.All(RussianFrames.HomogeneousSets, s => Assert.Equal(3, s.Nouns.Length));
    }

    [Fact]
    public void AmbiguousWords_NotInWrongTemplates()
    {
        // Омоним «печь» и названия действий не попадают в R12-10.
        Assert.DoesNotContain("печь", RussianFrames.ObjectNouns);
        Assert.DoesNotContain("бег", RussianFrames.ObjectNouns);
        // Изолированные неоднозначные формы не спрашиваются без контекста:
        // R34-06/08 используют только начальные формы из своих банков.
        Assert.All(RussianFrames.Genders, g => Assert.Equal(g.Lemma, g.Lemma.ToLowerInvariant()));
    }

    // ————— Отчёт о покрытии банка по каждому шаблону —————

    [Theory]
    [InlineData(GradeBand.Grades12)]
    [InlineData(GradeBand.Grades34)]
    [InlineData(GradeBand.Grades56)]
    public void Coverage_EveryTemplateHasMinimumBank(GradeBand band)
    {
        var templates = band switch
        {
            GradeBand.Grades12 => RussianTemplates12.All,
            GradeBand.Grades34 => RussianTemplates34.All,
            _ => RussianTemplates56.All,
        };

        Assert.Equal(10, templates.Length);
        var usedLexemeSets = new Dictionary<string, HashSet<string>>();

        foreach (var template in templates)
        {
            var variants = template.AllVariants().ToList();
            var keys = variants.Select(v => v.Key).ToList();
            Assert.True(keys.Count >= 12, $"{template.Id}: всего {keys.Count} вариантов (< 12)");
            Assert.Equal(keys.Count, keys.Distinct().Count());

            // Различные ключи = различные учебные материалы: сигнатура включает
            // условие, варианты и токены (не только строку условия).
            var signatures = variants.Select(v => MaterialSignature(v.Task)).ToList();
            Assert.Equal(signatures.Count, signatures.Distinct().Count());

            var segments = keys.Select(k => k.Split('|')).ToList();
            _output.WriteLine($"{template.Id}: вариантов={keys.Count}");

            // Двух-/трёхветочные шаблоны: сегмент ключа с веткой различается по шаблону.
            var branchIndex = template.Id switch
            {
                "Russian.R12-10" => 0,
                "Russian.R34-04" => 0,
                "Russian.R34-07" => 1,
                "Russian.R34-10" => 1,
                "Russian.R56-08" => 2,
                "Russian.R56-09" => 2,
                _ => -1,
            };

            if (branchIndex >= 0)
            {
                var branches = segments.Select(s => s[branchIndex]).Distinct().ToList();
                Assert.True(branches.Count >= 2, $"{template.Id}: ветка всего одна");
                Assert.All(branches, b => Assert.True(segments.Count(s => s[branchIndex] == b) >= 6,
                    $"{template.Id}: ветка {b} почти пуста"));
                _output.WriteLine($"  ветки: {string.Join(", ", branches.Select(b => $"{b}={segments.Count(s => s[branchIndex] == b)}"))}");
            }

            // Предложенческие шаблоны: ≥3 структурных конструкций.
            if (template.Id is "Russian.R34-05" or "Russian.R34-07" or "Russian.R34-09" or "Russian.R34-10"
                or "Russian.R56-01" or "Russian.R56-02" or "Russian.R56-08" or "Russian.R56-09")
            {
                var frameIds = keys.Select(k => k.Split('|')[0]).Distinct().Count();
                Assert.True(frameIds >= 3, $"{template.Id}: конструкций {frameIds} (< 3)");
            }
        }
    }

    private static string MaterialSignature(SchoolTask task) =>
        task.Prompt
        + "|" + string.Join("|", task.Options?.Select(o => o.Text) ?? Array.Empty<string>())
        + "|" + string.Join("|", task.Tokens?.Select(t => t.Text) ?? Array.Empty<string>());

    [Fact]
    public void Coverage_R5605_AtLeast12VerbsOfEachConjugation_IncludingExceptions()
    {
        var first = RussianLexicon.Verbs.Where(v => v.Conj == Conjugation.First).ToList();
        var second = RussianLexicon.Verbs.Where(v => v.Conj == Conjugation.Second).ToList();

        Assert.True(first.Count >= 12, $"I спряжение: {first.Count}");
        Assert.True(second.Count >= 12, $"II спряжение: {second.Count}");
        Assert.Contains(first, v => v.IsException);   // брить/стелить
        Assert.Contains(second, v => v.IsException);  // смотреть/держать/…
        _output.WriteLine($"R56-05: I={first.Count} (исключений {first.Count(v => v.IsException)}), " +
                          $"II={second.Count} (исключений {second.Count(v => v.IsException)})");
    }

    [Fact]
    public void Coverage_BankSize_Report()
    {
        var forms = RussianLexicon.Nouns.Select(n => n.LexemeId)
            .Concat(RussianLexicon.Adjectives.Select(a => a.LexemeId))
            .Concat(RussianLexicon.Verbs.Select(v => v.LexemeId))
            .Distinct().Count();

        var nounForms = RussianLexicon.Nouns.Length * 12;
        _output.WriteLine($"Лексем: существительных={RussianLexicon.Nouns.Length}, " +
                          $"прилагательных={RussianLexicon.Adjectives.Length}, глаголов={RussianLexicon.Verbs.Length}, " +
                          $"имён={RussianLexicon.Names.Length}, простых слов={RussianOrphograms.SimpleWords.Length}");
        _output.WriteLine($"Словоформ существительных в банке: {nounForms}");
        _output.WriteLine($"Орфограммы: ЖИ-ШИ={RussianOrphograms.ZhiShi.Length}, ЧА-ЩА={RussianOrphograms.ChaSha.Length}, " +
                          $"ЧУ-ЩУ={RussianOrphograms.ChuShu.Length}, безударные={RussianOrphograms.UnstressedVowels.Length}, " +
                          $"парные={RussianOrphograms.PairedConsonants.Length}, непроизносимые={RussianOrphograms.SilentConsonants.Length}, " +
                          $"разделительные={RussianOrphograms.SoftSeparators.Length + RussianOrphograms.HardSeparators.Length}, " +
                          $"з/с={RussianOrphograms.PrefixZS.Length}, н/нн={RussianOrphograms.DoubleN.Length}, тся={RussianOrphograms.TsaWords.Length}");
        _output.WriteLine($"Конструкции: управления={RussianFrames.GovernmentFrames.Length}, " +
                          $"части речи={RussianFrames.PosSentences.Length}, роли={RussianFrames.RoleSentences.Length}, " +
                          $"число={RussianFrames.NumberSentences.Length}, обращения={RussianFrames.AddressFrames.Length}, " +
                          $"однородные: наборы={RussianFrames.HomogeneousSets.Length}, рамки={RussianFrames.HomoFrames.Length}");

        Assert.True(forms >= 60);
    }
}
