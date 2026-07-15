using SoccerSchoolManagement.Models;

namespace SoccerSchoolManagement.ViewModels.Students;

public class StudentIndexViewModel
{
    public string? Keyword { get; set; }

    public List<Student> Students { get; set; } = new();
}