using System.ComponentModel.DataAnnotations;

namespace SoccerSchoolManagement.Models;

public class SoccerClass
{
    public int Id { get; set; }

    [Required(ErrorMessage = "クラス名は必須です。")]
    [StringLength(50,ErrorMessage = "クラス名は50文字以内で入力してください。")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "対象学年は必須です。")]
    [StringLength(50,ErrorMessage = "対象学年は50文字以内で入力してください。")]
    public string TargetGrade { get; set; } = string.Empty;

    [Required(ErrorMessage = "曜日は必須です。")]
    [StringLength(10,ErrorMessage = "曜日は10文字以内で入力してください。")]
    public string DayOfWeek { get; set; } = string.Empty;

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    [Required(ErrorMessage = "会場名は必須です。")]
    [StringLength(50,ErrorMessage = "会場名は50文字以内で入力してください。")]
    public string VenueName { get; set; } = string.Empty;

    [Required(ErrorMessage = "担当者名は必須です。")]
    [StringLength(30,ErrorMessage = "担当者名は30文字以内で入力してください。")]
    public string CoachName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    [StringLength(100,ErrorMessage = "備考は100文字以内で入力してください。")]
    public string? Note { get; set; }

    public ICollection<StudentClass> StudentClasses { get; set; }
        = new List<StudentClass>();
}