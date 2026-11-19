namespace LessonsFirst.App.Tasks;

public interface ITaskGenerator
{
    SchoolTask Generate(Random random);
}
