using LessonsFirst.App.Tasks;
using Xunit;

namespace LessonsFirst.Tests;

public class RussianTextTests
{
    [Theory]
    [InlineData(1, "карандаш")]
    [InlineData(2, "карандаша")]
    [InlineData(4, "карандаша")]
    [InlineData(5, "карандашей")]
    [InlineData(11, "карандашей")]
    [InlineData(12, "карандашей")]
    [InlineData(14, "карандашей")]
    [InlineData(21, "карандаш")]
    [InlineData(22, "карандаша")]
    [InlineData(25, "карандашей")]
    public void Plural_PencilForms(int n, string expected) =>
        Assert.Equal(expected, RussianText.Plural(n, "карандаш", "карандаша", "карандашей"));

    [Theory]
    [InlineData(1, "шоколадка")]
    [InlineData(3, "шоколадки")]
    [InlineData(12, "шоколадок")]
    [InlineData(21, "шоколадка")]
    public void Plural_ChocolateForms(int n, string expected) =>
        Assert.Equal(expected, RussianText.Plural(n, "шоколадка", "шоколадки", "шоколадок"));

    [Theory]
    [InlineData(1, "раз")]
    [InlineData(2, "раза")]
    [InlineData(5, "раз")]
    [InlineData(11, "раз")]
    [InlineData(21, "раз")]
    public void Raz_Forms(int n, string expected) =>
        Assert.Equal(expected, RussianText.Raz(n));

    [Theory]
    [InlineData(1, "набор")]
    [InlineData(2, "набора")]
    [InlineData(5, "наборов")]
    public void Plural_SetForms(int n, string expected) =>
        Assert.Equal(expected, RussianText.Plural(n, "набор", "набора", "наборов"));
}
