using System.ComponentModel.DataAnnotations;

namespace SoccerSchoolManagement.ViewModels.Payments;

public class PaymentEditViewModel
{
    public int Id { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public int TargetYear { get; set; }

    public int TargetMonth { get; set; }

    [Required(ErrorMessage = "請求額は必須です。")]
    [Range(typeof(decimal),"0","999999999", ErrorMessage = "請求額は0円以上で入力してください。")]
    [DisplayFormat(DataFormatString = "{0:0}", ApplyFormatInEditMode = true)]
    public decimal? Amount { get; set; }

    [StringLength(100, ErrorMessage = "備考は100文字以内で入力してください。")]
    public string? Note { get; set; }

    public int ReturnYear { get; set; }

    public int ReturnMonth { get; set; }

    public string? Keyword { get; set; }

    public string StatusFilter { get; set; } = "すべて";

    public int Page { get; set; } = 1;
}