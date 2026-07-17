using System.ComponentModel.DataAnnotations;

namespace SoccerSchoolManagement.ViewModels.Classes;

public class StudentClassEndViewModel
{
    public int StudentClassId { get; set; }

    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "所属終了日は必須です。")]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; } = DateTime.Today;
}