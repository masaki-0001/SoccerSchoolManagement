using System.ComponentModel.DataAnnotations;

namespace SoccerSchoolManagement.Models;

public class Student
{
    public int Id { get; set; }

    [Required(ErrorMessage = "氏名は必須です。")]
    [StringLength(30, ErrorMessage = "氏名は30文字以内で入力してください。")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "ふりがなは必須です。")]
    [StringLength(30, ErrorMessage = "ふりがなは30文字以内で入力してください。")]
    public string Kana { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "学年は必須です。")]
    [StringLength(20, ErrorMessage = "学年は20文字以内で入力してください。")]
    public string Grade { get; set; } = string.Empty;

    [Required(ErrorMessage = "性別は必須です。")]
    [StringLength(10, ErrorMessage = "性別は10文字以内で入力してください。")]
    public string Gender { get; set; } = string.Empty;

    [Range(0, 99, ErrorMessage = "背番号は0から99の範囲で入力してください。")]
    public int? JerseyNumber { get; set; }

    [DataType(DataType.Date)]
    public DateTime JoinedAt { get; set; }

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
    [StringLength(11, ErrorMessage = "保護者電話番号は11文字以内で入力してください。")]
    [Phone(ErrorMessage = "電話番号の形式が正しくありません。")]
    public string GuardianPhone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "メールアドレスの形式が正しくありません。")]
    [StringLength(254, ErrorMessage = "メールアドレスは254文字以内で入力してください。")]
    public string? GuardianEmail { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    [StringLength(100, ErrorMessage = "備考は100文字以内で入力してください。")]
    public string? Note { get; set; }

    public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
