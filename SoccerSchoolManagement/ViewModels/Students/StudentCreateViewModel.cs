using System.ComponentModel.DataAnnotations;

namespace SoccerSchoolManagement.ViewModels.Students;

public class StudentCreateViewModel : IValidatableObject
{
    private static readonly string[] ValidStatuses =
    {
        "在籍中",
        "休会中",
        "退会済み"
    };

    [Required(ErrorMessage = "氏名は必須です。")]
    [StringLength(30, ErrorMessage = "氏名は30文字以内で入力してください。")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "ふりがなは必須です。")]
    [StringLength(30, ErrorMessage = "ふりがなは30文字以内で入力してください。")]
    public string Kana { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "生年月日は必須です。")]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [Required(ErrorMessage = "学年は必須です。")]
    [StringLength(20, ErrorMessage = "学年は20文字以内で入力してください。")]
    public string Grade { get; set; } = string.Empty;

    [Required(ErrorMessage = "性別は必須です。")]
    [StringLength(10, ErrorMessage = "性別は10文字以内で入力してください。")]
    public string Gender { get; set; } = string.Empty;

    public int? JerseyNumber { get; set; }

    [DataType(DataType.Date)]
    public DateTime? JoinedAt { get; set; }

    [Required(ErrorMessage = "在籍状況は必須です。")]
    [StringLength(20, ErrorMessage = "在籍状況は20文字以内で入力してください。")]
    public string Status { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? WithdrawnAt { get; set; }
    
    [Required(ErrorMessage = "保護者氏名は必須です。")]        
    [StringLength(30, ErrorMessage = "保護者氏名は30文字以内で入力してください。")]
    public string GuardianName { get; set; } = string.Empty;

    [Required(ErrorMessage = "保護者続柄は必須です。")]
    [StringLength(20, ErrorMessage = "保護者続柄は20文字以内で入力してください。")]
    public string GuardianRelationship { get; set; } = string.Empty;

    [Required(ErrorMessage = "保護者電話番号は必須です。")]
    [StringLength(20, ErrorMessage = "保護者電話番号は20文字以内で入力してください。")]
    public string GuardianPhone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "メールアドレスの形式が正しくありません。")]
    [StringLength(254, ErrorMessage = "メールアドレスは254文字以内で入力してください。")]
    public string? GuardianEmail { get; set; }

    public string? Note { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Status)
            && !ValidStatuses.Contains(Status))
        {
            yield return new ValidationResult(
                "在籍状況の値が正しくありません。",
                new[] { nameof(Status) });
        }

        if (Status == "退会済み" && !WithdrawnAt.HasValue)
        {
            yield return new ValidationResult(
                "退会済みの場合は退会日を入力してください。",
                new[] { nameof(WithdrawnAt) });
        }
    }
}