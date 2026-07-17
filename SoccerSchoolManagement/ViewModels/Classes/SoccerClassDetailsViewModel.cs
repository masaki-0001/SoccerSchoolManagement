using SoccerSchoolManagement.Models;

namespace SoccerSchoolManagement.ViewModels.Classes;

public class SoccerClassDetailsViewModel
{
    public SoccerClass SoccerClass { get; set; } = null!;

    public List<StudentClass> CurrentMemberships { get; set; } = new();

    public List<StudentClass> PastMemberships { get; set; } = new();

    public List<Lesson> UpcomingLessons { get; set; } = new();

}   