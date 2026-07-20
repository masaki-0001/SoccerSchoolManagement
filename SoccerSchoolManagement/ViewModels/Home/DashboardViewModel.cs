namespace SoccerSchoolManagement.ViewModels.Home;

public class DashboardViewModel
{
    public int ActiveStudentCount { get; set; }

    public int UnpaidCount { get; set; }

    public decimal UnpaidTotalAmount { get; set; }

    public List<DashboardLessonViewModel> TodayLessons { get; set; } = new List<DashboardLessonViewModel>();
}