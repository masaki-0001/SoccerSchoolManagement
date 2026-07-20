namespace SoccerSchoolManagement.ViewModels.Home;

public class DashboardLessonViewModel
{
    public int Id { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string VenueName { get; set; } = string.Empty;

    public string CoachName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}