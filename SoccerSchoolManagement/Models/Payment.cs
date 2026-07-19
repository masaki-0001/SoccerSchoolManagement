using System.ComponentModel.DataAnnotations;

namespace SoccerSchoolManagement.Models;

public class Payment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    [Range(2000, 2100, ErrorMessage = "対象年は2000年から2100年の範囲で入力してください。")]
    public int TargetYear { get; set; }

    [Range(1, 12, ErrorMessage = "対象月は1月から12月の範囲で入力してください。")]
    public int TargetMonth { get; set; }

    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "請求額は0円以上で入力してください。")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "支払状況は必須です。")]
    [StringLength(20, ErrorMessage = "支払状況は20文字以内で入力してください。")]
    public string Status { get; set; } = "未払い";

    [DataType(DataType.Date)]
    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    [StringLength(100, ErrorMessage = "備考は100文字以内で入力してください。")]
    public string? Note { get; set; }
}