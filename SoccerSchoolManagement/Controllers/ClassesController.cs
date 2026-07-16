using Microsoft.AspNetCore.Mvc;
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
            .FirstOrDefaultAsync(soccerClass =>
                soccerClass.Id == id.Value
                && !soccerClass.IsDeleted);

        if (soccerClass is null)
        {
            return NotFound();
        }

        var memberships = await _context.StudentClasses
            .Where(studentClass =>
                studentClass.ClassId == id.Value
                && !studentClass.IsDeleted)
            .Include(studentClass => studentClass.Student)
            .AsNoTracking()
            .ToListAsync();

        var model = new SoccerClassDetailsViewModel
        {
            SoccerClass = soccerClass,

            CurrentMemberships = memberships
                .Where(studentClass =>
                    !studentClass.EndDate.HasValue)
                .OrderBy(studentClass =>
                    studentClass.Student.Kana)
                .ThenBy(studentClass =>
                    studentClass.Student.Name)
                .ToList(),

            PastMemberships = memberships
                .Where(studentClass =>
                    studentClass.EndDate.HasValue)
                .OrderByDescending(studentClass =>
                    studentClass.EndDate!.Value)
                .ThenBy(studentClass =>
                    studentClass.Student.Kana)
                .ToList()
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
    public async Task<IActionResult> Create(
        SoccerClassCreateViewModel model)
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

        return RedirectToAction(
            nameof(Details),
            new { id = soccerClass.Id });
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
            .FirstOrDefaultAsync(soccerClass =>
                soccerClass.Id == id.Value
                && !soccerClass.IsDeleted);

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
    public async Task<IActionResult> Edit(
        int id,
        SoccerClassEditViewModel model)
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
            .FirstOrDefaultAsync(soccerClass =>
                soccerClass.Id == id
                && !soccerClass.IsDeleted);

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

        return RedirectToAction(
            nameof(Details),
            new { id = soccerClass.Id });
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}