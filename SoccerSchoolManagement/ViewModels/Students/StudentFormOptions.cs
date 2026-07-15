namespace SoccerSchoolManagement.ViewModels.Students;

public static class StudentFormOptions
{
    public static IReadOnlyList<string> Grades { get; } = new[]
    {
        "年少",
        "年中",
        "年長",
        "1年生",
        "2年生",
        "3年生",
        "4年生",
        "5年生",
        "6年生"
    };

    public static IReadOnlyList<string> Genders { get; } = new[]
    {
        "男子",
        "女子"
    };

    public static IReadOnlyList<string> Statuses { get; } = new[]
    {
        "在籍中",
        "休会中",
        "退会済み"
    };

    public static IReadOnlyList<string> GuardianRelationship { get; } = new[]   
{
        "父親",
        "母親",
        "祖父母",
        "その他"
    };
}
