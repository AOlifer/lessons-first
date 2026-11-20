using System.Text.RegularExpressions;
using LessonsFirst.App.Tasks;
using LessonsFirst.App.Tasks.RussianTasks;
using Xunit;

namespace LessonsFirst.Tests;

/// <summary>
/// Движок русского: перемешанный цикл TemplateId, дедупликация, исчерпание
/// и прогон 1000 заданий на каждом уровне с независимой oracle-проверкой
/// каждого экземпляра (ответ пересчитывается из условия по таблицам RussianOracles).
/// </summary>
public class RussianEngineTests
{
    [Fact]
    public void SameSeed_ReproducibleSequence()
    {
        var a = Generate(GradeBand.Grades34, 777, 60);
        var b = Generate(GradeBand.Grades34, 777, 60);
        for (var i = 0; i < a.Count; i++)
        {
            Assert.Equal(a[i].TemplateId, b[i].TemplateId);
            Assert.Equal(a[i].Prompt, b[i].Prompt);
        }
    }

    [Theory]
    [InlineData(GradeBand.Grades12)]
    [InlineData(GradeBand.Grades34)]
    [InlineData(GradeBand.Grades56)]
    public void FullCycle_EachTemplateExactlyOnce(GradeBand band)
    {
        var engine = new RussianEngine(new Random(42), band);
        var ids = Enumerable.Range(0, 10).Select(_ => engine.Next().TemplateId).ToList();
        Assert.Equal(10, ids.Distinct().Count());
    }

    [Fact]
    public void NoConsecutiveSameTemplate()
    {
        var tasks = Generate(GradeBand.Grades56, 9, 500);
        for (var i = 1; i < tasks.Count; i++)
        {
            Assert.NotEqual(tasks[i - 1].TemplateId, tasks[i].TemplateId);
        }
    }

    [Theory]
    [InlineData(GradeBand.Grades12)]
    [InlineData(GradeBand.Grades34)]
    [InlineData(GradeBand.Grades56)]
    public void ThousandTasks_AllTemplatesCovered_AndValidated(GradeBand band)
    {
        var tasks = Generate(band, 20261120, 1000);

        var expectedIds = band switch
        {
            GradeBand.Grades12 => RussianTemplates12.All.Select(t => t.Id),
            GradeBand.Grades34 => RussianTemplates34.All.Select(t => t.Id),
            _ => RussianTemplates56.All.Select(t => t.Id),
        };
        Assert.Superset(expectedIds.ToHashSet(), tasks.Select(t => t.TemplateId).ToHashSet());

        foreach (var task in tasks)
        {
            Validate(task, band);
        }
    }

    [Fact]
    public void Exhaustion_EngineContinues_NoIdenticalConsecutive()
    {
        // R12-05 (ЖИ-ШИ) имеет всего 12 вариантов; в 1000 заданиях шаблон встретится
        // ~100 раз — движок обязан пережить исчерпание и не повторять задания подряд.
        var tasks = Generate(GradeBand.Grades12, 3, 1000);
        var masked = tasks.Where(t => t.TemplateId == "Russian.R12-05").ToList();

        Assert.True(masked.Count > 40, $"R12-05 встретился {masked.Count} раз");
        for (var i = 1; i < masked.Count; i++)
        {
            Assert.NotEqual(masked[i - 1].Prompt, masked[i].Prompt);
        }

        // Точно так же для маленького банка R56-07 (н/нн).
        var nn = Generate(GradeBand.Grades56, 4, 1000).Where(t => t.TemplateId == "Russian.R56-07").ToList();
        Assert.True(nn.Count > 40);
        for (var i = 1; i < nn.Count; i++)
        {
            Assert.NotEqual(nn[i - 1].Prompt, nn[i].Prompt);
        }
    }

