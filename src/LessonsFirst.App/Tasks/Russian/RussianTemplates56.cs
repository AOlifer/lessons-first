namespace LessonsFirst.App.Tasks.RussianTasks;

/// <summary>
/// Уровень «5–6 класс — сложный»: Russian.R56-01…Russian.R56-10.
/// Условия 4…12 слов; проверенные парадигмы и конструкции; нормативные
/// варианты форм принимаются полностью.
/// </summary>
internal static class RussianTemplates56
{
    public static readonly IRussianTemplate[] All =
    {
        new R5601(), new R5602(), new R5603(), new R5604(), new R5605(),
        new R5606(), new R5607(), new R5608(), new R5609(), new R5610(),
    };

    private static IReadOnlyList<string> WordFormAnswers(string form)
    {
        // Допустимая вариативность печати ё: конём и конем — обе формы нормативны.
        var lower = form.ToLowerInvariant();
        return lower.Contains('ё')
            ? new[] { lower, lower.Replace('ё', 'е') }
            : new[] { lower };
    }

    private static string FrameSentence(RussianFrames.GovFrame frame, string filler) =>
        frame.Prefix + filler + frame.Suffix;

    private static IEnumerable<(RussianFrames.GovFrame Frame, NounLexeme Noun, NumberRu Number)> CompatibleNouns()
    {
        foreach (var frame in RussianFrames.GovernmentFrames)
        {
            foreach (var noun in RussianLexicon.Nouns.Where(n => frame.SemClasses.Contains(n.SemClass)))
            {
                if (frame.PluralOnly)
                {
                    yield return (frame, noun, NumberRu.Plural);
                }
                else
                {
                    yield return (frame, noun, NumberRu.Singular);
                    yield return (frame, noun, NumberRu.Plural);
                }
            }
        }
    }

    // R56-01: падежная форма существительного.
    private sealed class R5601 : RussianTemplateBase
    {
        public override string Id => "Russian.R56-01";

        public override SchoolTask Build(Random rng, out string key)
        {
            var combo = CompatibleNouns().ToList();
            var (frame, noun, number) = combo[rng.Next(combo.Count)];
            return Make(frame, noun, number, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var (frame, noun, number) in CompatibleNouns())
            {
                yield return (Make(frame, noun, number, out var key), key);
            }
        }

