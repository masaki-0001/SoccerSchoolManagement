using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SoccerSchoolManagement.Models;

namespace SoccerSchoolManagement.ViewModels.Payments;

public class PaymentIndexViewModel
{
    [Range(2000,2100,ErrorMessage = "対象年は2000年から2100年の範囲で入力してください。")]
    public int Year { get; set; }

    [Range(1,12,ErrorMessage = "対象月は1月から12月の範囲で入力してください。")]
    public int Month { get; set; }

    [Required(ErrorMessage = "生徒を選択してください。")]
    public int? StudentId { get; set; }

    [Required(ErrorMessage = "請求額は必須です。")]
    [Range(typeof(decimal),"0","999999999",ErrorMessage = "請求額は0円以上で入力してください。")]
    public decimal? Amount { get; set; }

    [StringLength(100,ErrorMessage = "備考は100文字以内で入力してください。")]
    public string? Note { get; set; }

    public List<SelectListItem> StudentOptions { get; set; } = new();

    public List<Payment> Payments { get; set; } = new();

    public int CurrentPage { get; set; } = 1;

    public int TotalPages { get; set; }

    public int TotalCount { get; set; }

    public bool HasPreviousPage
    {
        get
        {
            return CurrentPage > 1;
        }
    }

    public bool HasNextPage
    {
        get
        {
            return CurrentPage < TotalPages;
        }
    }
}