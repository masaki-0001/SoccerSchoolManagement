using SoccerSchoolManagement.Models;

namespace SoccerSchoolManagement.ViewModels.Lessons;

public class LessonIndexViewModel
{
    public int Year { get; set; }

    public int Month { get; set; }

    public List<Lesson> Lessons { get; set; } = new();
}