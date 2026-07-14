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

    public async Task<IActionResult> Index()
    {
        var students = await _context.Students
            .Where(student => !student.IsDeleted)
            .OrderBy(student => student.Kana)
            .AsNoTracking()
            .ToListAsync();

        return View(students);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new StudentCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        StudentCreateViewModel model)
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
            JoinedAt = model.JoinedAt,
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
}