using System.ComponentModel.DataAnnotations;

namespace SoccerSchoolManagement.Models;

public class Attendance
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = null!;

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    [Required(ErrorMessage = "出欠状況は必須です。")]
    [StringLength(20, ErrorMessage = "出欠状況は20文字以内で入力してください。")]
    public string Status { get; set; } = "未確認";

    [StringLength(100, ErrorMessage = "欠席理由は100文字以内で入力してください。")]
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    [StringLength(100, ErrorMessage = "備考は100文字以内で入力してください。")]
    public string? Note { get; set; }
}