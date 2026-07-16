using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SoccerSchoolManagement.ViewModels.Classes;

public class StudentClassCreateViewModel
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    [Required(ErrorMessage = "生徒を選択してください。")]
    public int? StudentId { get; set; }

    [Required(ErrorMessage = "所属開始日は必須です。")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; } = DateTime.Today;

    [StringLength(100,ErrorMessage = "備考は100文字以内で入力してください。")]
    public string? Note { get; set; }

    public List<SelectListItem> StudentOptions { get; set; } = new();
}