    [Fact]
    public void Dedup_SameMaterialNotRepeatedWhileVariantsRemain()
    {
        // Пока у шаблона есть неиспользованные варианты, точные повторы исключены:
        // первые 12 появлений R12-08 (всего 12 вариантов) попарно различны.
        var tasks = Generate(GradeBand.Grades12, 21, 400)
            .Where(t => t.TemplateId == "Russian.R12-08")
            .Select(t => t.Prompt + "|" + string.Join("|", t.Options!.Select(o => o.Text)))
            .ToList();

        Assert.True(tasks.Count >= 12, $"R12-08 встретился {tasks.Count} раз");
        var firstCycle = tasks.Take(12).ToList();
        Assert.Equal(12, firstCycle.Distinct().Count());
    }

    private static List<SchoolTask> Generate(GradeBand band, int seed, int count)
    {
        var engine = new RussianEngine(new Random(seed), band);
        return Enumerable.Range(0, count).Select(_ => engine.Next()).ToList();
    }

    // ————— Независимая валидация: ответ пересчитывается из УСЛОВИЯ —————
    private static void Validate(SchoolTask task, GradeBand band)
    {
        Assert.Equal("Русский язык", task.Subject);
        Assert.Equal(band, task.Difficulty);
        Assert.False(string.IsNullOrWhiteSpace(task.Prompt));
        Assert.False(string.IsNullOrWhiteSpace(task.Hint));
        Assert.StartsWith("Russian.", task.TemplateId);

        switch (task.TemplateId)
        {
            case "Russian.R12-01":
            {
                var word = ExtractQuoted(task.Prompt);
                Assert.Equal(RussianTextUtil.CountLetters(word), task.ExpectedInteger);
                break;
            }
            case "Russian.R12-02":
            {
                var word = ExtractQuoted(task.Prompt);
                Assert.Equal(RussianTextUtil.CountVowels(word), task.ExpectedInteger);
                break;
            }
            case "Russian.R12-03":
            {
                var word = ExtractQuoted(task.Prompt);
                Assert.Equal(RussianTextUtil.CountVowels(word), task.ExpectedInteger);
                break;
            }
            case "Russian.R12-04":
            {
                var match = Regex.Match(task.Prompt, @"алфавиту: (.+), (.+), (.+), (.+)\?");
                Assert.True(match.Success, task.Prompt);
                var words = new[] { match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value, match.Groups[4].Value };
                Assert.Equal(4, words.Select(w => char.ToLowerInvariant(w[0])).Distinct().Count());
                var first = words.MinBy(w => RussianTextUtil.AlphabetIndex(char.ToLowerInvariant(w[0])));
                AssertCorrectChoice(task, first);
                break;
            }
            case "Russian.R12-05":
            case "Russian.R12-06":
            case "Russian.R12-07":
            {
                var masked = task.Prompt["Вставь букву: ".Length..];
                Assert.Contains('_', masked);
                Assert.Equal(1, masked.Count(c => c == '_'));
                var expected = task.TemplateId switch
                {
                    "Russian.R12-05" => 'и', // ЖИ-ШИ
                    "Russian.R12-06" => 'а', // ЧА-ЩА
                    _ => 'у',                // ЧУ-ЩУ
                };
                // Позиция относится к целевому правилу: рядом с пропуском —
                // ж/ш (ЖИ-ШИ) или ч/щ (ЧА-ЩА, ЧУ-ЩУ; ч/щ может стоять до или после пропуска).
                var maskIndex = masked.IndexOf('_');
                var neighbors = new[] { maskIndex - 1, maskIndex + 1 }
                    .Where(i => i >= 0 && i < masked.Length)
                    .Select(i => masked[i])
                    .ToArray();
                var ruleLetters = task.TemplateId == "Russian.R12-05" ? new[] { 'ж', 'ш' } : new[] { 'ч', 'щ' };
                Assert.Contains(neighbors, n => ruleLetters.Contains(n));

                Assert.Equal(expected, task.AcceptedAnswers!.Single().Single());
                AssertShortText(task, expected.ToString());
                break;
            }
            case "Russian.R12-08":
            {
                var correct = task.Options!.Single(o => o.Id == task.CorrectOptionId).Text;
                var wrong = task.Options!.Single(o => o.Id != task.CorrectOptionId).Text;
                Assert.Equal(correct.ToLowerInvariant(), wrong);
                Assert.True(char.IsUpper(correct[0]));
                Assert.True(task.Policy!.CaseSensitive);
                break;
            }
            case "Russian.R12-09":
            {
                Assert.Contains('…', task.Prompt);
                var correct = task.Options!.Single(o => o.Id == task.CorrectOptionId).Text;
                var wrong = task.Options!.Single(o => o.Id != task.CorrectOptionId).Text;
                Assert.Equal(correct.ToLowerInvariant(), wrong);
                Assert.Contains(correct, RussianOracles.Names);
                Assert.True(task.Policy!.CaseSensitive);
                break;
            }
            case "Russian.R12-10":
            {
                var branch = task.Prompt.Contains("предмет") ? "предмет"
                    : task.Prompt.Contains("действие") ? "действие" : "признак";
                var list = branch switch
                {
                    "предмет" => RussianOracles.R1210Nouns,
                    "действие" => RussianOracles.R1210Verbs,
                    _ => RussianOracles.R1210Adjectives,
                };
                var correct = task.Options!.Single(o => o.Id == task.CorrectOptionId).Text;
                Assert.Contains(correct, list);
                break;
            }
            case "Russian.R34-01":
            {
                var maskedWord = MaskedWordOf(task.Prompt);
                Assert.Contains(maskedWord, RussianOracles.UnstressedLetters.Keys);
                var letter = RussianOracles.UnstressedLetters[maskedWord];
                AssertShortText(task, letter.ToString());
                // Проверочное слово не раскрывается в подсказке до решения.
                Assert.DoesNotContain("Проверочное слово", task.Hint);
                break;
            }
            case "Russian.R34-02":
            {
                var maskedWord = MaskedWordOf(task.Prompt);
                Assert.Contains(maskedWord, RussianOracles.PairedLetters.Keys);
                AssertShortText(task, RussianOracles.PairedLetters[maskedWord].ToString());
                break;
            }
            case "Russian.R34-03":
            {
                var maskedWord = MaskedWordOf(task.Prompt).ToLowerInvariant();
                Assert.Contains(maskedWord, RussianOracles.SilentLetters.Keys);
                AssertShortText(task, RussianOracles.SilentLetters[maskedWord].ToString());
                break;
            }
            case "Russian.R34-04":
            {
                var maskedWord = MaskedWordOf(task.Prompt).ToLowerInvariant();
                Assert.Contains(maskedWord, RussianOracles.SeparatorLetters.Keys);
                AssertShortText(task, RussianOracles.SeparatorLetters[maskedWord].ToString());
                break;
            }
            case "Russian.R34-05":
            {
                Assert.Contains("ВСЕ", task.Prompt);
                var targetPos = task.Prompt.Contains("существительные") ? PartOfSpeech.Noun
                    : task.Prompt.Contains("прилагательные") ? PartOfSpeech.Adjective
                    : PartOfSpeech.Verb;
                var expected = task.Tokens!
                    .Where(t => RussianOracles.WordPos[t.Text.TrimEnd('.').ToLowerInvariant()] == targetPos)
                    .Select(t => t.TokenId)
                    .ToHashSet();
                Assert.True(expected.Count >= 1);
                Assert.Equal(expected, task.ExpectedTokenIds!.ToHashSet());
                break;
            }
            case "Russian.R34-06":
            {
                var lemma = ExtractQuoted(task.Prompt);
                var gender = task.Prompt.Contains("Какого рода") ? task.Options!.Single(o => o.Id == task.CorrectOptionId).Text : "";
                Assert.Equal(lemma switch
                {
                    "окно" or "солнце" or "море" or "платье" => "среднего",
                    "конь" or "папа" or "овощ" or "дедушка" or "стакан" or "огонь" => "мужского",
                    _ => "женского",
                }, gender);
                break;
            }
            case "Russian.R34-07":
            {
                var word = ExtractQuoted(task.Prompt);
                Assert.Contains(word, RussianOracles.NumberByForm.Keys);
                var expected = RussianOracles.NumberByForm[word] == "pl" ? "множественное" : "единственное";
                AssertCorrectChoice(task, expected);
                // Согласование глагола действительно задаёт число: слово — последнее в предложении.
                var sentence = task.Prompt.Split("? ", 2)[^1].TrimEnd('.');
                var lastWord = sentence.Split(' ')[^1];
                Assert.Equal(word, lastWord);
                break;
            }
            case "Russian.R34-08":
            {
                var lemma = ExtractQuoted(task.Prompt);
                var declension = lemma switch
                {
                    "папа" or "мама" or "девочка" or "дедушка" or "деревня" or "школа" => "d1",
                    "конь" or "окно" or "море" or "солнце" or "стакан" => "d2",
                    _ => "d3",
                };
                Assert.Equal(declension, task.CorrectOptionId);
                break;
            }
            case "Russian.R34-09":
            {
                var form = ExtractQuoted(task.Prompt);
                var sentence = task.Prompt[(task.Prompt.IndexOf(':') + 2)..];
                var prefix = RussianOracles.FramePrefixes.Single(kv => sentence.StartsWith(kv.Key)).Key;
                var c = RussianOracles.FramePrefixes[prefix];
                // В независимой таблице форма стоит именно в этом падеже — единственным образом.
                var found = RussianOracles.FindNounFormInCase(form, c);
                Assert.Single(found);
                Assert.Contains(form, sentence);
                var expectedId = c switch
                {
                    CaseRu.Nominative => "case-nom",
                    CaseRu.Genitive => "case-gen",
                    CaseRu.Dative => "case-dat",
                    CaseRu.Accusative => "case-acc",
                    CaseRu.Instrumental => "case-ins",
                    _ => "case-prep",
                };
                Assert.Equal(expectedId, task.CorrectOptionId);
                break;
            }
            case "Russian.R34-10":
            {
                var askSubject = task.Prompt.Contains("подлежащее");
                var sentence = string.Join(" ", task.Tokens!.Select(t => t.Text)).TrimEnd('.');
                Assert.Contains(sentence, RussianOracles.RoleWords.Keys);
                var (subj, pred) = RussianOracles.RoleWords[sentence];
                var expectedWord = (askSubject ? subj : pred) == subj ? sentence.Split(' ')[subj] : sentence.Split(' ')[pred];
                var expectedToken = task.Tokens!.Single(t => t.Text.TrimEnd('.') == expectedWord).TokenId;
                Assert.Equal(new HashSet<string> { expectedToken }, task.ExpectedTokenIds!.ToHashSet());
                break;
            }
            case "Russian.R56-01":
            {
                var lemma = ExtractQuoted(task.Prompt);
                var number = task.Prompt.Contains("множественном") ? NumberRu.Plural : NumberRu.Singular;
                var sentence = task.Prompt[(task.Prompt.IndexOf(':') + 1)..].Trim();
                var prefix = RussianOracles.FramePrefixes.Single(kv => sentence.StartsWith(kv.Key)).Key;
                var c = RussianOracles.FramePrefixes[prefix];
                var index = (number == NumberRu.Plural ? 6 : 0) + (int)c;
                var expected = RussianOracles.NounParadigms[lemma][index];
                AssertShortText(task, expected);
                break;
            }
            case "Russian.R56-02":
            {
                var match = Regex.Match(task.Prompt, @"\(([^)]+)\) (\S+)");
                Assert.True(match.Success, task.Prompt);
                var adjLemma = match.Groups[1].Value;
                var nounForm = match.Groups[2].Value;
                var prefix = RussianOracles.FramePrefixes.Single(kv => task.Prompt.Contains(kv.Key)).Key;
                var c = RussianOracles.FramePrefixes[prefix];
                var found = RussianOracles.FindNounFormInCase(nounForm, c);
                Assert.Single(found); // (лемма, число) определяются однозначно
                var (lemma, number) = found[0];
                var expected = RussianOracles.AdjectiveForm(adjLemma, RussianOracles.NounGenders[lemma], number, c);
                Assert.True(task.AcceptedAnswers!.Contains(expected.ToLowerInvariant()),
                    $"нет «{expected}» в [{string.Join(", ", task.AcceptedAnswers)}]: {task.Prompt}");
                AssertShortText(task, expected);
                break;
            }
            case "Russian.R56-03":
            {
                var match = Regex.Match(task.Prompt, @"время: (\S+) ___ .+ \(([^)]+)\)");
                Assert.True(match.Success, task.Prompt);
                var pronoun = match.Groups[1].Value;
                var infinitive = match.Groups[2].Value;
                var person = RussianOracles.Pronouns.Single(p => p.Pronoun == pronoun).Person;
                AssertShortText(task, RussianOracles.VerbPresent[infinitive][person]);
                break;
            }
            case "Russian.R56-04":
            {
                var before = task.Prompt[(task.Prompt.IndexOf('«') + 1)..task.Prompt.IndexOf("___")];
                Assert.Contains(before, RussianOracles.TsaAnswers.Keys);
                var expectedId = RussianOracles.TsaAnswers[before] ? "tsa" : "tsya";
                Assert.Equal(expectedId, task.CorrectOptionId);
                break;
            }
            case "Russian.R56-05":
            {
                var infinitive = ExtractQuoted(task.Prompt);
                Assert.Equal(RussianOracles.VerbConj[infinitive] == Conjugation.First ? "c1" : "c2", task.CorrectOptionId);
                break;
            }
            case "Russian.R56-06":
            {
                var masked = task.Prompt["Вставь букву: ".Length..];
                var after = masked[(masked.IndexOf('_') + 1)..];
                var next = after[0];
                // Правило: перед глухой — с, перед звонкой/сонорной — з.
                var expected = "кпстфхцчшщ".Contains(next) ? 'с' : 'з';
                AssertShortText(task, expected.ToString());
                break;
            }
            case "Russian.R56-07":
            {
                var maskedWord = MaskedWordOf(task.Prompt);
                Assert.Contains(maskedWord, RussianOracles.DoubleNLetters.Keys);
                AssertShortText(task, RussianOracles.DoubleNLetters[maskedWord]);
                break;
            }
            case "Russian.R56-08":
            {
                var hasAnd = task.Tokens!.Any(t => t.Text == "и");
                Assert.Equal(hasAnd ? 1 : 2, task.ExpectedBoundaryIds!.Count);
                var nouns = task.Tokens!.Where(t => RussianOracles.HomogeneousNouns.Contains(t.Text.TrimEnd('.'))).ToList();
                Assert.Equal(3, nouns.Count);
                foreach (var boundaryId in task.ExpectedBoundaryIds!)
                {
                    var after = int.Parse(boundaryId[1..]);
                    Assert.Contains(task.Tokens![after].Text.TrimEnd('.'), RussianOracles.HomogeneousNouns);
                    Assert.Contains(task.Tokens![after + 1].Text.TrimEnd('.'), RussianOracles.HomogeneousNouns);
                    if (hasAnd)
                    {
                        Assert.NotEqual("и", task.Tokens[after + 1].Text); // запятая не перед «и»
                    }
                }

                break;
            }
            case "Russian.R56-09":
            {
                Assert.Equal(1, task.ExpectedBoundaryIds!.Count);
                var first = task.Tokens![0].Text;
                var isStart = char.IsUpper(first[0]) && RussianOracles.Names.Contains(first);
                var expectedBoundary = isStart ? "b0" : $"b{task.Tokens!.Count - 2}";
                Assert.Equal(expectedBoundary, task.ExpectedBoundaryIds.Single());
                break;
            }
            case "Russian.R56-10":
            {
                var form = ExtractQuoted(task.Prompt);
                var prefix = RussianOracles.FramePrefixes.Single(kv => task.Prompt.Contains(kv.Key)).Key;
                var c = RussianOracles.FramePrefixes[prefix];
                var found = RussianOracles.FindNounFormInCase(form, c);
                Assert.Single(found);
                var (lemma, number) = found[0];
                var expectedText =
                    $"{RussianOracles.GenderName(RussianOracles.NounGenders[lemma])} род, " +
                    $"{RussianOracles.NumberName(number)} число, {RussianOracles.CaseName(c)} падеж";
                AssertCorrectChoice(task, expectedText);
                Assert.Equal(4, task.Options!.Select(o => o.Text).Distinct().Count());
                break;
            }
            default:
                Assert.Fail($"Неизвестный шаблон {task.TemplateId}");
                break;
        }

        ValidateFormStructure(task);
    }

