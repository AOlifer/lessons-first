namespace LessonsFirst.App.Tasks;

/// <summary>Русские числительные и формы предметов для текстовых задач.</summary>
public static class RussianText
{
    /// <summary>Форма существительного для числа: один/два-четыре/много, включая 11–14.</summary>
    public static string Plural(int n, string one, string few, string many)
    {
        var lastHundred = Math.Abs(n) % 100;
        if (lastHundred is >= 11 and <= 14)
            return many;

        var lastDigit = Math.Abs(n) % 10;
        return lastDigit switch
        {
            1 => one,
            >= 2 and <= 4 => few,
            _ => many,
        };
    }

    /// <summary>«1 раз / 2 раза / 5 раз / 11 раз».</summary>
    public static string Raz(int n) => Plural(n, "раз", "раза", "раз");

    /// <summary>Предмет для счёта: (единственное, 2–4, много).</summary>
    public sealed record Countable(string One, string Few, string Many);

    public static readonly Countable Sticker = new("наклейка", "наклейки", "наклеек");
    public static readonly Countable Sweet = new("конфета", "конфеты", "конфет");
    public static readonly Countable Pencil = new("карандаш", "карандаша", "карандашей");
    public static readonly Countable Chocolate = new("шоколадка", "шоколадки", "шоколадок");
    public static readonly Countable Toy = new("игрушка", "игрушки", "игрушек");
    public static readonly Countable Set = new("набор", "набора", "наборов");

    public static string Counted(int n, Countable item) => $"{n} {Plural(n, item.One, item.Few, item.Many)}";

    /// <summary>Имена для сюжетов (женский род, «ей»).</summary>
    public static readonly string[] GirlNames = { "Маши", "Оли", "Сони", "Кати", "Даши", "Лены" };

    public static string RandomGirl(Random rng) => GirlNames[rng.Next(GirlNames.Length)];

    private static readonly Countable[] StoryItems = { Sticker, Sweet, Pencil, Toy };

    public static Countable RandomItem(Random rng) => StoryItems[rng.Next(StoryItems.Length)];
}
