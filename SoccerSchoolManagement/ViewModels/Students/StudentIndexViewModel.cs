using Microsoft.AspNetCore.Mvc.Rendering;
using SoccerSchoolManagement.Models;

namespace SoccerSchoolManagement.ViewModels.Students;

public class StudentIndexViewModel
{
    public string? Keyword { get; set; }

    public string GradeFilter { get; set; } = "すべて";

    public string StatusFilter { get; set; } = "すべて";

    public int? ClassFilter { get; set; }

    public List<SelectListItem> ClassOptions { get; set; } = new();

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