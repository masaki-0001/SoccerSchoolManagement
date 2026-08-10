using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoccerSchoolManagement.Data;
using SoccerSchoolManagement.Models;
using SoccerSchoolManagement.ViewModels.Students;
using System.Text;

namespace SoccerSchoolManagement.Controllers;

public class StudentsController : Controller
{
    private readonly AppDbContext _context;

    public StudentsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? keyword, 
        string gradeFilter = "すべて",
        string statusFilter = "すべて",
        int? classFilter = null,
        int page = 1)
    {
        const int pageSize = 10;

        if (page < 1)
        {
            page = 1;
        }

        var normalizedKeyword = string.IsNullOrWhiteSpace(keyword)
            ? null
            : keyword.Trim();

        if (gradeFilter != "すべて" && !StudentFormOptions.Grades.Contains(gradeFilter))
        {
            gradeFilter = "すべて";
        }

        if (statusFilter != "すべて" && !StudentFormOptions.Statuses.Contains(statusFilter))
        {
            statusFilter = "すべて";
        }

        var classes = await _context.Classes
            .Where(soccerClass => !soccerClass.IsDeleted)
            .OrderBy(soccerClass => soccerClass.Name)
            .Select(soccerClass => new
    {
        soccerClass.Id,
        soccerClass.Name
    })

    .AsNoTracking()
    .ToListAsync();

        if (classFilter.HasValue && !classes.Any(soccerClass => soccerClass.Id == classFilter.Value))
        {
            classFilter = null;
        }

        var classOptions = classes
            .Select(soccerClass => new SelectListItem
            {
                Value = soccerClass.Id.ToString(),
                Text = soccerClass.Name
            })
            .ToList();

        var query = _context.Students
            .Where(student => !student.IsDeleted)
            .AsNoTracking();

        if (normalizedKeyword is not null)
        {
            query = query.Where(student => student.Name.Contains(normalizedKeyword) || student.Kana.Contains(normalizedKeyword));
        }

        if (gradeFilter != "すべて")
        {
            query = query.Where(student => student.Grade == gradeFilter);
        }

        if (statusFilter != "すべて")
        {
            query = query.Where(student => student.Status == statusFilter);
        }

        if (classFilter.HasValue)
        {
            query = query.Where(student =>
                student.StudentClasses.Any(studentClass =>
                    !studentClass.IsDeleted
                    && !studentClass.EndDate.HasValue
                    && studentClass.ClassId == classFilter.Value));
        }

        var totalCount = await query.CountAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var students = await query
            .OrderBy(student => student.Kana)
            .ThenBy(student => student.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new StudentIndexViewModel
        {
            Keyword = normalizedKeyword,
            GradeFilter = gradeFilter,
            StatusFilter = statusFilter,
            ClassFilter = classFilter,
            ClassOptions = classOptions,
            Students = students,
            CurrentPage = page,
            TotalPages = totalPages,
            TotalCount = totalCount
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(
        string? keyword,
        string gradeFilter = "すべて",
        string statusFilter = "すべて",
        int? classFilter = null)
    {
        var normalizedKeyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();

        if (gradeFilter != "すべて" && !StudentFormOptions.Grades.Contains(gradeFilter))
        {
            gradeFilter = "すべて";
        }

        if (statusFilter != "すべて" && !StudentFormOptions.Statuses.Contains(statusFilter))
        {
            statusFilter = "すべて";
        }

        if (classFilter.HasValue)
        {
            var classExists = await _context.Classes
                .AsNoTracking()
                .AnyAsync(soccerClass => soccerClass.Id == classFilter.Value && !soccerClass.IsDeleted);

            if (!classExists)
            {
                classFilter = null;
            }
        }

        var query = _context.Students
            .Where(student => !student.IsDeleted)
            .AsNoTracking();

        if (normalizedKeyword is not null)
        {
            query = query.Where(student => student.Name.Contains(normalizedKeyword) || student.Kana.Contains(normalizedKeyword));
        }

        if (gradeFilter != "すべて")
        {
            query = query.Where(student => student.Grade == gradeFilter);
        }

        if (statusFilter != "すべて")
        {
            query = query.Where(student => student.Status == statusFilter);
        }

        if (classFilter.HasValue)
        {
            query = query.Where(student =>
                student.StudentClasses.Any(studentClass =>
                    !studentClass.IsDeleted
                    && !studentClass.EndDate.HasValue
                    && studentClass.ClassId == classFilter.Value));
        }

        var students = await query
            .OrderBy(student => student.Kana)
            .ThenBy(student => student.Id)
            .ToListAsync();

        var csv = new StringBuilder();

        csv.AppendLine(
            "\"生徒ID\","
            + "\"氏名\","
            + "\"ふりがな\","
            + "\"生年月日\","
            + "\"学年\","
            + "\"性別\","
            + "\"背番号\","
            + "\"入会日\","
            + "\"在籍状況\","
            + "\"退会日\","
            + "\"保護者氏名\","
            + "\"保護者続柄\","
            + "\"保護者電話番号\","
            + "\"保護者メールアドレス\","
            + "\"備考\"");

        foreach (var student in students)
        {
            var values = new[]
            {
            student.Id.ToString(),
            student.Name,
            student.Kana,
            student.BirthDate.ToString("yyyy/MM/dd"),
            student.Grade,
            student.Gender,
            student.JerseyNumber?.ToString() ?? string.Empty,
            student.JoinedAt.ToString("yyyy/MM/dd"),
            student.Status,
            student.WithdrawnAt?.ToString("yyyy/MM/dd") ?? string.Empty,
            student.GuardianName,
            student.GuardianRelationship,
            FormatForExcelText(student.GuardianPhone),
            student.GuardianEmail ?? string.Empty,
            student.Note ?? string.Empty
        };

            csv.AppendLine( string.Join(",", values.Select(EscapeCsv)));
        }

        var encoding = new UTF8Encoding(true);

        var preamble = encoding.GetPreamble();
        var csvBytes = encoding.GetBytes(csv.ToString());

        var bytes = preamble
            .Concat(csvBytes)
            .ToArray();

        var fileName = $"生徒一覧_{DateTime.Now:yyyyMMdd}.csv";

        return File( bytes, "text/csv; charset=utf-8", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        int? id,
        string? keyword,
        string gradeFilter = "すべて",
        string statusFilter = "すべて",
        int? classFilter = null,
        int page = 1)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(student => student.Id == id.Value && !student.IsDeleted);

        if (student is null)
        {
            return NotFound();
        }

        ViewData["Keyword"] = keyword;
        ViewData["GradeFilter"] = gradeFilter;
        ViewData["StatusFilter"] = statusFilter;
        ViewData["ClassFilter"] = classFilter;
        ViewData["Page"] = page;

        return View(student);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new StudentCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var name = model.Name.Trim();
        var kana = model.Kana.Trim();
        var guardianPhone = model.GuardianPhone.Trim();

        var normalizedName = RemoveSpaces(name);
        var normalizedKana = RemoveSpaces(kana);

        var existingStudents = await _context.Students
            .Where(student => !student.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        var possibleDuplicate = false;

        foreach (var existingStudent in existingStudents)
        {
            var matchCount = 0;

            if (RemoveSpaces(existingStudent.Name) == normalizedName)
            {
                matchCount++;
            }

            if (RemoveSpaces(existingStudent.Kana) == normalizedKana)
            {
                matchCount++;
            }

            if (existingStudent.BirthDate == model.BirthDate!.Value)
            {
                matchCount++;
            }

            if (existingStudent.GuardianPhone.Trim() == guardianPhone)
            {
                matchCount++;
            }

            if (matchCount == 4)
            {
                if (existingStudent.Status == "退会済み")
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "同じ生徒が退会済みで登録されています。既存の生徒情報を編集して再入会してください。");
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "同じ生徒情報がすでに登録されています。");
                }

                return View(model);
            }

            if (matchCount >= 2)
            {
                possibleDuplicate = true;
            }
        }

        if (possibleDuplicate && !model.ConfirmDuplicate)
        {
            model.HasDuplicateWarning = true;

            ModelState.AddModelError(
                string.Empty,
                "登録済みの生徒と2項目以上一致しています。内容を確認してください。");

            return View(model);
        }

        var now = DateTime.Now;

        var student = new Student
        {
            Name = name,
            Kana = kana,
            BirthDate = model.BirthDate!.Value,
            Grade = model.Grade,
            Gender = model.Gender,
            JerseyNumber = model.JerseyNumber,
            JoinedAt = model.JoinedAt!.Value,
            Status = model.Status,
            WithdrawnAt = model.WithdrawnAt,
            GuardianName = model.GuardianName,
            GuardianRelationship = model.GuardianRelationship,
            GuardianPhone = guardianPhone,
            GuardianEmail = model.GuardianEmail,
            Note = model.Note,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false,
            DeletedAt = null
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
        int? id,
        string? keyword,
        string gradeFilter = "すべて",
        string statusFilter = "すべて",
        int? classFilter = null,
        int page = 1)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(student => student.Id == id.Value && !student.IsDeleted);

        if (student is null)
        {
            return NotFound();
        }

        var model = new StudentEditViewModel
        {
            Id = student.Id,
            Name = student.Name,
            Kana = student.Kana,
            BirthDate = student.BirthDate,
            Grade = student.Grade,
            Gender = student.Gender,
            JerseyNumber = student.JerseyNumber,
            JoinedAt = student.JoinedAt,
            Status = student.Status,
            WithdrawnAt = student.WithdrawnAt,
            GuardianName = student.GuardianName,
            GuardianRelationship = student.GuardianRelationship,
            GuardianPhone = student.GuardianPhone,
            GuardianEmail = student.GuardianEmail,
            Note = student.Note
        };

        ViewData["Keyword"] = keyword;
        ViewData["GradeFilter"] = gradeFilter;
        ViewData["StatusFilter"] = statusFilter;
        ViewData["ClassFilter"] = classFilter;
        ViewData["Page"] = page;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        StudentEditViewModel model,
        string? keyword,
        string gradeFilter = "すべて",
        string statusFilter = "すべて",
        int? classFilter = null,
        int page = 1)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewData["Keyword"] = keyword;
            ViewData["GradeFilter"] = gradeFilter;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["ClassFilter"] = classFilter;
            ViewData["Page"] = page;

            return View(model);
        }

        var name = model.Name.Trim();
        var kana = model.Kana.Trim();
        var guardianPhone = model.GuardianPhone.Trim();

        var normalizedName = RemoveSpaces(name);
        var normalizedKana = RemoveSpaces(kana);

        var existingStudents = await _context.Students
            .Where(student => !student.IsDeleted && student.Id != model.Id)
            .AsNoTracking()
            .ToListAsync();

        var possibleDuplicate = false;

        foreach (var existingStudent in existingStudents)
        {
            var matchCount = 0;

            if (RemoveSpaces(existingStudent.Name) == normalizedName)
            {
                matchCount++;
            }

            if (RemoveSpaces(existingStudent.Kana) == normalizedKana)
            {
                matchCount++;
            }

            if (existingStudent.BirthDate == model.BirthDate!.Value)
            {
                matchCount++;
            }

            if (existingStudent.GuardianPhone.Trim() == guardianPhone)
            {
                matchCount++;
            }

            if (matchCount == 4)
            {
                ModelState.AddModelError(string.Empty,"同じ生徒情報がすでに登録されています。");

                ViewData["Keyword"] = keyword;
                ViewData["GradeFilter"] = gradeFilter;
                ViewData["StatusFilter"] = statusFilter;
                ViewData["ClassFilter"] = classFilter;
                ViewData["Page"] = page;

                return View(model);
            }

            if (matchCount >= 2)
            {
                possibleDuplicate = true;
            }
        }

        if (possibleDuplicate && !model.ConfirmDuplicate)
        {
            model.HasDuplicateWarning = true;

            ModelState.AddModelError(string.Empty,"登録済みの生徒と2項目以上一致しています。内容を確認してください。");

            ViewData["Keyword"] = keyword;
            ViewData["GradeFilter"] = gradeFilter;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["ClassFilter"] = classFilter;
            ViewData["Page"] = page;

            return View(model);
        }

        var student = await _context.Students
             .FirstOrDefaultAsync(student => student.Id == id && !student.IsDeleted);

        if (student is null)
        {
            return NotFound();
        }

        var isNewlyWithdrawn = student.Status != "退会済み" && model.Status == "退会済み";

        var now = DateTime.Now;

        student.Name = name;
        student.Kana = kana;
        student.BirthDate = model.BirthDate!.Value;
        student.Grade = model.Grade;
        student.Gender = model.Gender;
        student.JerseyNumber = model.JerseyNumber;
        student.JoinedAt = model.JoinedAt!.Value;
        student.Status = model.Status;
        student.WithdrawnAt = model.WithdrawnAt;
        student.GuardianName = model.GuardianName;
        student.GuardianRelationship = model.GuardianRelationship;
        student.GuardianPhone = guardianPhone;
        student.GuardianEmail = model.GuardianEmail;
        student.Note = model.Note;
        student.UpdatedAt = now;

        if (isNewlyWithdrawn)
        {
            var currentMemberships = await _context.StudentClasses
                .Where(studentClass =>
                    studentClass.StudentId == student.Id
                    && !studentClass.EndDate.HasValue
                    && !studentClass.IsDeleted)
                .ToListAsync();

            foreach (var membership in currentMemberships)
            {
                membership.EndDate = model.WithdrawnAt!.Value.Date;
                membership.UpdatedAt = now;
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Index),
            new
            {
                keyword,
                gradeFilter,
                statusFilter,
                classFilter,
                page
            });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(
        int? id,
        string? keyword,
        string gradeFilter = "すべて",
        string statusFilter = "すべて",
        int? classFilter = null,
        int page = 1)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var student = await _context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(student => student.Id == id.Value && !student.IsDeleted);

        if (student is null)
        {
            return NotFound();
        }

        ViewData["Keyword"] = keyword;
        ViewData["GradeFilter"] = gradeFilter;
        ViewData["StatusFilter"] = statusFilter;
        ViewData["ClassFilter"] = classFilter;
        ViewData["Page"] = page;

        return View(student);
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id,
        string? keyword,
        string gradeFilter = "すべて",
        string statusFilter = "すべて",
        int? classFilter = null,
        int page = 1)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(student => student.Id == id && !student.IsDeleted);

        if (student is null)
        {
            return NotFound();
        }

        var now = DateTime.Now;

        student.IsDeleted = true;
        student.DeletedAt = now;
        student.UpdatedAt = now;

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Index),
            new
            {
                keyword,
                gradeFilter,
                statusFilter,
                classFilter,
                page
            });
    }
    private static string RemoveSpaces(string value)
    {
        return value
            .Replace(" ", "")
            .Replace("　", "");
    }
    private static string FormatForExcelText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return $"'{value}";
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