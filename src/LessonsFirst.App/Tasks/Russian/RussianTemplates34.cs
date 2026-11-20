namespace LessonsFirst.App.Tasks.RussianTasks;

/// <summary>
/// Уровень «3–4 класс — базовый»: Russian.R34-01…Russian.R34-10.
/// Слова и предложения из проверенного банка, контекст 3…9 слов.
/// Контекст исключает другое прочтение (изолированные «л_са», «книги» не выдаются).
/// </summary>
internal static class RussianTemplates34
{
    public static readonly IRussianTemplate[] All =
    {
        new R3401(), new R3402(), new R3403(), new R3404(), new R3405(),
        new R3406(), new R3407(), new R3408(), new R3409(), new R3410(),
    };

    private abstract class LetterInSentenceBase : RussianTemplateBase
    {
        public override string Id => TemplateId;

        protected abstract string TemplateId { get; }

        public override SchoolTask Build(Random rng, out string key)
        {
            var (task, taskKey) = Variants().ToList()[rng.Next(VariantsCount)];
            key = taskKey;
            return task;
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants() => Variants();

        protected abstract int VariantsCount { get; }

        protected abstract IEnumerable<(SchoolTask Task, string Key)> Variants();
    }

    // R34-01: проверяемая безударная гласная в корне.
    private sealed class R3401 : LetterInSentenceBase
    {
        protected override string TemplateId => "Russian.R34-01";
        protected override int VariantsCount => RussianOrphograms.UnstressedVowels.Length;

        protected override IEnumerable<(SchoolTask, string)> Variants()
        {
            foreach (var entry in RussianOrphograms.UnstressedVowels)
            {
                yield return (new SchoolTask
                {
                    Subject = Subject,
                    TemplateId = TemplateId,
                    Prompt = $"Вставь букву: {entry.Sentence}",
                    Hint = "Подбери проверочное слово, чтобы безударная гласная стала ударной.",
                    Form = AnswerForm.ShortText,
                    AcceptedAnswers = new[] { entry.Letter.ToString() },
                    InputMaxLength = 1,
                    Policy = LetterPolicy,
                    Explanation = $"{Fill(entry)} Проверочное слово: {entry.CheckWord}.",
                }, entry.EntryId);
            }
        }

        internal static string Fill(RussianOrphograms.UnstressedEntry entry) =>
            entry.Sentence.Replace("_", entry.Letter.ToString());
    }

    // R34-02: парная согласная в конце слова.
    private sealed class R3402 : LetterInSentenceBase
    {
        protected override string TemplateId => "Russian.R34-02";
        protected override int VariantsCount => RussianOrphograms.PairedConsonants.Length;

        protected override IEnumerable<(SchoolTask, string)> Variants()
        {
            foreach (var entry in RussianOrphograms.PairedConsonants)
            {
                yield return (new SchoolTask
                {
                    Subject = Subject,
                    TemplateId = TemplateId,
                    Prompt = $"Вставь букву: {entry.Sentence}",
                    Hint = "Проверь парную согласную: измени слово так, чтобы после согласной стояла гласная.",
                    Form = AnswerForm.ShortText,
                    AcceptedAnswers = new[] { entry.Letter.ToString() },
                    InputMaxLength = 1,
                    Policy = LetterPolicy,
                    Explanation = $"{entry.Sentence.Replace("_", entry.Letter.ToString())} Проверка: {entry.CheckForm}.",
                }, entry.EntryId);
            }
        }
    }

    // R34-03: непроизносимая согласная.
    private sealed class R3403 : LetterInSentenceBase
    {
        protected override string TemplateId => "Russian.R34-03";
        protected override int VariantsCount => RussianOrphograms.SilentConsonants.Length;

        protected override IEnumerable<(SchoolTask, string)> Variants()
        {
            foreach (var entry in RussianOrphograms.SilentConsonants)
            {
                yield return (new SchoolTask
                {
                    Subject = Subject,
                    TemplateId = TemplateId,
                    Prompt = $"Вставь букву: {entry.Sentence}",
                    Hint = "Проверь непроизносимую согласную: подбери форму слова, где она хорошо слышна.",
                    Form = AnswerForm.ShortText,
                    AcceptedAnswers = new[] { entry.Letter.ToString() },
                    InputMaxLength = 1,
                    Policy = LetterPolicy,
                    Explanation = $"{entry.Sentence.Replace("_", entry.Letter.ToString())} Проверочное слово: {entry.CheckWord}.",
                }, entry.EntryId);
            }
        }
    }

