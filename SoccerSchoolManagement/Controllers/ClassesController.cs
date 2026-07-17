using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoccerSchoolManagement.Data;
using SoccerSchoolManagement.Models;
using SoccerSchoolManagement.ViewModels.Classes;

namespace SoccerSchoolManagement.Controllers;

public class ClassesController : Controller
{
    private readonly AppDbContext _context;

    public ClassesController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var classes = await _context.Classes
            .Where(soccerClass => !soccerClass.IsDeleted)
            .AsNoTracking()
            .ToListAsync();

        classes = classes
            .OrderBy(soccerClass => soccerClass.Name)
            .ThenBy(soccerClass => soccerClass.StartTime)
            .ToList();

        return View(classes);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var soccerClass = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(soccerClass => soccerClass.Id == id.Value
                && !soccerClass.IsDeleted);

        if (soccerClass is null)
        {
            return NotFound();
        }

        var memberships = await _context.StudentClasses
            .Where(studentClass => studentClass.ClassId == id.Value
                && !studentClass.IsDeleted
                && !studentClass.Student.IsDeleted)
            .Include(studentClass => studentClass.Student)
            .AsNoTracking()
            .ToListAsync();

        var upcomingLessons = await _context.Lessons
            .Where(lesson => lesson.ClassId == id.Value
                && !lesson.IsDeleted
                && lesson.LessonDate >= DateTime.Today)
            .AsNoTracking()
            .ToListAsync();

        upcomingLessons = upcomingLessons
            .OrderBy(lesson => lesson.LessonDate)
            .ThenBy(lesson => lesson.StartTime)
            .Take(5)
            .ToList();

