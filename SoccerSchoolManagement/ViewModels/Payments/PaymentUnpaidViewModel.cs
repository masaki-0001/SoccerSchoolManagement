using SoccerSchoolManagement.Models;

namespace SoccerSchoolManagement.ViewModels.Payments;

public class PaymentUnpaidViewModel
{
    public List<Payment> Payments { get; set; } = new();

    public int UnpaidCount
    {
        get
        {
            return Payments.Count;
        }
    }

    public decimal TotalAmount
    {
        get
        {
            return Payments.Sum(payment => payment.Amount);
        }
    }
}