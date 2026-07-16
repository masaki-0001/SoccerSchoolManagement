using System.ComponentModel.DataAnnotations;

namespace SoccerSchoolManagement.ViewModels.Classes;

public class SoccerClassCreateViewModel : IValidatableObject
{
    [Required(ErrorMessage = "クラス名は必須です。")]
    [StringLength(50,ErrorMessage = "クラス名は50文字以内で入力してください。")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "対象学年は必須です。")]
    [StringLength(50,ErrorMessage = "対象学年は50文字以内で入力してください。")]
    public string TargetGrade { get; set; } = string.Empty;

    [Required(ErrorMessage = "曜日は必須です。")]
    [StringLength(10,ErrorMessage = "曜日は10文字以内で入力してください。")]
    public string DayOfWeek { get; set; } = string.Empty;

    [Required(ErrorMessage = "開始時刻は必須です。")]
    [DataType(DataType.Time)]
    public TimeSpan? StartTime { get; set; }

    [Required(ErrorMessage = "終了時刻は必須です。")]
    [DataType(DataType.Time)]
    public TimeSpan? EndTime { get; set; }

    [Required(ErrorMessage = "会場名は必須です。")]
    [StringLength(50,ErrorMessage = "会場名は50文字以内で入力してください。")]
    public string VenueName { get; set; } = string.Empty;

    [Required(ErrorMessage = "担当者名は必須です。")]
    [StringLength(30,ErrorMessage = "担当者名は30文字以内で入力してください。")]
    public string CoachName { get; set; } = string.Empty;

    [StringLength(100,ErrorMessage = "備考は100文字以内で入力してください。")]
    public string? Note { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(DayOfWeek)
            && !SoccerClassFormOptions.DaysOfWeek.Contains(DayOfWeek))
        {
            yield return new ValidationResult(
                "曜日の値が正しくありません。",
                new[] { nameof(DayOfWeek) });
        }

        if (StartTime.HasValue
            && EndTime.HasValue
            && EndTime.Value <= StartTime.Value)
        {
            yield return new ValidationResult(
                "終了時刻は開始時刻より後にしてください。",
                new[] { nameof(EndTime) });
        }
    }
}