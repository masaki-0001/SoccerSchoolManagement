namespace SoccerSchoolManagement.ViewModels.Lessons;

public static class LessonFormOptions
{
    public static IReadOnlyList<string> Statuses { get; } = new[]
    {
        "予定",
        "実施済み",
        "中止",
        "休講"
    };
}