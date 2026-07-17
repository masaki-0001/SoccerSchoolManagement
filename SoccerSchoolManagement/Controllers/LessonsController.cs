using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoccerSchoolManagement.Data;
using SoccerSchoolManagement.Models;
using SoccerSchoolManagement.ViewModels.Lessons;

namespace SoccerSchoolManagement.Controllers;

public class LessonsController : Controller
{
    private readonly AppDbContext _context;

    public LessonsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? year , int? month)
    {
        var today = DateTime.Today;

        var targetYear = year ?? today.Year;
        var targetMonth = month ?? today.Month;

        if (targetYear < 1
            || targetYear > 9999
            || targetMonth < 1
            || targetMonth > 12
            || targetYear == 9999 && targetMonth == 12)
        {
            return BadRequest();
        }

        var startDate = new DateTime(targetYear , targetMonth , 1);

        var endDate = startDate.AddMonths(1);

        var lessons = await _context.Lessons
            .Where(lesson => !lesson.IsDeleted
                && lesson.LessonDate >= startDate
                && lesson.LessonDate < endDate)
            .Include(lesson => lesson.SoccerClass)
            .AsNoTracking()
            .ToListAsync();

        lessons = lessons
            .OrderBy(lesson => lesson.LessonDate)
            .ThenBy(lesson => lesson.StartTime)
            .ThenBy(lesson => lesson.SoccerClass.Name)
            .ToList();

        var model = new LessonIndexViewModel
        {
            Year = targetYear,
            Month = targetMonth,
            Lessons = lessons
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? classId)
    {
        var model = new LessonCreateViewModel();

        if (classId.HasValue)
        {
            var soccerClass = await _context.Classes
                .AsNoTracking()
                .FirstOrDefaultAsync(soccerClass => soccerClass.Id == classId.Value
                    && !soccerClass.IsDeleted
                    && soccerClass.IsActive);

            if (soccerClass is null)
            {
                return NotFound();
            }

            model.ClassId = soccerClass.Id;
            model.StartTime = soccerClass.StartTime;
            model.EndTime = soccerClass.EndTime;
            model.VenueName = soccerClass.VenueName;
            model.CoachName = soccerClass.CoachName;
        }

        model.ClassOptions = await LoadClassOptionsAsync();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonCreateViewModel model)
    {
        if (model.ClassId.HasValue)
        {
            var classExists = await _context.Classes
                .AnyAsync(soccerClass => soccerClass.Id == model.ClassId.Value
                    && !soccerClass.IsDeleted
                    && soccerClass.IsActive);

            if (!classExists)
            {
                ModelState.AddModelError(nameof(model.ClassId) , "選択したクラスが見つからないか、使用停止中です。");
            }
        }

        if (model.ClassId.HasValue && model.LessonDate.HasValue && model.StartTime.HasValue)
        {
            var lessonDate = model.LessonDate.Value.Date;

            var duplicateExists =
                await _context.Lessons.AnyAsync(lesson => lesson.ClassId == model.ClassId.Value
                    && lesson.LessonDate == lessonDate
                    && lesson.StartTime == model.StartTime.Value
                    && !lesson.IsDeleted);

            if (duplicateExists)
            {
                ModelState.AddModelError(nameof(model.StartTime) , "同じクラス、開催日、開始時刻の練習予定が既に登録されています。");
            }
        }

        if (!ModelState.IsValid)
        {
            model.ClassOptions = await LoadClassOptionsAsync();

            return View(model);
        }

        var now = DateTime.Now;

        var lesson = new Lesson
        {
            ClassId = model.ClassId!.Value,
            LessonDate = model.LessonDate!.Value.Date,
            StartTime = model.StartTime!.Value,
            EndTime = model.EndTime!.Value,
            VenueName = model.VenueName.Trim(),
            CoachName = model.CoachName.Trim(),
            Status = model.Status,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false,
            DeletedAt = null,
            Note = NormalizeOptionalText(model.Note)
        };

        _context.Lessons.Add(lesson);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "練習予定を保存できませんでした。同じクラス、開催日、開始時刻の予定が既に登録されていないか確認してください。");

            model.ClassOptions = await LoadClassOptionsAsync();

            return View(model);
        }

        return RedirectToAction(nameof(Index),
            new
            {
                year = lesson.LessonDate.Year,
                month = lesson.LessonDate.Month
            });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound();
        }

        var lesson = await _context.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(lesson => lesson.Id == id.Value && !lesson.IsDeleted);

        if (lesson is null)
        {
            return NotFound();
        }

        var model = new LessonEditViewModel
        {
            Id = lesson.Id,
            ClassId = lesson.ClassId,
            LessonDate = lesson.LessonDate,
            StartTime = lesson.StartTime,
            EndTime = lesson.EndTime,
            VenueName = lesson.VenueName,
            CoachName = lesson.CoachName,
            Status = lesson.Status,
            Note = lesson.Note,
            ClassOptions = await LoadClassOptionsAsync(lesson.ClassId)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id , LessonEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(lesson => lesson.Id == id && !lesson.IsDeleted);

        if (lesson is null)
        {
            return NotFound();
        }

        if (model.ClassId.HasValue)
        {
            var classExists = await _context.Classes
                .AnyAsync(soccerClass =>
                    soccerClass.Id == model.ClassId.Value
                    && !soccerClass.IsDeleted
                    && (soccerClass.IsActive
                    || soccerClass.Id == lesson.ClassId));

            if (!classExists)
            {
                ModelState.AddModelError(nameof(model.ClassId) , "選択したクラスが見つからないか、使用停止中です。");
            }
        }

        if (model.ClassId.HasValue && model.LessonDate.HasValue && model.StartTime.HasValue)
        {
            var lessonDate = model.LessonDate.Value.Date;

            var duplicateExists =
                await _context.Lessons.AnyAsync(
                    existingLesson => existingLesson.Id != id
                        && existingLesson.ClassId == model.ClassId.Value
                        && existingLesson.LessonDate == lessonDate
                        && existingLesson.StartTime == model.StartTime.Value
                        && !existingLesson.IsDeleted);

            if (duplicateExists)
            {
                ModelState.AddModelError(nameof(model.StartTime), "同じクラス、開催日、開始時刻の練習予定が既に登録されています。");
            }
        }

        if (!ModelState.IsValid)
        {
            model.ClassOptions = await LoadClassOptionsAsync(lesson.ClassId);

            return View(model);
        }

        lesson.ClassId = model.ClassId!.Value;
        lesson.LessonDate = model.LessonDate!.Value.Date;
        lesson.StartTime = model.StartTime!.Value;
        lesson.EndTime = model.EndTime!.Value;
        lesson.VenueName = model.VenueName.Trim();
        lesson.CoachName = model.CoachName.Trim();
        lesson.Status = model.Status;
        lesson.Note = NormalizeOptionalText(model.Note);
        lesson.UpdatedAt = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(string.Empty, "練習予定を保存できませんでした。同じクラス、開催日、開始時刻の予定が既に登録されていないか確認してください。");

            model.ClassOptions = await LoadClassOptionsAsync(lesson.ClassId);

            return View(model);
        }

        return RedirectToAction(nameof(Index),
            new
            {
                year = lesson.LessonDate.Year,
                month = lesson.LessonDate.Month
            });
    }

    private async Task<List<SelectListItem>>LoadClassOptionsAsync(int? includedClassId = null)
    {
        var query = _context.Classes
            .Where(soccerClass => !soccerClass.IsDeleted);

        if (includedClassId.HasValue)
        {
            query = query.Where(soccerClass => soccerClass.IsActive || soccerClass.Id == includedClassId.Value);
        }
        else
        {
            query = query.Where(soccerClass => soccerClass.IsActive);
        }

        var classes = await query
            .AsNoTracking()
            .OrderBy(soccerClass => soccerClass.Name)
            .ToListAsync();

        return classes
            .Select(soccerClass => new SelectListItem
                {
                    Value =
                        soccerClass.Id.ToString(),

                    Text = soccerClass.IsActive
                        ? soccerClass.Name
                        : $"{soccerClass.Name}" + "（使用停止）"
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