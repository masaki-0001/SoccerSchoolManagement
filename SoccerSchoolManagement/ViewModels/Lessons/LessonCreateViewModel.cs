using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SoccerSchoolManagement.ViewModels.Lessons;

public class LessonCreateViewModel : IValidatableObject
{
    [Required(ErrorMessage = "クラスを選択してください。")]
    public int? ClassId { get; set; }

    [Required(ErrorMessage = "開催日は必須です。")]
    [DataType(DataType.Date)]
    public DateTime? LessonDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "開始時刻は必須です。")]
    [DataType(DataType.Time)]
    public TimeSpan? StartTime { get; set; }

    [Required(ErrorMessage = "終了時刻は必須です。")]
    [DataType(DataType.Time)]
    public TimeSpan? EndTime { get; set; }

    [Required(ErrorMessage = "会場名は必須です。")]
    [StringLength(
        50,
        ErrorMessage = "会場名は50文字以内で入力してください。")]
    public string VenueName { get; set; } = string.Empty;

    [Required(ErrorMessage = "担当者名は必須です。")]
    [StringLength(
        30,
        ErrorMessage = "担当者名は30文字以内で入力してください。")]
    public string CoachName { get; set; } = string.Empty;

    [Required(ErrorMessage = "開催状況は必須です。")]
    [StringLength(
        20,
        ErrorMessage = "開催状況は20文字以内で入力してください。")]
    public string Status { get; set; } = "予定";

    [StringLength(
        100,
        ErrorMessage = "備考は100文字以内で入力してください。")]
    public string? Note { get; set; }

    public List<SelectListItem> ClassOptions { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (StartTime.HasValue && EndTime.HasValue && EndTime.Value <= StartTime.Value)
        {
            yield return new ValidationResult("終了時刻は開始時刻より後にしてください。", new[] { nameof(EndTime) });
        }

        if (!string.IsNullOrWhiteSpace(Status) && !LessonFormOptions.Statuses.Contains(Status))
        {
            yield return new ValidationResult("開催状況の値が正しくありません。", new[] { nameof(Status) });
        }
    }
}