    // R34-04: разделительные Ь/Ъ — две равновероятные ветки.
    private sealed class R3404 : RussianTemplateBase
    {
        public override string Id => "Russian.R34-04";

        public override SchoolTask Build(Random rng, out string key)
        {
            var useHard = rng.Next(2) == 0;
            var bank = useHard ? RussianOrphograms.HardSeparators : RussianOrphograms.SoftSeparators;
            var entry = Pick(rng, bank);
            return Make(useHard, entry, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var entry in RussianOrphograms.SoftSeparators)
            {
                yield return (Make(false, entry, out var key), key);
            }

            foreach (var entry in RussianOrphograms.HardSeparators)
            {
                yield return (Make(true, entry, out var key), key);
            }
        }

        private static SchoolTask Make(bool hardBranch, RussianOrphograms.SeparatorEntry entry, out string key)
        {
            key = $"{(hardBranch ? "hard" : "soft")}|{entry.EntryId}";
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R34-04",
                Prompt = $"Вставь букву: {entry.Sentence}",
                Hint = hardBranch
                    ? "Разделительный Ъ пишется после приставки на согласную перед буквами е, ё, ю, я."
                    : "Разделительный Ь пишется в корне перед буквами е, ё, ю, я, и.",
                Form = AnswerForm.ShortText,
                AcceptedAnswers = new[] { entry.Letter.ToString() },
                InputMaxLength = 1,
                Policy = LetterPolicy,
                Explanation = entry.Sentence.Replace("_", entry.Letter.ToString()),
            };
        }
    }

    // R34-05: части речи в предложении — выбор всех слов нужной части речи.
    private sealed class R3405 : RussianTemplateBase
    {
        public override string Id => "Russian.R34-05";

        public override SchoolTask Build(Random rng, out string key)
        {
            var sentence = Pick(rng, RussianFrames.PosSentences);
            var target = Pick(rng, sentence.Pos.Distinct().ToList());
            return Make(sentence, target, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var sentence in RussianFrames.PosSentences)
            {
                foreach (var target in sentence.Pos.Distinct())
                {
                    var task = Make(sentence, target, out var key);
                    yield return (task, key);
                }
            }
        }

