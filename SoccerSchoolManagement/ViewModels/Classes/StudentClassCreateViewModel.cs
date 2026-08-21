using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SoccerSchoolManagement.ViewModels.Classes;

public class StudentClassCreateViewModel:IValidatableObject
{
    public int ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string? Keyword { get; set; }

    [Required(ErrorMessage = "生徒を選択してください。")]
    public int? StudentId { get; set; }

    [Required(ErrorMessage = "所属開始日は必須です。")]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; } = DateTime.Today;

    [StringLength(100,ErrorMessage = "備考は100文字以内で入力してください。")]
    public string? Note { get; set; }

    public List<SelectListItem> StudentOptions { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && StartDate.Value.Date > DateTime.Today)
        {
            yield return new ValidationResult("所属開始日に未来日は指定できません。", new[] { nameof(StartDate) });
        }
    }
}