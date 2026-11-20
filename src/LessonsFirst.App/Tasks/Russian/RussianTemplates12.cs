namespace LessonsFirst.App.Tasks.RussianTasks;

/// <summary>
/// Уровень «1–2 класс — простой»: Russian.R12-01…Russian.R12-10.
/// Короткие знакомые слова (2…10 букв), предложения 2…6 слов; без фонетического
/// разбора и подсчёта звуков. Подсчёты — только по буквам (дефисы, пробелы,
/// цифры и знак ударения буквой не считаются).
/// </summary>
internal static class RussianTemplates12
{
    public static readonly IRussianTemplate[] All =
    {
        new R1201(), new R1202(), new R1203(), new R1204(), new R1205(),
        new R1206(), new R1207(), new R1208(), new R1209(), new R1210(),
    };

    private sealed class R1201 : RussianTemplateBase
    {
        public override string Id => "Russian.R12-01";

        public override SchoolTask Build(Random rng, out string key)
        {
            var word = Pick(rng, RussianOrphograms.SimpleWords);
            return Make(word, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var word in RussianOrphograms.SimpleWords)
            {
                var task = Make(word, out var key);
                yield return (task, key);
            }
        }

        private static SchoolTask Make(RussianOrphograms.SimpleWord word, out string key)
        {
            key = $"{word.WordId}";
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R12-01",
                Prompt = $"Сколько букв в слове «{word.Text}»?",
                Hint = "Посчитай все буквы, включая ь и ъ. Знак ударения буквой не является.",
                Form = AnswerForm.Integer,
                ExpectedInteger = RussianTextUtil.CountLetters(word.Text),
            };
        }
    }

    private sealed class R1202 : RussianTemplateBase
    {
        public override string Id => "Russian.R12-02";

        public override SchoolTask Build(Random rng, out string key)
        {
            var word = Pick(rng, RussianOrphograms.SimpleWords);
            return Make(word, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var word in RussianOrphograms.SimpleWords)
            {
                var task = Make(word, out var key);
                yield return (task, key);
            }
        }

        private static SchoolTask Make(RussianOrphograms.SimpleWord word, out string key)
        {
            key = $"{word.WordId}";
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R12-02",
                Prompt = $"Сколько гласных букв в слове «{word.Text}»?",
                Hint = "Гласные буквы: а, е, ё, и, о, у, ы, э, ю, я.",
                Form = AnswerForm.Integer,
                ExpectedInteger = RussianTextUtil.CountVowels(word.Text),
            };
        }
    }

    private sealed class R1203 : RussianTemplateBase
    {
        public override string Id => "Russian.R12-03";

        public override SchoolTask Build(Random rng, out string key)
        {
            var word = Pick(rng, RussianOrphograms.SimpleWords.Where(w => w.Syllables is >= 1 and <= 4).ToList());
            return Make(word, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var word in RussianOrphograms.SimpleWords.Where(w => w.Syllables is >= 1 and <= 4))
            {
                var task = Make(word, out var key);
                yield return (task, key);
            }
        }

        private static SchoolTask Make(RussianOrphograms.SimpleWord word, out string key)
        {
            key = $"{word.WordId}";
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R12-03",
                Prompt = $"Сколько слогов в слове «{word.Text}»?",
                Hint = "Сколько в слове гласных — столько и слогов.",
                Form = AnswerForm.Integer,
                ExpectedInteger = word.Syllables,
                Explanation = $"Гласных букв в слове — {word.Syllables}.",
            };
        }
    }

    private sealed class R1204 : RussianTemplateBase
    {
        public override string Id => "Russian.R12-04";

        public override SchoolTask Build(Random rng, out string key)
        {
            // Перемешанный банк; берём слова с разными первыми буквами (не сортировка е/ё).
            var pool = RussianOrphograms.SimpleWords.ToList();
            Shuffle(pool, rng);

            var chosen = new List<RussianOrphograms.SimpleWord>();
            var firstLetters = new HashSet<char>();
            foreach (var word in pool)
            {
                if (firstLetters.Add(char.ToLowerInvariant(word.Text[0])))
                {
                    chosen.Add(word);
                    if (chosen.Count == 4)
                    {
                        break;
                    }
                }
            }

            return Make(chosen, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            var words = RussianOrphograms.SimpleWords;
            for (var a = 0; a < words.Length; a++)
            {
                for (var b = a + 1; b < words.Length; b++)
                {
                    if (SameFirst(words[a], words[b]))
                    {
                        continue;
                    }

                    for (var c = b + 1; c < words.Length; c++)
                    {
                        if (SameFirst(words[a], words[c]) || SameFirst(words[b], words[c]))
                        {
                            continue;
                        }

                        for (var d = c + 1; d < words.Length; d++)
                        {
                            if (SameFirst(words[a], words[d]) || SameFirst(words[b], words[d]) || SameFirst(words[c], words[d]))
                            {
                                continue;
                            }

                            var task = Make(
                                new List<RussianOrphograms.SimpleWord> { words[a], words[b], words[c], words[d] },
                                out var key);
                            yield return (task, key);
                        }
                    }
                }
            }
        }

        private static bool SameFirst(RussianOrphograms.SimpleWord x, RussianOrphograms.SimpleWord y) =>
            char.ToLowerInvariant(x.Text[0]) == char.ToLowerInvariant(y.Text[0]);

        private static SchoolTask Make(IReadOnlyList<RussianOrphograms.SimpleWord> chosen, out string key)
        {
            // Первое по алфавиту — по явному алфавиту из 33 букв (ё после е).
            var first = chosen.OrderBy(w => RussianTextUtil.AlphabetIndex(char.ToLowerInvariant(w.Text[0]))).First();
            var ids = chosen.Select(w => w.WordId).OrderBy(x => x, StringComparer.Ordinal).ToList();
            key = string.Join("|", ids);

            var options = ShuffledOptions(
                new Random(key.GetHashCode()),
                chosen.Select(w => (w.WordId, w.Text)).ToArray());

            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R12-04",
                Prompt = $"Какое слово идёт первым по алфавиту: {string.Join(", ", chosen.Select(w => w.Text))}?",
                Hint = "Сравни первые буквы слов по алфавиту: а, б, в, г, д, е, ё, ж…",
                Form = AnswerForm.Choice,
                Options = options,
                CorrectOptionId = first.WordId,
                Policy = CaseSensitivePolicy,
                Explanation = $"По алфавиту раньше других идёт «{first.Text}».",
            };
        }
    }

    private abstract class MaskedRuleBase : RussianTemplateBase
    {
        public override string Id => TemplateId;

        protected abstract string TemplateId { get; }

        protected abstract IReadOnlyList<RussianOrphograms.MaskedWord> Bank { get; }

        public override SchoolTask Build(Random rng, out string key)
        {
            var entry = Pick(rng, Bank);
            return Make(entry, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var entry in Bank)
            {
                var task = Make(entry, out var key);
                yield return (task, key);
            }
        }

        private SchoolTask Make(RussianOrphograms.MaskedWord entry, out string key)
        {
            key = $"{entry.EntryId}";
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = TemplateId,
                Prompt = $"Вставь букву: {RussianTextUtil.MaskedWord(entry.Word, entry.MaskIndex)}",
                Hint = HintText,
                Form = AnswerForm.ShortText,
                AcceptedAnswers = new[] { entry.Letter.ToString() },
                InputMaxLength = 1,
                Policy = LetterPolicy,
                Explanation = $"Полное слово: {entry.Word}.",
            };
        }

        protected abstract string HintText { get; }
    }

    private sealed class R1205 : MaskedRuleBase
    {
        protected override string TemplateId => "Russian.R12-05";
        protected override IReadOnlyList<RussianOrphograms.MaskedWord> Bank => RussianOrphograms.ZhiShi;
        protected override string HintText => "ЖИ–ШИ пиши с буквой И.";
    }

    private sealed class R1206 : MaskedRuleBase
    {
        protected override string TemplateId => "Russian.R12-06";
        protected override IReadOnlyList<RussianOrphograms.MaskedWord> Bank => RussianOrphograms.ChaSha;
        protected override string HintText => "ЧА–ЩА пиши с буквой А.";
    }

    private sealed class R1207 : MaskedRuleBase
    {
        protected override string TemplateId => "Russian.R12-07";
        protected override IReadOnlyList<RussianOrphograms.MaskedWord> Bank => RussianOrphograms.ChuShu;
        protected override string HintText => "ЧУ–ЩУ пиши с буквой У.";
    }

    private sealed class R1208 : RussianTemplateBase
    {
        public override string Id => "Russian.R12-08";

        public override SchoolTask Build(Random rng, out string key)
        {
            var frame = Pick(rng, RussianFrames.CapSentences);
            return Make(frame, rng, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var frame in RussianFrames.CapSentences)
            {
                var task = Make(frame, new Random(frame.FrameId.GetHashCode()), out var key);
                yield return (task, key);
            }
        }

        private static SchoolTask Make(RussianFrames.CapFrame frame, Random rng, out string key)
        {
            key = $"{frame.FrameId}";
            var options = ShuffledOptions(rng, ("cap-up", frame.Upper + "."), ("cap-low", frame.Lower + "."));
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R12-08",
                Prompt = "Какое предложение записано правильно?",
                Hint = "Предложение начинается с заглавной буквы.",
                Form = AnswerForm.Choice,
                Options = options,
                CorrectOptionId = "cap-up",
                Policy = CaseSensitivePolicy,
                Explanation = $"Правильно: {frame.Upper}.",
            };
        }
    }

    private sealed class R1209 : RussianTemplateBase
    {
        public override string Id => "Russian.R12-09";

        public override SchoolTask Build(Random rng, out string key)
        {
            var frame = Pick(rng, RussianFrames.NameSentences);
            var name = Pick(rng, RussianLexicon.Names);
            return Make(frame, name, rng, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            foreach (var frame in RussianFrames.NameSentences)
            {
                foreach (var name in RussianLexicon.Names)
                {
                    var task = Make(frame, name, new Random((frame.FrameId + name).GetHashCode()), out var key);
                    yield return (task, key);
                }
            }
        }

        private static SchoolTask Make(RussianFrames.NameFrame frame, string name, Random rng, out string key)
        {
            key = $"{frame.FrameId}|{name.ToLowerInvariant()}";
            var lower = name.ToLowerInvariant();
            var options = ShuffledOptions(rng, ("name-up", name), ("name-low", lower));
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R12-09",
                Prompt = $"Как правильно? {frame.Before}…{frame.After}",
                Hint = "Имена людей пишутся с заглавной буквы.",
                Form = AnswerForm.Choice,
                Options = options,
                CorrectOptionId = "name-up",
                Policy = CaseSensitivePolicy,
                Explanation = $"Правильно: {name}.",
            };
        }
    }

    private sealed class R1210 : RussianTemplateBase
    {
        public override string Id => "Russian.R12-10";

        private static readonly string[] BranchNames = { "предмет", "действие", "признак" };

        public override SchoolTask Build(Random rng, out string key)
        {
            var branch = rng.Next(3);
            var noun = Pick(rng, RussianFrames.ObjectNouns);
            var verb = Pick(rng, RussianFrames.ActionVerbs);
            var adjective = Pick(rng, RussianFrames.QualityAdjectives);
            return Make(branch, noun, verb, adjective, rng, out key);
        }

        public override IEnumerable<(SchoolTask, string)> AllVariants()
        {
            for (var branch = 0; branch < 3; branch++)
            {
                foreach (var noun in RussianFrames.ObjectNouns)
                {
                    foreach (var verb in RussianFrames.ActionVerbs)
                    {
                        foreach (var adjective in RussianFrames.QualityAdjectives)
                        {
                            var task = Make(branch, noun, verb, adjective,
                                new Random((branch + noun + verb + adjective).GetHashCode()), out var key);
                            yield return (task, key);
                        }
                    }
                }
            }
        }

        private static SchoolTask Make(int branch, string noun, string verb, string adjective, Random rng, out string key)
        {
            key = $"{BranchNames[branch]}|{noun}|{verb}|{adjective}";

            var triple = new List<(string Id, string Text)> { ("noun", noun), ("verb", verb), ("adj", adjective) };
            var display = triple.Select(t => t.Text).ToList();
            Shuffle(display, rng);
            var options = ShuffledOptions(rng, triple.ToArray());
            return new SchoolTask
            {
                Subject = Subject,
                TemplateId = "Russian.R12-10",
                Prompt = $"Какое слово обозначает {BranchNames[branch]}: {string.Join(", ", display)}?",
                Hint = branch switch
                {
                    0 => "Предмет отвечает на вопрос «что?» или «кто?».",
                    1 => "Действие отвечает на вопрос «что делает?».",
                    _ => "Признак отвечает на вопрос «какой?».",
                },
                Form = AnswerForm.Choice,
                Options = options,
                CorrectOptionId = branch switch { 0 => "noun", 1 => "verb", _ => "adj" },
                Policy = LetterPolicy,
            };
        }
    }
}