        private static SchoolTask Make(RussianFrames.PosSentence sentence, PartOfSpeech target, out string key)
        {
            key = $"{sentence.FrameId}|{target}";
            var tokens = new List<TaskToken>();
            for (var i = 0; i < sentence.Words.Length; i++)
            {
                var text = sentence.Words[i] + (i == sentence.Words.Length - 1 ? "." : "");
                tokens.Add(new TaskToken($"t{i}", text, sentence.Pos[i], "member"));
            }

            var expected = tokens.Where(t => t.Pos == target).Select(t => t.TokenId).ToHashSet();
            var targetName = target switch
            {
                PartOfSpeech.Noun => "существительные",
                PartOfSpeech.Adjective => "прилагательные",
                _ => "глаголы",
            };

            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R34-05",
                Prompt = $"Выбери ВСЕ {targetName} (щелчком по словам):",
                Hint = "Существительное — «кто? что?», прилагательное — «какой?», глагол — «что делает?».",
                Form = AnswerForm.TokenSelection,
                Tokens = tokens,
                ExpectedTokenIds = expected,
            };
        }
    }

    // R34-06: род существительного.
    private sealed class R3406 : RussianTemplateBase
    {
        public override string Id => "Russian.R34-06";

        public override SchoolTask Build(Random rng, out string key)
        {
            var entry = Pick(rng, RussianFrames.Genders);
            return Make(entry, rng, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var entry in RussianFrames.Genders)
            {
                yield return (Make(entry, new Random(entry.LexemeRef.GetHashCode()), out var key), key);
            }
        }

        private static SchoolTask Make(RussianFrames.GenderEntry entry, Random rng, out string key)
        {
            key = $"{entry.LexemeRef}";
            var options = ShuffledOptions(rng, ("m", "мужского"), ("f", "женского"), ("n", "среднего"));
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R34-06",
                Prompt = $"Какого рода слово «{entry.Lemma}»?",
                Hint = "Подставь слова «мой» (м. р.), «моя» (ж. р.), «моё» (с. р.).",
                Form = AnswerForm.Choice,
                Options = options,
                CorrectOptionId = entry.Gender switch
                {
                    Gender.Masculine => "m",
                    Gender.Feminine => "f",
                    _ => "n",
                },
                Policy = LetterPolicy,
                Explanation = $"«{entry.Lemma}» — {entry.Gender.GenitiveName()} рода.",
            };
        }
    }

    // R34-07: число существительного в контексте (согласованный глагол снимает неоднозначность).
    private sealed class R3407 : RussianTemplateBase
    {
        public override string Id => "Russian.R34-07";

        public override SchoolTask Build(Random rng, out string key)
        {
            var entry = Pick(rng, RussianFrames.NumberSentences);
            var plural = rng.Next(2) == 1;
            return Make(entry, plural, rng, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var entry in RussianFrames.NumberSentences)
            {
                yield return (Make(entry, false, new Random((entry.FrameId + "sg").GetHashCode()), out var key1), key1);
                yield return (Make(entry, true, new Random((entry.FrameId + "pl").GetHashCode()), out var key2), key2);
            }
        }

        private static SchoolTask Make(RussianFrames.NumberSentence entry, bool plural, Random rng, out string key)
        {
            key = $"{entry.FrameId}|{(plural ? "pl" : "sg")}";
            var sentence = plural ? entry.SentencePl : entry.SentenceSg;
            var word = plural ? entry.NounPl : entry.NounSg;
            var options = ShuffledOptions(rng, ("sg", "единственное"), ("pl", "множественное"));
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R34-07",
                Prompt = $"Какого числа слово «{word}»? {sentence}.",
                Hint = "Посмотри на слово рядом: глагол подсказывает число.",
                Form = AnswerForm.Choice,
                Options = options,
                CorrectOptionId = plural ? "pl" : "sg",
                Policy = LetterPolicy,
                Explanation = $"{sentence}.",
            };
        }
    }

    // R34-08: склонение существительного.
    private sealed class R3408 : RussianTemplateBase
    {
        public override string Id => "Russian.R34-08";

        public override SchoolTask Build(Random rng, out string key)
        {
            var entry = Pick(rng, RussianFrames.Declensions);
            return Make(entry, rng, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var entry in RussianFrames.Declensions)
            {
                yield return (Make(entry, new Random(entry.LexemeRef.GetHashCode()), out var key), key);
            }
        }

        private static SchoolTask Make(RussianFrames.DeclensionEntry entry, Random rng, out string key)
        {
            key = $"{entry.LexemeRef}";
            var options = ShuffledOptions(rng,
                ("d1", "1-е склонение"), ("d2", "2-е склонение"), ("d3", "3-е склонение"));
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R34-08",
                Prompt = $"К какому склонению относится слово «{entry.Lemma}»?",
                Hint = "1-е — м. р. и ж. р. на -а/-я; 2-е — м. р. и с. р. на -о/-е; 3-е — ж. р. на -ь.",
                Form = AnswerForm.Choice,
                Options = options,
                CorrectOptionId = $"d{entry.Declension}",
                Policy = LetterPolicy,
                Explanation = $"«{entry.Lemma}» — {entry.Declension}-е склонение.",
            };
        }
    }

    // R34-09: падеж в контексте (падеж задаётся конструкцией управления).
    private sealed class R3409 : RussianTemplateBase
    {
        public override string Id => "Russian.R34-09";

        public override SchoolTask Build(Random rng, out string key)
        {
            var combo = CompatibleCombos().ToList();
            var (frame, noun, number) = combo[rng.Next(combo.Count)];
            return Make(frame, noun, number, rng, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var (frame, noun, number) in CompatibleCombos())
            {
                yield return (Make(frame, noun, number,
                    new Random((frame.FrameId + noun.LexemeId).GetHashCode()), out var key), key);
            }
        }

        internal static IEnumerable<(RussianFrames.GovFrame Frame, NounLexeme Noun, NumberRu Number)> CompatibleCombos()
        {
            foreach (var frame in RussianFrames.GovernmentFrames)
            {
                if (frame.FrameId == "gf-rastut")
                {
                    continue; // множественное «растут» — шаблон R56-01; здесь контекст единственного/мн. с управлением
                }

                foreach (var noun in RussianLexicon.Nouns.Where(n => frame.SemClasses.Contains(n.SemClass)))
                {
                    yield return (frame, noun, NumberRu.Singular);
                    yield return (frame, noun, NumberRu.Plural);
                }
            }
        }

        private static SchoolTask Make(RussianFrames.GovFrame frame, NounLexeme noun, NumberRu number, Random rng, out string key)
        {
            key = $"{frame.FrameId}|{noun.LexemeId}|{number}";
            var form = noun.Form(frame.Case, number);
            var sentence = $"{frame.Prefix}{form}{frame.Suffix}";
            var options = ShuffledOptions(rng,
                ("case-nom", "именительный"), ("case-gen", "родительный"), ("case-dat", "дательный"),
                ("case-acc", "винительный"), ("case-ins", "творительный"), ("case-prep", "предложный"));
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R34-09",
                Prompt = $"Определи падеж слова «{form}»: {sentence}",
                Hint = "Поставь вопрос к слову от глагола или предлога.",
                Form = AnswerForm.Choice,
                Options = options,
                CorrectOptionId = CaseOptionId(frame.Case),
                Policy = LetterPolicy,
                Explanation = $"{sentence} — {frame.Case.Name()} падеж.",
            };
        }

        internal static string CaseOptionId(CaseRu c) => c switch
        {
            CaseRu.Nominative => "case-nom",
            CaseRu.Genitive => "case-gen",
            CaseRu.Dative => "case-dat",
            CaseRu.Accusative => "case-acc",
            CaseRu.Instrumental => "case-ins",
            _ => "case-prep",
        };
    }

    // R34-10: подлежащее или сказуемое — ровно одно слово.
    private sealed class R3410 : RussianTemplateBase
    {
        public override string Id => "Russian.R34-10";

        public override SchoolTask Build(Random rng, out string key)
        {
            var sentence = Pick(rng, RussianFrames.RoleSentences);
            var askSubject = rng.Next(2) == 0;
            return Make(sentence, askSubject, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var sentence in RussianFrames.RoleSentences)
            {
                yield return (Make(sentence, true, out var key1), key1);
                yield return (Make(sentence, false, out var key2), key2);
            }
        }

        private static SchoolTask Make(RussianFrames.RoleSentence sentence, bool askSubject, out string key)
        {
            key = $"{sentence.FrameId}|{(askSubject ? "subj" : "pred")}";
            var tokens = new List<TaskToken>();
            for (var i = 0; i < sentence.Words.Length; i++)
            {
                var role = i == sentence.SubjectIndex ? "subject"
                    : i == sentence.PredicateIndex ? "predicate"
                    : "other";
                var text = sentence.Words[i] + (i == sentence.Words.Length - 1 ? "." : "");
                tokens.Add(new TaskToken($"t{i}", text, sentence.Pos[i], role));
            }

            var expectedId = askSubject ? $"t{sentence.SubjectIndex}" : $"t{sentence.PredicateIndex}";
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R34-10",
                Prompt = askSubject
                    ? "Выбери ОДНО слово — подлежащее (кто? что?):"
                    : "Выбери ОДНО слово — сказуемое (что делает?):",
                Hint = askSubject
                    ? "Подлежащее отвечает на вопрос «кто?» или «что?»."
                    : "Сказуемое отвечает на вопрос «что делает?».",
                Form = AnswerForm.TokenSelection,
                Tokens = tokens,
                ExpectedTokenIds = new HashSet<string> { expectedId },
            };
        }
    }
}
