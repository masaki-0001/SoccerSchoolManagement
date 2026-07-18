namespace SoccerSchoolManagement.ViewModels.Attendances;

public class AttendanceEditViewModel
{
    public int LessonId { get; set; }

    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public DateTime LessonDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string VenueName { get; set; } = string.Empty;

    public string CoachName { get; set; } = string.Empty;

    public string LessonStatus { get; set; } = string.Empty;

    public List<AttendanceStudentViewModel> Students { get; set; } = new List<AttendanceStudentViewModel>();
}