using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerSchoolManagement.Data;
using SoccerSchoolManagement.Models;
using SoccerSchoolManagement.ViewModels.Home;
using System.Diagnostics;

namespace SoccerSchoolManagement.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;

        var activeStudentCount = await _context.Students
            .CountAsync(student => !student.IsDeleted && student.Status == "在籍中");

        var unpaidCount = await _context.Payments
            .CountAsync(payment => !payment.IsDeleted && payment.Status == "未払い" && !payment.Student.IsDeleted);

        var unpaidTotalAmount = await _context.Payments
            .Where(payment => !payment.IsDeleted && payment.Status == "未払い" && !payment.Student.IsDeleted)
            .SumAsync(payment => payment.Amount);

        var lessonEntities = await _context.Lessons
            .Where(lesson => !lesson.IsDeleted)
            .Include(lesson => lesson.SoccerClass)
            .AsNoTracking()
            .ToListAsync();

        var todayLessons = lessonEntities
            .Where(lesson => lesson.LessonDate.Date == today)
            .Select(lesson => new DashboardLessonViewModel
            {
                Id = lesson.Id,
                ClassName = lesson.SoccerClass.Name,
                StartTime = lesson.StartTime,
                EndTime = lesson.EndTime,
                VenueName = lesson.VenueName,
                CoachName = lesson.CoachName,
                Status = lesson.Status
            })
            .ToList();

        todayLessons = todayLessons
            .OrderBy(lesson => lesson.StartTime)
            .ThenBy(lesson => lesson.ClassName)
            .ToList();

        var model = new DashboardViewModel
        {
            ActiveStudentCount = activeStudentCount,
            UnpaidCount = unpaidCount,
            UnpaidTotalAmount = unpaidTotalAmount,
            TodayLessons = todayLessons
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
