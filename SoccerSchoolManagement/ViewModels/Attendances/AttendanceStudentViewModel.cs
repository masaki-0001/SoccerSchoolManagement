using System.ComponentModel.DataAnnotations;

namespace SoccerSchoolManagement.ViewModels.Attendances;

public class AttendanceStudentViewModel
{
    public int StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string Kana { get; set; } = string.Empty;

    public string Grade { get; set; } = string.Empty;

    public int? JerseyNumber { get; set; }

    [Required(ErrorMessage = "出欠状況は必須です。")]
    [StringLength(20,ErrorMessage = "出欠状況は20文字以内で入力してください。")]
    public string Status { get; set; } = "未確認";

    [StringLength(100,ErrorMessage = "欠席理由は100文字以内で入力してください。")]
    public string? Reason { get; set; }

    [StringLength(100,ErrorMessage = "備考は100文字以内で入力してください。")]
    public string? Note { get; set; }
}