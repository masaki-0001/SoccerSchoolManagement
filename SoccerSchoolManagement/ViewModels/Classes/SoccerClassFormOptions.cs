namespace SoccerSchoolManagement.ViewModels.Classes;

public static class SoccerClassFormOptions
{
    public static IReadOnlyList<string> DaysOfWeek { get; } = new[]
    {
        "月曜日",
        "火曜日",
        "水曜日",
        "木曜日",
        "金曜日",
        "土曜日",
        "日曜日"
    };
}