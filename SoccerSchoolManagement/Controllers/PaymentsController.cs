using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoccerSchoolManagement.Data;
using SoccerSchoolManagement.Models;
using SoccerSchoolManagement.ViewModels.Payments;

namespace SoccerSchoolManagement.Controllers;

public class PaymentsController : Controller
{
    private readonly AppDbContext _context;

    public PaymentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? year, int? month, int page = 1)
    {
        const int pageSize = 10;

        if (page < 1)
        {
            page = 1;
        }

        var today = DateTime.Today;

        var targetYear = year ?? today.Year;
        var targetMonth = month ?? today.Month;

        if (targetYear < 2000 || targetYear > 2100 || targetMonth < 1 || targetMonth > 12)
        {
            return BadRequest();
        }

        var query = _context.Payments
            .Where(payment => !payment.IsDeleted
                && payment.TargetYear == targetYear
                && payment.TargetMonth == targetMonth)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var payments = await query
            .Include(payment => payment.Student)
            .OrderBy(payment => payment.Student.Kana)
            .ThenBy(payment => payment.Student.Name)
            .ThenBy(payment => payment.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var studentOptions = await _context.Students
            .Where(student => !student.IsDeleted && student.Status == "在籍中")
            .OrderBy(student => student.Kana)
            .ThenBy(student => student.Name)
            .Select(student => new SelectListItem
            {
                Value = student.Id.ToString(),
                Text = student.Name
            })
            .AsNoTracking()
            .ToListAsync();

        var model = new PaymentIndexViewModel
        {
            Year = targetYear,
            Month = targetMonth,
            StudentOptions = studentOptions,
            Payments = payments,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(PaymentIndexViewModel model)
    {
        const int pageSize = 10;

        if (model.Year < 2000 || model.Year > 2100 || model.Month < 1 || model.Month > 12)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            var studentExists = await _context.Students
                .AnyAsync(student => student.Id == model.StudentId && !student.IsDeleted && student.Status == "在籍中");

            if (!studentExists)
            {
                ModelState.AddModelError(nameof(model.StudentId),"選択した生徒は月謝の登録対象ではありません。");
            }
        }

        if (ModelState.IsValid)
        {
            var paymentExists = await _context.Payments
                .AnyAsync(payment => !payment.IsDeleted
                    && payment.StudentId == model.StudentId
                    && payment.TargetYear == model.Year
                    && payment.TargetMonth == model.Month);

            if (paymentExists)
            {
                ModelState.AddModelError(nameof(model.StudentId),"この生徒の対象年月の月謝はすでに登録されています。");
            }
        }

        if (!ModelState.IsValid)
        {
            model.StudentOptions = await _context.Students
                .Where(student => !student.IsDeleted && student.Status == "在籍中")
                .OrderBy(student => student.Kana)
                .ThenBy(student => student.Name)
                .Select(student => new SelectListItem
                {
                    Value = student.Id.ToString(),
                    Text = student.Name
                })
                .AsNoTracking()
                .ToListAsync();

            var query = _context.Payments
                .Where(payment => !payment.IsDeleted
                    && payment.TargetYear == model.Year
                    && payment.TargetMonth == model.Month)
                .AsNoTracking();

            model.TotalCount = await query.CountAsync();

            model.TotalPages = (int)Math.Ceiling(model.TotalCount / (double)pageSize);

            if (model.TotalPages > 0 && model.CurrentPage > model.TotalPages)
            {
                model.CurrentPage = model.TotalPages;
            }

            model.Payments = await query
                .Include(payment => payment.Student)
                .OrderBy(payment => payment.Student.Kana)
                .ThenBy(payment => payment.Student.Name)
                .ThenBy(payment => payment.Id)
                .Skip((model.CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return View(model);
        }

        var now = DateTime.Now;

        var payment = new Payment
        {
            StudentId = model.StudentId!.Value,
            TargetYear = model.Year,
            TargetMonth = model.Month,
            Amount = model.Amount!.Value,
            Status = "未払い",
            PaidAt = null,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false,
            DeletedAt = null,
            Note = string.IsNullOrWhiteSpace(model.Note)
                ? null
                : model.Note.Trim()
        };

        _context.Payments.Add(payment);

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Index),
            new
            {
                year = model.Year,
                month = model.Month,
                page = model.CurrentPage
            });
    }

    [HttpGet]
    public async Task<IActionResult> Unpaid(int page = 1)
    {
        const int pageSize = 10;

        if (page < 1)
        {
            page = 1;
        }

        var query = _context.Payments
            .Where(payment => !payment.IsDeleted && payment.Status == "未払い" && !payment.Student.IsDeleted)
            .AsNoTracking();

        var unpaidCount = await query.CountAsync();

        var totalAmount = await query
            .SumAsync(payment => payment.Amount);

        var totalPages = (int)Math.Ceiling( unpaidCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var payments = await query
            .Include(payment => payment.Student)
            .OrderBy(payment => payment.TargetYear)
            .ThenBy(payment => payment.TargetMonth)
            .ThenBy(payment => payment.Student.Kana)
            .ThenBy(payment => payment.Student.Name)
            .ThenBy(payment => payment.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PaymentUnpaidViewModel
        {
            Payments = payments,
            UnpaidCount = unpaidCount,
            TotalAmount = totalAmount,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id,int year,int month,int page = 1)
    {
        if (year < 2000 || year > 2100 || month < 1 || month > 12)
        {
            return BadRequest();
        }

        if (page < 1)
        {
            page = 1;
        }

        var payment = await _context.Payments
            .FirstOrDefaultAsync(payment => payment.Id == id && !payment.IsDeleted);

        if (payment == null)
        {
            return NotFound();
        }

        if (payment.TargetYear != year || payment.TargetMonth != month)
        {
            return BadRequest();
        }

        if (payment.Status == "未払い")
        {
            payment.Status = "支払済み";
            payment.PaidAt = DateTime.Today;
        }
        else if (payment.Status == "支払済み")
        {
            payment.Status = "未払い";
            payment.PaidAt = null;
        }
        else
        {
            return BadRequest();
        }

        payment.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index),
            new
            {
                year,
                month,
                page
            });
    }
}