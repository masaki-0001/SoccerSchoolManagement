using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerSchoolManagement.Data;
using SoccerSchoolManagement.Models;
using SoccerSchoolManagement.ViewModels.Students;

namespace SoccerSchoolManagement.Controllers;

public class StudentsController : Controller
{
    private readonly AppDbContext _context;

    public StudentsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? keyword)
    {
        var normalizedKeyword = string.IsNullOrWhiteSpace(keyword)
            ? null
            : keyword.Trim();

        var query = _context.Students
            .Where(student => !student.IsDeleted)
            .AsNoTracking();

        if (normalizedKeyword is not null)
        {
            query = query.Where(student =>
                student.Name.Contains(normalizedKeyword)
                || student.Kana.Contains(normalizedKeyword));
        }

        var students = await query
            .OrderBy(student => student.Kana)
            .ToListAsync();

        var model = new StudentIndexViewModel
        {
            Keyword = normalizedKeyword,
            Students = students
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int? id)
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

        var now = DateTime.Now;

        var student = new Student
        {
            Name = model.Name,
            Kana = model.Kana,
            BirthDate = model.BirthDate!.Value,
            Grade = model.Grade,
            Gender = model.Gender,
            JerseyNumber = model.JerseyNumber,
            JoinedAt = model.JoinedAt!.Value,
            Status = model.Status,
            WithdrawnAt = model.WithdrawnAt,
            GuardianName = model.GuardianName,
            GuardianRelationship = model.GuardianRelationship,
            GuardianPhone = model.GuardianPhone,
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
    public async Task<IActionResult> Edit(int? id)
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
            GuardianRelationship =
                student.GuardianRelationship,
            GuardianPhone = student.GuardianPhone,
            GuardianEmail = student.GuardianEmail,
            Note = student.Note
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id , StudentEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(student => student.Id == id && !student.IsDeleted);

        if (student is null)
        {
            return NotFound();
        }

        student.Name = model.Name;
        student.Kana = model.Kana;
        student.BirthDate = model.BirthDate!.Value;
        student.Grade = model.Grade;
        student.Gender = model.Gender;
        student.JerseyNumber = model.JerseyNumber;
        student.JoinedAt = model.JoinedAt!.Value;
        student.Status = model.Status;
        student.WithdrawnAt = model.WithdrawnAt;
        student.GuardianName = model.GuardianName;
        student.GuardianRelationship =
            model.GuardianRelationship;
        student.GuardianPhone = model.GuardianPhone;
        student.GuardianEmail = model.GuardianEmail;
        student.Note = model.Note;
        student.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = student.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
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

        return View(student);
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
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

        return RedirectToAction(nameof(Index));
    }
}