        var model = new SoccerClassDetailsViewModel
        {
            SoccerClass = soccerClass,

            CurrentMemberships = memberships
                .Where(studentClass => !studentClass.EndDate.HasValue)
                .OrderBy(studentClass => studentClass.Student.Kana)
                .ThenBy(studentClass => studentClass.Student.Name)
                .ToList(),

            PastMemberships = memberships
                .Where(studentClass => studentClass.EndDate.HasValue)
                .OrderByDescending(studentClass => studentClass.EndDate)
                .ThenBy(studentClass => studentClass.Student.Kana)
                .ToList(),

            UpcomingLessons = upcomingLessons
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new SoccerClassCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SoccerClassCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var now = DateTime.Now;

        var soccerClass = new SoccerClass
        {
            Name = model.Name.Trim(),
            TargetGrade = model.TargetGrade.Trim(),
            DayOfWeek = model.DayOfWeek,
            StartTime = model.StartTime!.Value,
            EndTime = model.EndTime!.Value,
            VenueName = model.VenueName.Trim(),
            CoachName = model.CoachName.Trim(),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false,
            DeletedAt = null,
            Note = NormalizeOptionalText(model.Note)
        };

        _context.Classes.Add(soccerClass);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details) , new {id = soccerClass.Id});
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var soccerClass = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(soccerClass => soccerClass.Id == id.Value && !soccerClass.IsDeleted);

        if (soccerClass is null)
        {
            return NotFound();
        }

        var model = new SoccerClassEditViewModel
        {
            Id = soccerClass.Id,
            Name = soccerClass.Name,
            TargetGrade = soccerClass.TargetGrade,
            DayOfWeek = soccerClass.DayOfWeek,
            StartTime = soccerClass.StartTime,
            EndTime = soccerClass.EndTime,
            VenueName = soccerClass.VenueName,
            CoachName = soccerClass.CoachName,
            IsActive = soccerClass.IsActive,
            Note = soccerClass.Note
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id , SoccerClassEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var soccerClass = await _context.Classes
            .FirstOrDefaultAsync(soccerClass => soccerClass.Id == id && !soccerClass.IsDeleted);

        if (soccerClass is null)
        {
            return NotFound();
        }

        soccerClass.Name = model.Name.Trim();
        soccerClass.TargetGrade = model.TargetGrade.Trim();
        soccerClass.DayOfWeek = model.DayOfWeek;
        soccerClass.StartTime = model.StartTime!.Value;
        soccerClass.EndTime = model.EndTime!.Value;
        soccerClass.VenueName = model.VenueName.Trim();
        soccerClass.CoachName = model.CoachName.Trim();
        soccerClass.IsActive = model.IsActive;
        soccerClass.Note = NormalizeOptionalText(model.Note);
        soccerClass.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new {id = soccerClass.Id });
    }

    [HttpGet]
    public async Task<IActionResult> AddMembership(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var soccerClass = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(soccerClass => soccerClass.Id == id.Value && !soccerClass.IsDeleted);

        if (soccerClass is null)
        {
            return NotFound();
        }

        if (!soccerClass.IsActive)
        {
            TempData["ErrorMessage"] = "使用停止中のクラスには生徒を所属させられません。";

            return RedirectToAction( nameof(Details), new { id = soccerClass.Id });
        }

        var model = new StudentClassCreateViewModel
        {
            ClassId = soccerClass.Id,
            ClassName = soccerClass.Name,
            StartDate = DateTime.Today
        };

        await LoadStudentOptionsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMembership(int id , StudentClassCreateViewModel model)
    {
        if (id != model.ClassId)
        {
            return NotFound();
        }

        var soccerClass = await _context.Classes
            .AsNoTracking()
            .FirstOrDefaultAsync(soccerClass => soccerClass.Id == id && !soccerClass.IsDeleted);

        if (soccerClass is null)
        {
            return NotFound();
        }

        model.ClassName = soccerClass.Name;

        if (!soccerClass.IsActive)
        {
            TempData["ErrorMessage"] = "使用停止中のクラスには生徒を所属させられません。";

            return RedirectToAction(nameof(Details), new {id});
        }

        Student? selectedStudent = null;

        if (model.StudentId.HasValue)
        {
            selectedStudent = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(student => student.Id == model.StudentId.Value
                    && !student.IsDeleted 
                    && student.Status != "退会済み");

            if (selectedStudent is null)
            {
                ModelState.AddModelError(nameof(StudentClassCreateViewModel.StudentId) , "選択した生徒が見つからないか、退会済みです。");
            }
        }

        if (selectedStudent is not null)
        {
            var alreadyExists = await _context.StudentClasses
                .AnyAsync(studentClass => studentClass.StudentId == selectedStudent.Id
                    && studentClass.ClassId == id
                    && !studentClass.IsDeleted
                    && !studentClass.EndDate.HasValue);

            if (alreadyExists)
            {
                ModelState.AddModelError(nameof(StudentClassCreateViewModel.StudentId) , "この生徒は既にクラスへ所属しています。");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadStudentOptionsAsync(model);

            return View(model);
        }

        var now = DateTime.Now;

        var membership = new StudentClass
        {
            StudentId = model.StudentId!.Value,
            ClassId = id,
            StartDate = model.StartDate!.Value.Date,
            EndDate = null,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false,
            DeletedAt = null,
            Note = NormalizeOptionalText(model.Note)
        };

        _context.StudentClasses.Add(membership);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty,"所属情報を保存できませんでした。同じ生徒が既に所属していないか確認してください。");

            await LoadStudentOptionsAsync(model);

            return View(model);
        }

        return RedirectToAction(nameof(Details), new {id});
    }

    [HttpGet]
    public async Task<IActionResult> EndMembership(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var membership = await _context.StudentClasses
            .Include(studentClass => studentClass.Student)
            .Include(studentClass => studentClass.SoccerClass)
            .AsNoTracking()
            .FirstOrDefaultAsync(studentClass => studentClass.Id == id.Value
                && !studentClass.IsDeleted
                && !studentClass.Student.IsDeleted
                && !studentClass.SoccerClass.IsDeleted
                && !studentClass.EndDate.HasValue);

        if (membership is null)
        {
            return NotFound();
        }

        var model = new StudentClassEndViewModel
        {
            StudentClassId = membership.Id,
            ClassId = membership.ClassId,
            ClassName = membership.SoccerClass.Name,
            StudentName = membership.Student.Name,
            StartDate = membership.StartDate,
            EndDate = DateTime.Today
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EndMembership(
    int id,
    StudentClassEndViewModel model)
    {
        if (id != model.StudentClassId)
        {
            return NotFound();
        }

        var membership = await _context.StudentClasses
            .Include(studentClass => studentClass.Student)
            .Include(studentClass => studentClass.SoccerClass)
            .FirstOrDefaultAsync(studentClass => studentClass.Id == id
                && !studentClass.IsDeleted
                && !studentClass.Student.IsDeleted
                && !studentClass.SoccerClass.IsDeleted
                && !studentClass.EndDate.HasValue);

        if (membership is null)
        {
            return NotFound();
        }

        model.ClassId = membership.ClassId;
        model.ClassName = membership.SoccerClass.Name;
        model.StudentName = membership.Student.Name;
        model.StartDate = membership.StartDate;

        if (model.EndDate.HasValue && model.EndDate.Value.Date < membership.StartDate.Date)
        {
            ModelState.AddModelError(nameof(StudentClassEndViewModel.EndDate),"所属終了日は所属開始日以降にしてください。");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        membership.EndDate = model.EndDate!.Value.Date;
        membership.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new {id = membership.ClassId});
    }

    private async Task LoadStudentOptionsAsync(StudentClassCreateViewModel model)
    {
        var currentStudentIds = await _context.StudentClasses
            .Where(studentClass => studentClass.ClassId == model.ClassId
                && !studentClass.IsDeleted
                && !studentClass.EndDate.HasValue)
            .Select(studentClass =>
                studentClass.StudentId)
            .ToListAsync();

        var students = await _context.Students
            .Where(student => !student.IsDeleted
                && student.Status != "退会済み"
                && !currentStudentIds.Contains(student.Id))
            .AsNoTracking()
            .OrderBy(student => student.Kana)
            .ThenBy(student => student.Name)
            .ToListAsync();

        model.StudentOptions = students
            .Select(student => new SelectListItem
            {
                Value = student.Id.ToString(),
                Text = $"{student.Name}（{student.Grade}）"
            })
            .ToList();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}