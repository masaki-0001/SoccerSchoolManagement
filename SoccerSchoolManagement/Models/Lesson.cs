using System.ComponentModel.DataAnnotations;

namespace SoccerSchoolManagement.Models;

public class Lesson
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public SoccerClass SoccerClass { get; set; } = null!;

    [DataType(DataType.Date)]
    public DateTime LessonDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    [Required(ErrorMessage = "会場名は必須です。")]
    [StringLength(50, ErrorMessage = "会場名は50文字以内で入力してください。")]
    public string VenueName { get; set; } = string.Empty;

    [Required(ErrorMessage = "担当者名は必須です。")]
    [StringLength(30, ErrorMessage = "担当者名は30文字以内で入力してください。")]
    public string CoachName { get; set; } = string.Empty;

    [Required(ErrorMessage = "開催状況は必須です。")]
    [StringLength(20, ErrorMessage = "開催状況は20文字以内で入力してください。")]
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    [StringLength(100, ErrorMessage = "備考は100文字以内で入力してください。")]
    public string? Note { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}