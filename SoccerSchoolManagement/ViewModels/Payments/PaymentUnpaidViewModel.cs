using SoccerSchoolManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace SoccerSchoolManagement.ViewModels.Payments;

public class PaymentUnpaidViewModel
{
    public List<Payment> Payments { get; set; } = new();

    [Range(2000, 2100, ErrorMessage = "対象年は2000年から2100年の範囲で入力してください。")]
    public int Year { get; set; }

    [Range(1, 12, ErrorMessage = "対象月は1月から12月の範囲で入力してください。")]
    public int Month { get; set; }

    public int UnpaidCount { get; set; }

    public decimal TotalAmount { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

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