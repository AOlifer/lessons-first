using LessonsFirst.App.Tasks;
using Xunit;

namespace LessonsFirst.Tests;

public class SchoolTaskTests
{
    [Fact]
    public void NumericAnswer_AcceptsLeadingZerosAndSpaces()
    {
        var task = new SchoolTask
        {
            Subject = "Математика",
            Prompt = "Сколько будет 2 + 2?",
            CorrectAnswer = "4",
            IsTextInput = true,
        };

        Assert.True(task.CheckAnswer("4"));
        Assert.True(task.CheckAnswer(" 04 "));
        Assert.False(task.CheckAnswer("5"));
        Assert.False(task.CheckAnswer(""));
        Assert.False(task.CheckAnswer(null));
    }

    [Fact]
    public void FractionAnswer_AcceptsEquivalentFractions()
    {
        var task = new SchoolTask
        {
            Subject = "Математика",
            Prompt = "3/8 + 2/8?",
            CorrectAnswer = "5/8",
            IsTextInput = true,
            IsFraction = true,
        };

        Assert.True(task.CheckAnswer("5/8"));
        Assert.True(task.CheckAnswer("10/16"));
        Assert.False(task.CheckAnswer("3/8"));
        Assert.False(task.CheckAnswer("abc"));
        Assert.False(task.CheckAnswer("5"));
    }

    [Fact]
    public void ChoiceAnswer_IsCaseInsensitive()
    {
        var task = new SchoolTask
        {
            Subject = "Русский язык",
            Prompt = "Выбери букву: к_рова",
            CorrectAnswer = "о",
            Options = new[] { "а", "о" },
            IsTextInput = false,
        };

        Assert.True(task.CheckAnswer("о"));
        Assert.True(task.CheckAnswer("О"));
        Assert.False(task.CheckAnswer("а"));
    }
}
