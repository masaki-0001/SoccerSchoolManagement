using SoccerSchoolManagement.Models;

namespace SoccerSchoolManagement.ViewModels.Payments;

public class PaymentUnpaidViewModel
{
    public List<Payment> Payments { get; set; } = new();

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