using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoccerSchoolManagement.Data;
using SoccerSchoolManagement.Models;
using SoccerSchoolManagement.ViewModels.Payments;
using System.Text;

namespace SoccerSchoolManagement.Controllers;

public class PaymentsController : Controller
{
    private readonly AppDbContext _context;

    private static readonly string[] ValidStatusFilters =
    {
        "すべて",
        "未払い",
        "支払済み"
    };

    public PaymentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int? year,
        int? month,
        string? keyword,
        string? studentKeyword,
        string statusFilter = "すべて",
        int page = 1)
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

        var nextMonthStart = new DateTime(targetYear, targetMonth, 1).AddMonths(1);

        if (!ValidStatusFilters.Contains(statusFilter))
        {
            statusFilter = "すべて";
        }

        var query = _context.Payments
            .Where(payment => !payment.IsDeleted 
                && !payment.Student.IsDeleted  
                && payment.TargetYear == targetYear 
                && payment.TargetMonth == targetMonth)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();

            query = query.Where(payment => payment.Student.Name.Contains(keyword) || payment.Student.Kana.Contains(keyword));
        }

        if (string.IsNullOrWhiteSpace(studentKeyword))
        {
            studentKeyword = null;
        }
        else
        {
            studentKeyword = studentKeyword.Trim();
        }

        if (statusFilter != "すべて")
        {
            query = query.Where(payment => payment.Status == statusFilter);
        }

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

        var registeredStudentIds = await _context.Payments
            .Where(payment =>
                !payment.IsDeleted
                && payment.TargetYear == targetYear
                && payment.TargetMonth == targetMonth)
            .Select(payment => payment.StudentId)
            .ToListAsync();

        var studentQuery = _context.Students
            .Where(student =>
                !student.IsDeleted
                && student.Status == "在籍中"
                && student.JoinedAt < nextMonthStart
                && !registeredStudentIds.Contains(student.Id))
           .AsNoTracking();

        if (studentKeyword is not null)
        {
            studentQuery = studentQuery.Where(student =>
                student.Name.Contains(studentKeyword)
                || student.Kana.Contains(studentKeyword));
        }

        var studentOptions = await studentQuery
            .OrderBy(student => student.Kana)
            .ThenBy(student => student.Name)
            .Select(student => new SelectListItem
            {
                Value = student.Id.ToString(),
                Text = student.Name
            })
            .ToListAsync();

        int? selectedStudentId = null;

        if (studentKeyword is not null && studentOptions.Count == 1)
        {
            selectedStudentId = int.Parse(studentOptions[0].Value);
        }

        var model = new PaymentIndexViewModel
        {
            Year = targetYear,
            Month = targetMonth,
            Keyword = keyword,
            StudentKeyword = studentKeyword,
            StatusFilter = statusFilter,
            StudentId = selectedStudentId,
            StudentOptions = studentOptions,
            Payments = payments,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(
        int? year,
        int? month,
        string? keyword,
        string statusFilter = "すべて")
    {
        var today = DateTime.Today;

        var targetYear = year ?? today.Year;
        var targetMonth = month ?? today.Month;

        if (targetYear < 2000 || targetYear > 2100 || targetMonth < 1 || targetMonth > 12)
        {
            return BadRequest();
        }

        if (!ValidStatusFilters.Contains(statusFilter))
        {
            statusFilter = "すべて";
        }

        var normalizedKeyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();

        var query = _context.Payments
            .Where(payment => !payment.IsDeleted
                && !payment.Student.IsDeleted
                && payment.TargetYear == targetYear
                && payment.TargetMonth == targetMonth)
            .AsNoTracking();

        if (normalizedKeyword is not null)
        {
            query = query.Where(payment => payment.Student.Name.Contains(normalizedKeyword) || payment.Student.Kana.Contains(normalizedKeyword));
        }

        if (statusFilter != "すべて")
        {
            query = query.Where(payment => payment.Status == statusFilter);
        }

        var payments = await query
            .Include(payment => payment.Student)
            .OrderBy(payment => payment.Student.Kana)
            .ThenBy(payment => payment.Student.Name)
            .ThenBy(payment => payment.Id)
            .ToListAsync();

        var csv = new StringBuilder();

        csv.AppendLine(
            "\"月謝ID\","
            + "\"対象年月\","
            + "\"生徒ID\","
            + "\"生徒氏名\","
            + "\"ふりがな\","
            + "\"請求額\","
            + "\"支払状況\","
            + "\"支払日\","
            + "\"備考\"");

        foreach (var payment in payments)
        {
            var values = new[]
            {
            payment.Id.ToString(),
            $"{payment.TargetYear}年{payment.TargetMonth:D2}",
            payment.StudentId.ToString(),
            payment.Student.Name,
            payment.Student.Kana,
            payment.Amount.ToString("0"),
            payment.Status,
            payment.PaidAt?.ToString("yyyy/MM/dd") ?? string.Empty,
            payment.Note ?? string.Empty
        };

            csv.AppendLine(
                string.Join(",", values.Select(EscapeCsv)));
        }

        var encoding = new UTF8Encoding(true);

        var preamble = encoding.GetPreamble();
        var csvBytes = encoding.GetBytes(csv.ToString());

        var bytes = preamble
            .Concat(csvBytes)
            .ToArray();

        var fileNamePrefix = statusFilter == "未払い" ? "未払い一覧" : "月謝一覧";

        var fileName = $"{fileNamePrefix}_{targetYear}{targetMonth:D2}_{DateTime.Now:yyyyMMdd}.csv";

        return File(bytes, "text/csv; charset=utf-8", fileName);
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

        var nextMonthStart = new DateTime(model.Year, model.Month, 1).AddMonths(1);

        if (!ValidStatusFilters.Contains(model.StatusFilter))
        {
            model.StatusFilter = "すべて";
        }

        if (string.IsNullOrWhiteSpace(model.Keyword))
        {
            model.Keyword = null;
        }
        else
        {
            model.Keyword = model.Keyword.Trim();
        }

        if (string.IsNullOrWhiteSpace(model.StudentKeyword))
        {
            model.StudentKeyword = null;
        }
        else
        {
            model.StudentKeyword = model.StudentKeyword.Trim();
        }

        if (model.CurrentPage < 1)
        {
            model.CurrentPage = 1;
        }

        if (ModelState.IsValid)
        {
            var studentExists = await _context.Students
                .AnyAsync(student => 
                    student.Id == model.StudentId 
                    && !student.IsDeleted 
                    && student.Status == "在籍中"
                    && student.JoinedAt < nextMonthStart);

            if (!studentExists)
            {
                ModelState.AddModelError(nameof(model.StudentId),"選択した生徒は月謝の登録対象ではありません。");
            }
        }

        if (ModelState.IsValid)
        {
            var paymentExists = await _context.Payments
                .AnyAsync(payment => 
                    !payment.IsDeleted 
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
            var registeredStudentIds = await _context.Payments
                .Where(payment =>
                    !payment.IsDeleted
                    && payment.TargetYear == model.Year
                    && payment.TargetMonth == model.Month)
                .Select(payment => payment.StudentId)
                .ToListAsync();

            var studentQuery = _context.Students
                .Where(student =>
                    !student.IsDeleted
                    && student.Status == "在籍中"
                    && student.JoinedAt < nextMonthStart
                    && !registeredStudentIds.Contains(student.Id))
                .AsNoTracking();

            if (model.StudentKeyword is not null)
            {
                studentQuery = studentQuery.Where(student =>
                    student.Name.Contains(model.StudentKeyword)
                    || student.Kana.Contains(model.StudentKeyword));
            }

            model.StudentOptions = await studentQuery
                .OrderBy(student => student.Kana)
                .ThenBy(student => student.Name)
                .Select(student => new SelectListItem
                {
                    Value = student.Id.ToString(),
                    Text = student.Name
                })
                .ToListAsync();

            var query = _context.Payments
                .Where(payment => 
                    !payment.IsDeleted 
                    && !payment.Student.IsDeleted 
                    && payment.TargetYear == model.Year 
                    && payment.TargetMonth == model.Month)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(model.Keyword))
            {
                query = query.Where(payment => payment.Student.Name.Contains(model.Keyword) || payment.Student.Kana.Contains(model.Keyword));
            }

            if (model.StatusFilter != "すべて")
            {
                query = query.Where(payment => payment.Status == model.StatusFilter);
            }

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

        TempData["SuccessMessage"] = "月謝を登録しました。";

        return RedirectToAction(
            nameof(Index),
            new
            {
                year = model.Year,
                month = model.Month,
                keyword = model.Keyword,
                statusFilter = model.StatusFilter,
                page = model.CurrentPage
            });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id, string? keyword, string statusFilter = "すべて", int page = 1)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        if (!ValidStatusFilters.Contains(statusFilter))
        {
            statusFilter = "すべて";
        }

        if (page < 1)
        {
            page = 1;
        }

        if (string.IsNullOrWhiteSpace(keyword))
        {
            keyword = null;
        }
        else
        {
            keyword = keyword.Trim();
        }

        var payment = await _context.Payments
            .Include(payment => payment.Student)
            .AsNoTracking()
            .FirstOrDefaultAsync(payment =>
                payment.Id == id.Value
                && !payment.IsDeleted
                && !payment.Student.IsDeleted);

        if (payment is null)
        {
            return NotFound();
        }

        var model = new PaymentEditViewModel
        {
            Id = payment.Id,
            StudentName = payment.Student.Name,
            TargetYear = payment.TargetYear,
            TargetMonth = payment.TargetMonth,
            Amount = payment.Amount,
            Note = payment.Note,
            ReturnYear = payment.TargetYear,
            ReturnMonth = payment.TargetMonth,
            Keyword = keyword,
            StatusFilter = statusFilter,
            Page = page
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PaymentEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ValidStatusFilters.Contains(model.StatusFilter))
        {
            model.StatusFilter = "すべて";
        }

        if (model.Page < 1)
        {
            model.Page = 1;
        }

        if (string.IsNullOrWhiteSpace(model.Keyword))
        {
            model.Keyword = null;
        }
        else
        {
            model.Keyword = model.Keyword.Trim();
        }

        var payment = await _context.Payments
            .Include(payment => payment.Student)
            .FirstOrDefaultAsync(payment =>
                payment.Id == id
                && !payment.IsDeleted
                && !payment.Student.IsDeleted);

        if (payment is null)
        {
            return NotFound();
        }

        model.StudentName = payment.Student.Name;
        model.TargetYear = payment.TargetYear;
        model.TargetMonth = payment.TargetMonth;
        model.ReturnYear = payment.TargetYear;
        model.ReturnMonth = payment.TargetMonth;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        payment.Amount = model.Amount!.Value;

        payment.Note = string.IsNullOrWhiteSpace(model.Note)
            ? null
            : model.Note.Trim();

        payment.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index),
            new
            {
                year = payment.TargetYear,
                month = payment.TargetMonth,
                keyword = model.Keyword,
                statusFilter = model.StatusFilter,
                page = model.Page
            });
    }

    [HttpGet]
    public async Task<IActionResult> Unpaid(int? year, int? month, int page = 1)
    {
        const int pageSize = 10;

        if (page < 1)
        {
            page = 1;
        }

        var today = DateTime.Today;

        var targetYear = year ?? today.Year;
        var targetMonth = month ?? today.Month;

        if (targetYear < 2000
            || targetYear > 2100
            || targetMonth < 1
            || targetMonth > 12)
        {
            return BadRequest();
        }

        var query = _context.Payments
            .Where(payment =>
                !payment.IsDeleted
                && !payment.Student.IsDeleted
                && payment.TargetYear == targetYear
                && payment.TargetMonth == targetMonth
                && payment.Status == "未払い")
            .AsNoTracking();

        var unpaidCount = await query.CountAsync();

        var totalAmount = await query.SumAsync(payment => payment.Amount);

        var totalPages =(int)Math.Ceiling(unpaidCount / (double)pageSize);

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

        var model = new PaymentUnpaidViewModel
        {
            Year = targetYear,
            Month = targetMonth,
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
    public async Task<IActionResult> ChangeStatus(int id,int year,int month, string? keyword, string statusFilter = "すべて", int page = 1)
    {
        if (year < 2000 || year > 2100 || month < 1 || month > 12)
        {
            return BadRequest();
        }

        if (page < 1)
        {
            page = 1;
        }

        if (!ValidStatusFilters.Contains(statusFilter))
        {
            statusFilter = "すべて";
        }

        if (string.IsNullOrWhiteSpace(keyword))
        {
            keyword = null;
        }
        else
        {
            keyword = keyword.Trim();
        }

        var payment = await _context.Payments
            .FirstOrDefaultAsync(payment => 
                payment.Id == id 
                && !payment.IsDeleted 
                && !payment.Student.IsDeleted);

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

        TempData["SuccessMessage"] = "支払状況を変更しました。";

        return RedirectToAction(
            nameof(Index),
            new
            {
                year,
                month,
                keyword,
                statusFilter,
                page
            });
    }
    private static string EscapeCsv(string? value)
    {
        var text = value ?? string.Empty;

        if (text.Length > 0
            && "=+-@\t\r\n".Contains(text[0]))
        {
            text = $"'{text}";
        }

        var escapedText = text.Replace("\"", "\"\"");

        return $"\"{escapedText}\"";
    }
}