using SoccerSchoolManagement.Models;

namespace SoccerSchoolManagement.ViewModels.Students;

public class StudentIndexViewModel
{
    public string? Keyword { get; set; }

    public List<Student> Students { get; set; } = new();

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int TotalCount { get; set; }

    public bool HasPreviousPage
    {
        get
        {
            return CurrentPage > 1;
        }
    }

    public bool HasNextPage
    {
        get
        {
            return CurrentPage < TotalPages;
        }
    }
}