        private static SchoolTask Make(RussianFrames.GovFrame frame, NounLexeme noun, NumberRu number, out string key)
        {
            key = $"{frame.FrameId}|{noun.LexemeId}|{number}";
            var form = noun.Form(frame.Case, number);
            var numberName = number == NumberRu.Singular ? "единственном" : "множественном";
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R56-01",
                Prompt = $"Поставь слово «{noun.Lemma}» в {numberName} числе: {FrameSentence(frame, "___")}",
                Hint = "Определи падеж по слову рядом (предлогу или глаголу), затем измени слово.",
                Form = AnswerForm.ShortText,
                AcceptedAnswers = WordFormAnswers(form),
                InputMaxLength = 24,
                Policy = WordFormPolicy,
                Explanation = $"Правильно: {FrameSentence(frame, form)}.",
            };
        }
    }

    // R56-02: согласование прилагательного.
    private sealed class R5602 : RussianTemplateBase
    {
        public override string Id => "Russian.R56-02";

        public override SchoolTask Build(Random rng, out string key)
        {
            var combo = CompatibleAdjectiveCombos().ToList();
            var (frame, noun, adj, number) = combo[rng.Next(combo.Count)];
            return Make(frame, noun, adj, number, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var combo in CompatibleAdjectiveCombos())
            {
                yield return (Make(combo.Frame, combo.Noun, combo.Adj, combo.Number, out var key), key);
            }
        }

        private static IEnumerable<(RussianFrames.GovFrame Frame, NounLexeme Noun, AdjectiveLexeme Adj, NumberRu Number)> CompatibleAdjectiveCombos() =>
            from frame in RussianFrames.GovernmentFrames
            from noun in RussianLexicon.Nouns
            where frame.SemClasses.Contains(noun.SemClass) && !noun.Animate
            from adj in RussianLexicon.Adjectives
            from number in frame.PluralOnly
                ? new[] { NumberRu.Plural }
                : new[] { NumberRu.Singular, NumberRu.Plural }
            select (Frame: frame, Noun: noun, Adj: adj, Number: number);

        private static SchoolTask Make(RussianFrames.GovFrame frame, NounLexeme noun, AdjectiveLexeme adj, NumberRu number, out string key)
        {
            key = $"{frame.FrameId}|{noun.LexemeId}|{adj.LexemeId}|{number}";
            var nounForm = noun.Form(frame.Case, number);
            var accepted = adj.Accepted(noun.Gender, number, frame.Case, animate: false);
            var prompt = $"Вставь нужную форму: {frame.Prefix}({adj.Lemma}) {nounForm}{frame.Suffix}";
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R56-02",
                Prompt = prompt,
                Hint = "Прилагательное согласуется с существительным в роде, числе и падеже.",
                Form = AnswerForm.ShortText,
                AcceptedAnswers = accepted.Select(a => a.ToLowerInvariant()).ToList(),
                InputMaxLength = 24,
                Policy = WordFormPolicy,
                Explanation = $"Правильно: {frame.Prefix}{accepted[0]} {nounForm}{frame.Suffix}",
            };
        }
    }

    // R56-03: личная форма глагола в настоящем времени.
    private sealed class R5603 : RussianTemplateBase
    {
        public override string Id => "Russian.R56-03";

        internal static readonly (string Pronoun, int Person, NumberRu Number)[] Pronouns =
        {
            ("я", 1, NumberRu.Singular),
            ("ты", 2, NumberRu.Singular),
            ("он", 3, NumberRu.Singular),
            ("она", 3, NumberRu.Singular),
            ("мы", 1, NumberRu.Plural),
            ("вы", 2, NumberRu.Plural),
            ("они", 3, NumberRu.Plural),
        };

        public override SchoolTask Build(Random rng, out string key)
        {
            var verbs = RussianLexicon.Verbs.Where(v => v.Complement is not null).ToList();
            var verb = Pick(rng, verbs);
            var pronoun = Pronouns[rng.Next(Pronouns.Length)];
            return Make(verb, pronoun, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var verb in RussianLexicon.Verbs.Where(v => v.Complement is not null))
            {
                foreach (var pronoun in Pronouns)
                {
                    yield return (Make(verb, pronoun, out var key), key);
                }
            }
        }

        private static SchoolTask Make(VerbLexeme verb, (string Pronoun, int Person, NumberRu Number) pronoun, out string key)
        {
            key = $"{verb.LexemeId}|{pronoun.Pronoun}";
            var form = verb.Present(pronoun.Person, pronoun.Number);
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R56-03",
                Prompt = $"Настоящее время: {pronoun.Pronoun} ___ {verb.Complement} ({verb.Infinitive})",
                Hint = "Определи лицо и число подлежащего, затем вспомни форму глагола.",
                Form = AnswerForm.ShortText,
                AcceptedAnswers = WordFormAnswers(form),
                InputMaxLength = 24,
                Policy = WordFormPolicy,
                Explanation = $"Правильно: {pronoun.Pronoun} {form} {verb.Complement}.",
            };
        }
    }

    // R56-04: -тся/-ться по контексту.
    private sealed class R5604 : RussianTemplateBase
    {
        public override string Id => "Russian.R56-04";

        public override SchoolTask Build(Random rng, out string key)
        {
            var entry = Pick(rng, RussianOrphograms.TsaWords);
            return Make(entry, rng, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var entry in RussianOrphograms.TsaWords)
            {
                yield return (Make(entry, new Random(entry.EntryId.GetHashCode()), out var key), key);
            }
        }

        private static SchoolTask Make(RussianOrphograms.TsaEntry entry, Random rng, out string key)
        {
            key = $"{entry.EntryId}";
            var options = ShuffledOptions(rng, ("tsa", "тся"), ("tsya", "ться"));
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R56-04",
                Prompt = $"Что пишется на месте пропуска: «{entry.Before}___{entry.After}»?",
                Hint = "Задай вопрос к слову: «что делает?» — тся, «что делать?» — ться.",
                Form = AnswerForm.Choice,
                Options = options,
                CorrectOptionId = entry.IsTsa ? "tsa" : "tsya",
                Policy = LetterPolicy,
                Explanation = $"Правильно: {entry.FullWord}.",
            };
        }
    }

    // R56-05: спряжение глагола (I/II, включая исключения; разноспрягаемые исключены из банка).
    private sealed class R5605 : RussianTemplateBase
    {
        public override string Id => "Russian.R56-05";

        public override SchoolTask Build(Random rng, out string key)
        {
            var verb = Pick(rng, RussianLexicon.Verbs);
            return Make(verb, rng, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var verb in RussianLexicon.Verbs)
            {
                yield return (Make(verb, new Random(verb.LexemeId.GetHashCode()), out var key), key);
            }
        }

        private static SchoolTask Make(VerbLexeme verb, Random rng, out string key)
        {
            key = $"{verb.LexemeId}";
            var options = ShuffledOptions(rng, ("c1", "I спряжение"), ("c2", "II спряжение"));
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R56-05",
                Prompt = $"Какого спряжения глагол «{verb.Infinitive}»?",
                Hint = "Вспомни окончание 3-го лица: -ет/-ут — I, -ит/-ат — II. Не забудь исключения!",
                Form = AnswerForm.Choice,
                Options = options,
                CorrectOptionId = verb.Conj == Conjugation.First ? "c1" : "c2",
                Policy = LetterPolicy,
                Explanation = verb.IsException
                    ? $"«{verb.Infinitive}» — исключение: {verb.Conj.Name()} спряжение."
                    : $"«{verb.Infinitive}» — {verb.Conj.Name()} спряжение.",
            };
        }
    }

    // R56-06: з/с в приставке.
    private sealed class R5606 : RussianTemplateBase
    {
        public override string Id => "Russian.R56-06";

        public override SchoolTask Build(Random rng, out string key)
        {
            var entry = Pick(rng, RussianOrphograms.PrefixZS);
            return Make(entry, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var entry in RussianOrphograms.PrefixZS)
            {
                yield return (Make(entry, out var key), key);
            }
        }

        private static SchoolTask Make(RussianOrphograms.PrefixEntry entry, out string key)
        {
            key = $"{entry.EntryId}";
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R56-06",
                Prompt = $"Вставь букву: {entry.Masked}",
                Hint = "В приставке перед глухой согласной пиши с, перед звонкой — з.",
                Form = AnswerForm.ShortText,
                AcceptedAnswers = new[] { entry.Letter.ToString() },
                InputMaxLength = 1,
                Policy = LetterPolicy,
                Explanation = $"Правильно: {entry.FullWord}.",
            };
        }
    }

    // R56-07: н/нн в полных прилагательных (маска скрывает весь фрагмент н…нн).
    private sealed class R5607 : RussianTemplateBase
    {
        public override string Id => "Russian.R56-07";

        public override SchoolTask Build(Random rng, out string key)
        {
            var entry = Pick(rng, RussianOrphograms.DoubleN);
            return Make(entry, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var entry in RussianOrphograms.DoubleN)
            {
                yield return (Make(entry, out var key), key);
            }
        }

        private static SchoolTask Make(RussianOrphograms.DoubleNEntry entry, out string key)
        {
            key = $"{entry.EntryId}";
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R56-07",
                Prompt = entry.Phrase,
                Hint = "Одна Н или НН зависит от суффикса, который есть в слове: -ан-, -ян-, -ин- — одна Н.",
                Form = AnswerForm.ShortText,
                AcceptedAnswers = new[] { entry.Fragment },
                InputMaxLength = 2,
                Policy = LetterPolicy,
                Explanation = $"Правильно: {entry.Phrase.Replace("_", entry.Fragment)} ({entry.Rule}).",
            };
        }
    }

    // R56-08: запятые при однородных членах (две ветки: без союза / с одиночным «и»).
    private sealed class R5608 : RussianTemplateBase
    {
        public override string Id => "Russian.R56-08";

        public override SchoolTask Build(Random rng, out string key)
        {
            var withAnd = rng.Next(2) == 1;
            var frame = Pick(rng, RussianFrames.HomoFrames);
            var set = Pick(rng, RussianFrames.HomogeneousSets
                .Where(s => s.Categories.Intersect(frame.Categories).Any()).ToList());
            return Make(frame, set, withAnd, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var frame in RussianFrames.HomoFrames)
            {
                foreach (var set in RussianFrames.HomogeneousSets.Where(s => s.Categories.Intersect(frame.Categories).Any()))
                {
                    yield return (Make(frame, set, false, out var key1), key1);
                    yield return (Make(frame, set, true, out var key2), key2);
                }
            }
        }

        private static SchoolTask Make(RussianFrames.HomoFrame frame, RussianFrames.HomoSet set, bool withAnd, out string key)
        {
            key = $"{frame.FrameId}|{set.SetId}|{(withAnd ? "and" : "list")}";

            var tokens = new List<TaskToken>();
            foreach (var word in frame.Prefix.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                tokens.Add(new TaskToken($"t{tokens.Count}", word, PartOfSpeech.None, "frame"));
            }

            tokens.Add(new TaskToken($"t{tokens.Count}", frame.VerbPl, PartOfSpeech.Verb, "frame"));
            var noun0 = tokens.Count;
            tokens.Add(new TaskToken($"t{tokens.Count}", set.Nouns[0], PartOfSpeech.Noun, "homogeneous"));
            tokens.Add(new TaskToken($"t{tokens.Count}", set.Nouns[1], PartOfSpeech.Noun, "homogeneous"));
            if (withAnd)
            {
                tokens.Add(new TaskToken($"t{tokens.Count}", "и", PartOfSpeech.None, "conjunction"));
            }

            tokens.Add(new TaskToken($"t{tokens.Count}", set.Nouns[2], PartOfSpeech.Noun, "homogeneous"));
            var last = tokens.Count - 1;
            tokens[last] = tokens[last] with { Text = tokens[last].Text + "." };

            var expected = new HashSet<string> { $"b{noun0}", $"b{noun0 + 1}" };
            if (withAnd)
            {
                // Перед одиночным «и» запятая не нужна: граница b(noun0+1) после
                // второго существительного — не целевая.
                expected.Remove($"b{noun0 + 1}");
            }

            var boundaries = new List<BoundaryGap>();
            for (var i = 0; i + 1 < tokens.Count; i++)
            {
                boundaries.Add(new BoundaryGap($"b{i}", i));
            }

            var sentence = string.Join(" ", tokens.Select(t => t.Text));
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R56-08",
                Prompt = $"Где нужны запятые? Щёлкни между словами: {sentence}",
                Hint = withAnd
                    ? "Между однородными членами без союза — запятая; перед одиночным «и» запятая не ставится."
                    : "Между однородными членами без союза ставится запятая.",
                Form = AnswerForm.BoundarySelection,
                Tokens = tokens,
                Boundaries = boundaries,
                ExpectedBoundaryIds = expected,
                Explanation = withAnd
                    ? "Одиночное «и» соединяет последние два члена — запятая перед ним не нужна."
                    : "Запятые ставятся между всеми однородными членами.",
            };
        }
    }

    // R56-09: запятая при обращении (ветки: обращение в начале / в конце).
    private sealed class R5609 : RussianTemplateBase
    {
        public override string Id => "Russian.R56-09";

        public override SchoolTask Build(Random rng, out string key)
        {
            var atStart = rng.Next(2) == 0;
            var frame = Pick(rng, RussianFrames.AddressFrames);
            var name = Pick(rng, RussianLexicon.Names);
            return Make(frame, name, atStart, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var frame in RussianFrames.AddressFrames)
            {
                foreach (var name in RussianLexicon.Names)
                {
                    yield return (Make(frame, name, true, out var key1), key1);
                    yield return (Make(frame, name, false, out var key2), key2);
                }
            }
        }

        private static SchoolTask Make(RussianFrames.AddressFrame frame, string name, bool atStart, out string key)
        {
            key = $"{frame.FrameId}|{name.ToLowerInvariant()}|{(atStart ? "start" : "end")}";

            // Повелительная форма глагола + прямое дополнение; обращение — отдельная роль,
            // подлежащим не является (безличная конструкция). Фраза начинается с заглавной.
            var imperative = char.ToUpperInvariant(frame.Imperative[0]) + frame.Imperative[1..];
            var words = imperative.Split(' ');
            var tokens = new List<TaskToken>();
            if (atStart)
            {
                tokens.Add(new TaskToken("t0", name, PartOfSpeech.None, "address"));
                tokens.Add(new TaskToken("t1", words[0], PartOfSpeech.Verb, "predicate"));
                tokens.Add(new TaskToken("t2", words[1], PartOfSpeech.Noun, "object"));
            }
            else
            {
                tokens.Add(new TaskToken("t0", words[0], PartOfSpeech.Verb, "predicate"));
                tokens.Add(new TaskToken("t1", words[1], PartOfSpeech.Noun, "object"));
                tokens.Add(new TaskToken("t2", name, PartOfSpeech.None, "address"));
            }

            var last = tokens.Count - 1;
            tokens[last] = tokens[last] with { Text = tokens[last].Text + "." };

            var boundaries = new List<BoundaryGap>();
            for (var i = 0; i + 1 < tokens.Count; i++)
            {
                boundaries.Add(new BoundaryGap($"b{i}", i));
            }

            var expectedBoundary = atStart ? "b0" : "b1";
            var sentence = string.Join(" ", tokens.Select(t => t.Text));

            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R56-09",
                Prompt = $"Где нужна запятая? Щёлкни между словами: {sentence}",
                Hint = "Обращение — имя того, к кому обращаются; оно выделяется запятой.",
                Form = AnswerForm.BoundarySelection,
                Tokens = tokens,
                Boundaries = boundaries,
                ExpectedBoundaryIds = new HashSet<string> { expectedBoundary },
                Explanation = atStart ? $"{name}, {imperative.ToLowerInvariant()}." : $"{imperative.ToLowerInvariant()}, {name}.",
            };
        }
    }

    // R56-10: набор грамматических признаков существительного.
    private sealed class R5610 : RussianTemplateBase
    {
        public override string Id => "Russian.R56-10";

        public override SchoolTask Build(Random rng, out string key)
        {
            var combo = CompatibleNouns().Where(c => c.Frame.FrameId != "gf-rastut").ToList();
            var (frame, noun, number) = combo[rng.Next(combo.Count)];
            return Make(frame, noun, number, rng, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var (frame, noun, number) in CompatibleNouns().Where(c => c.Frame.FrameId != "gf-rastut"))
            {
                yield return (Make(frame, noun, number,
                    new Random((frame.FrameId + noun.LexemeId).GetHashCode()), out var key), key);
            }
        }

        private static SchoolTask Make(RussianFrames.GovFrame frame, NounLexeme noun, NumberRu number, Random rng, out string key)
        {
            key = $"{frame.FrameId}|{noun.LexemeId}|{number}";
            var form = noun.Form(frame.Case, number);
            var sentence = FrameSentence(frame, form);

            var correct = FeatureText(noun.Gender, number, frame.Case);

            // Неверные варианты: отличается ровно один признак (род / число / падеж).
            var otherGender = noun.Gender == Gender.Masculine ? Gender.Feminine : Gender.Masculine;
            var otherNumber = number == NumberRu.Singular ? NumberRu.Plural : NumberRu.Singular;
            var otherCase = frame.Case == CaseRu.Nominative ? CaseRu.Genitive
                : frame.Case == CaseRu.Accusative ? CaseRu.Dative
                : CaseRu.Nominative;

            var distractors = new List<string>
            {
                FeatureText(otherGender, number, frame.Case),
                FeatureText(noun.Gender, otherNumber, frame.Case),
                FeatureText(noun.Gender, number, otherCase),
            }
            .Where(d => d != correct)
            .Distinct()
            .ToList();

            var options = new List<(string Id, string Text)> { ("f0", correct) };
            for (var i = 0; i < distractors.Count; i++)
            {
                options.Add(($"f{i + 1}", distractors[i]));
            }

            var shuffled = ShuffledOptions(rng, options.ToArray());
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R56-10",
                Prompt = $"Какие признаки у слова «{form}»? {sentence}",
                Hint = "Определи род, число и падеж по вопросу от соседних слов.",
                Form = AnswerForm.Choice,
                Options = shuffled,
                CorrectOptionId = "f0",
                Policy = LetterPolicy,
                Explanation = $"«{form}»: {correct}.",
            };
        }

        private static string FeatureText(Gender g, NumberRu n, CaseRu c) =>
            $"{g.Name()} род, {n.Name()} число, {c.Name()} падеж";
    }
}