    private static void ValidateFormStructure(SchoolTask task)
    {
        switch (task.Form)
        {
            case AnswerForm.Choice:
            {
                Assert.NotNull(task.Options);
                Assert.True(task.Options!.Count >= 2);
                var policy = task.Policy ?? new AnswerPolicy { CaseSensitive = false };
                var normalized = task.Options.Select(o => AnswerValidator.NormalizeRussian(o.Text, policy)).ToList();
                Assert.Equal(normalized.Count, normalized.Distinct().Count()); // после нормализации уникальны
                Assert.Equal(1, task.Options.Count(o => o.Id == task.CorrectOptionId)); // правильный ровно один
                Assert.Equal(task.Options.Count, task.Options.Select(o => o.Id).Distinct().Count());
                break;
            }
            case AnswerForm.ShortText:
            {
                Assert.NotEmpty(task.AcceptedAnswers);
                Assert.All(task.AcceptedAnswers!, a =>
                    Assert.Equal(AnswerCheckResult.Correct,
                        AnswerValidator.CheckShortText(a, task.AcceptedAnswers!, task.Policy!, task.InputMaxLength)));
                Assert.True(task.InputMaxLength >= task.AcceptedAnswers!.Max(a => a.Length));
                break;
            }
            case AnswerForm.TokenSelection:
            {
                Assert.NotEmpty(task.Tokens);
                Assert.NotEmpty(task.ExpectedTokenIds);
                Assert.Subset(task.Tokens!.Select(t => t.TokenId).ToHashSet(), task.ExpectedTokenIds!.ToHashSet());
                break;
            }
            case AnswerForm.BoundarySelection:
            {
                Assert.NotEmpty(task.Tokens);
                Assert.NotEmpty(task.ExpectedBoundaryIds);
                var ids = task.Tokens!.Select(t => t.TokenId).ToHashSet();
                Assert.All(task.Boundaries!, b => Assert.True(b.AfterTokenIndex >= 0 && b.AfterTokenIndex < task.Tokens!.Count - 1));
                Assert.Subset(task.Boundaries!.Select(b => b.BoundaryId).ToHashSet(), task.ExpectedBoundaryIds!.ToHashSet());
                Assert.Equal(task.Tokens.Count - 1, task.Boundaries!.Count);
                break;
            }
            case AnswerForm.Integer:
                break;
            default:
                Assert.Fail($"Форма {task.Form} не ожидается в русском модуле");
                break;
        }
    }

    private static string ExtractQuoted(string prompt)
    {
        var match = Regex.Match(prompt, @"«([^»]+)»");
        Assert.True(match.Success, prompt);
        return match.Groups[1].Value;
    }

    private static string MaskedWordOf(string prompt)
    {
        // Замаскировано ровно одно слово условия (в любом формате инструкции).
        var word = prompt.Split(' ', '(', '«', '"').Select(w => w.Trim('?', '!', ',', ';', ':'))
            .FirstOrDefault(w => w.Contains('_'));
        Assert.False(word is null, prompt);
        return word!;
    }

    private static void AssertShortText(SchoolTask task, string expected)
    {
        Assert.Equal(AnswerCheckResult.Correct,
            AnswerValidator.CheckShortText(expected, task.AcceptedAnswers!, task.Policy!, task.InputMaxLength));
    }

    private static void AssertCorrectChoice(SchoolTask task, string expectedText)
    {
        var correct = task.Options!.Single(o => o.Id == task.CorrectOptionId);
        Assert.Equal(expectedText, correct.Text);
    }
}
